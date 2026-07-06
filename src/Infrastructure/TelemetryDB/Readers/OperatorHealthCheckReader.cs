using Common.Application.Interfaces;
using TrackHub.Telemetry.Domain.Enums;
using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;

public sealed class OperatorHealthCheckReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IOperatorHealthCheckReader
{
    public async Task<IReadOnlyCollection<OperatorHealthCheckVm>> GetByOperatorAsync(Guid operatorId, int take, CancellationToken cancellationToken)
    {
        var op = await Context.Operators.Where(o => o.OperatorId == operatorId)
            .Select(o => new { o.AccountId }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Operator", $"{operatorId}");
        RequireAccountAccess(op.AccountId);
        var pageSize = Math.Clamp(take <= 0 ? 50 : take, 1, 500);
        return await Context.OperatorHealthChecks
            .Where(c => c.OperatorId == operatorId)
            .OrderByDescending(c => c.StartedAt)
            .Take(pageSize)
            .Select(c => new OperatorHealthCheckVm(c.OperatorHealthCheckId, c.AccountId, c.OperatorId,
                (OperatorHealthCheckType)c.CheckType, (OperatorHealthStatus)c.Status, c.LatencyMs,
                c.StartedAt, c.CompletedAt, c.ErrorCode, c.ErrorMessage, c.RetryCount, c.CorrelationId))
            .ToListAsync(cancellationToken);
    }

    public async Task<OperatorHealthSummaryVm> GetSummaryAsync(Guid operatorId, DateTimeOffset since, CancellationToken cancellationToken)
    {
        var op = await Context.Operators.Where(o => o.OperatorId == operatorId)
            .Select(o => new { o.AccountId }).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Operator", $"{operatorId}");
        RequireAccountAccess(op.AccountId);

        var checks = await Context.OperatorHealthChecks
            .Where(c => c.OperatorId == operatorId && c.StartedAt >= since)
            .Select(c => new { c.Status, c.LatencyMs, c.StartedAt, c.ErrorCode })
            .ToListAsync(cancellationToken);

        var total = checks.Count;
        var healthy = checks.Count(c => c.Status == (int)OperatorHealthStatus.Healthy);
        var degraded = checks.Count(c => c.Status == (int)OperatorHealthStatus.Degraded);
        var offline = checks.Count(c => c.Status == (int)OperatorHealthStatus.Offline);
        var failures = degraded + offline;
        var uptime = total == 0 ? 0d : Math.Round(100d * healthy / total, 2);
        var avgLatency = checks.Where(c => c.LatencyMs.HasValue).Select(c => (double)c.LatencyMs!.Value).DefaultIfEmpty().Average();
        var hasLatency = checks.Any(c => c.LatencyMs.HasValue);
        var last = checks.OrderByDescending(c => c.StartedAt).FirstOrDefault();
        var lastFailure = checks.Where(c => c.Status != (int)OperatorHealthStatus.Healthy)
            .OrderByDescending(c => c.StartedAt).FirstOrDefault();

        return new OperatorHealthSummaryVm(
            operatorId,
            since,
            total,
            healthy,
            degraded,
            offline,
            failures,
            uptime,
            hasLatency ? avgLatency : null,
            last is null ? null : last.StartedAt,
            lastFailure is null ? null : lastFailure.StartedAt,
            lastFailure?.ErrorCode);
    }

    // Derived from the telemetry tables (Slice B decision): Telemetry no longer maintains the
    // denormalized operator health/sync-summary columns, so the current health snapshot is computed
    // at read time — status/latency from the latest health check, sync timestamps from the sync runs,
    // faithfully mirroring how Manager stamped those columns.
    public async Task<OperatorHealthVm> GetLatestHealthAsync(Guid operatorId, CancellationToken cancellationToken)
    {
        var accountId = await Context.Operators.Where(o => o.OperatorId == operatorId)
            .Select(o => o.AccountId).FirstOrDefaultAsync(cancellationToken);
        if (accountId == Guid.Empty)
        {
            throw new NotFoundException("Operator", $"{operatorId}");
        }
        RequireAccountAccess(accountId);

        // Current status + latency: most recent health check.
        var latestCheck = await Context.OperatorHealthChecks
            .Where(c => c.OperatorId == operatorId)
            .OrderByDescending(c => c.StartedAt)
            .Select(c => new { c.Status, c.LatencyMs })
            .FirstOrDefaultAsync(cancellationToken);

        // Failure details: most recent degraded/offline health check.
        var latestFailureCheck = await Context.OperatorHealthChecks
            .Where(c => c.OperatorId == operatorId && (c.Status == (int)OperatorHealthStatus.Degraded || c.Status == (int)OperatorHealthStatus.Offline))
            .OrderByDescending(c => c.StartedAt)
            .Select(c => new { At = c.CompletedAt ?? c.StartedAt, c.ErrorCode, c.ErrorMessage })
            .FirstOrDefaultAsync(cancellationToken);

        // A run is "successful" when it Succeeded or PartiallySucceeded (mirrors the Manager summary).
        var lastSuccessfulSync = await Context.OperatorSyncRuns
            .Where(r => r.OperatorId == operatorId && (r.Result == (int)OperatorSyncResult.Succeeded || r.Result == (int)OperatorSyncResult.PartiallySucceeded))
            .OrderByDescending(r => r.StartedAt)
            .Select(r => (DateTimeOffset?)(r.CompletedAt ?? r.StartedAt))
            .FirstOrDefaultAsync(cancellationToken);

        // Failed runs also advance the "last failed" timestamp; the later of a failed run / failed
        // health check wins, matching Manager's last-writer-wins column.
        var lastFailedRun = await Context.OperatorSyncRuns
            .Where(r => r.OperatorId == operatorId && r.Result == (int)OperatorSyncResult.Failed)
            .OrderByDescending(r => r.StartedAt)
            .Select(r => (DateTimeOffset?)(r.CompletedAt ?? r.StartedAt))
            .FirstOrDefaultAsync(cancellationToken);

        // Device syncs record DevicesSeen; position syncs record PositionsRead.
        var lastDeviceSync = await Context.OperatorSyncRuns
            .Where(r => r.OperatorId == operatorId && r.DevicesSeen > 0)
            .OrderByDescending(r => r.StartedAt)
            .Select(r => (DateTimeOffset?)(r.CompletedAt ?? r.StartedAt))
            .FirstOrDefaultAsync(cancellationToken);

        var lastPositionSync = await Context.OperatorSyncRuns
            .Where(r => r.OperatorId == operatorId && r.PositionsRead > 0)
            .OrderByDescending(r => r.StartedAt)
            .Select(r => (DateTimeOffset?)(r.CompletedAt ?? r.StartedAt))
            .FirstOrDefaultAsync(cancellationToken);

        var status = latestCheck is null ? OperatorHealthStatus.Unknown : (OperatorHealthStatus)latestCheck.Status;

        return new OperatorHealthVm(operatorId, status,
            lastSuccessfulSync,
            Latest(latestFailureCheck?.At, lastFailedRun),
            lastDeviceSync,
            lastPositionSync,
            latestFailureCheck?.ErrorCode,
            latestFailureCheck?.ErrorMessage,
            latestCheck?.LatencyMs);
    }

    private static DateTimeOffset? Latest(DateTimeOffset? a, DateTimeOffset? b)
        => a.HasValue && b.HasValue ? (a.Value >= b.Value ? a : b) : (a ?? b);
}
