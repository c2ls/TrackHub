using Common.Application.Interfaces;
using Common.Domain.Constants;
using Common.Domain.Helpers;
using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class OperatorReader(
    IApplicationDbContext context,
    ICurrentPrincipal principal,
    IIdentityService identityService)
    : AccountScopedDataAccess(context, principal), IOperatorReader
{
    // The operator health/failure/position-sync summary is DERIVED from the telemetry tables at read
    // time: after the extraction those rows are written by the Router into the
    // telemetry-owned operator_health_checks / operator_sync_runs, and the denormalized operator
    // columns are no longer maintained. The device-sync and manual-sync timestamps
    // (LastSuccessfulSyncAt, LastDeviceSyncAt, LastManualSyncAt) remain Manager-owned bookkeeping.
    private readonly record struct DerivedSummary(
        OperatorHealthStatus HealthStatus,
        int? LatencyMs,
        string? FailureCode,
        string? FailureMessage,
        DateTimeOffset? LastFailedSyncAt,
        DateTimeOffset? LastPositionSyncAt,
        DateTimeOffset? LastHealthCheckAt);

    private static OperatorVm Map(Entities.Operator o, bool includeCredential, DerivedSummary summary)
        => new(
            o.OperatorId,
            o.Name,
            o.Description,
            o.PhoneNumber,
            o.EmailAddress,
            o.Address,
            o.ContactName,
            (ProtocolType)o.ProtocolType,
            o.ProtocolType,
            o.AccountId,
            o.LastModified,
            includeCredential && o.Credential != null ? new CredentialTokenVm(
                o.Credential.CredentialId,
                o.Credential.Uri,
                o.Credential.Username,
                o.Credential.Password,
                o.Credential.Salt,
                o.Credential.Key,
                o.Credential.Key2,
                o.Credential.Token,
                o.Credential.TokenExpiration,
                o.Credential.RefreshToken,
                o.Credential.RefreshTokenExpiration) : null,
            o.Enabled,
            o.SyncIntervalMinutes,
            summary.HealthStatus,
            o.LastSuccessfulSyncAt,
            summary.LastFailedSyncAt,
            o.LastManualSyncAt,
            o.LastDeviceSyncAt,
            summary.LastPositionSyncAt,
            summary.FailureCode,
            summary.FailureMessage,
            summary.LatencyMs,
            summary.LastHealthCheckAt);

    private async Task<DerivedSummary> DeriveSummaryAsync(Guid operatorId, CancellationToken cancellationToken)
        => (await DeriveSummariesAsync([operatorId], cancellationToken)).GetValueOrDefault(operatorId);

    private async Task<Dictionary<Guid, DerivedSummary>> DeriveSummariesAsync(IReadOnlyCollection<Guid> operatorIds, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Guid, DerivedSummary>();
        if (operatorIds.Count == 0)
        {
            return result;
        }

        // Latest health check per operator -> status, latency, timestamp.
        var latestCheckTimes = await Context.OperatorHealthChecks
            .Where(c => operatorIds.Contains(c.OperatorId))
            .GroupBy(c => c.OperatorId)
            .Select(g => new { OperatorId = g.Key, Ts = g.Max(x => x.StartedAt) })
            .ToListAsync(cancellationToken);
        var latestCheckTsSet = latestCheckTimes.Select(x => x.Ts).Distinct().ToList();
        var latestCheckRows = latestCheckTsSet.Count == 0
            ? []
            : await Context.OperatorHealthChecks
                .Where(c => operatorIds.Contains(c.OperatorId) && latestCheckTsSet.Contains(c.StartedAt))
                .Select(c => new { c.OperatorId, c.StartedAt, c.Status, c.LatencyMs })
                .ToListAsync(cancellationToken);

        // Latest degraded/offline health check per operator -> failure code/message/time.
        var failStatuses = new[] { (int)OperatorHealthStatus.Degraded, (int)OperatorHealthStatus.Offline };
        var latestFailTimes = await Context.OperatorHealthChecks
            .Where(c => operatorIds.Contains(c.OperatorId) && failStatuses.Contains(c.Status))
            .GroupBy(c => c.OperatorId)
            .Select(g => new { OperatorId = g.Key, Ts = g.Max(x => x.StartedAt) })
            .ToListAsync(cancellationToken);
        var latestFailTsSet = latestFailTimes.Select(x => x.Ts).Distinct().ToList();
        var latestFailRows = latestFailTsSet.Count == 0
            ? []
            : await Context.OperatorHealthChecks
                .Where(c => operatorIds.Contains(c.OperatorId) && latestFailTsSet.Contains(c.StartedAt) && failStatuses.Contains(c.Status))
                .Select(c => new { c.OperatorId, c.StartedAt, c.CompletedAt, c.ErrorCode, c.ErrorMessage })
                .ToListAsync(cancellationToken);

        // Sync-run timestamps: last failed run and last position sync (Router-recorded).
        var lastFailedRun = await Context.OperatorSyncRuns
            .Where(r => operatorIds.Contains(r.OperatorId) && r.Result == (int)OperatorSyncResult.Failed)
            .GroupBy(r => r.OperatorId)
            .Select(g => new { OperatorId = g.Key, At = g.Max(x => x.StartedAt) })
            .ToDictionaryAsync(x => x.OperatorId, x => (DateTimeOffset?)x.At, cancellationToken);
        var lastPositionSync = await Context.OperatorSyncRuns
            .Where(r => operatorIds.Contains(r.OperatorId) && r.PositionsRead > 0)
            .GroupBy(r => r.OperatorId)
            .Select(g => new { OperatorId = g.Key, At = g.Max(x => x.StartedAt) })
            .ToDictionaryAsync(x => x.OperatorId, x => (DateTimeOffset?)x.At, cancellationToken);

        foreach (var operatorId in operatorIds.Distinct())
        {
            var latestTs = latestCheckTimes.FirstOrDefault(x => x.OperatorId == operatorId)?.Ts;
            var latestCheck = latestTs is null ? null : latestCheckRows.FirstOrDefault(r => r.OperatorId == operatorId && r.StartedAt == latestTs);
            var failTs = latestFailTimes.FirstOrDefault(x => x.OperatorId == operatorId)?.Ts;
            var latestFail = failTs is null ? null : latestFailRows.FirstOrDefault(r => r.OperatorId == operatorId && r.StartedAt == failTs);

            var failedFromCheck = latestFail is null ? (DateTimeOffset?)null : latestFail.CompletedAt ?? latestFail.StartedAt;
            var failedFromRun = lastFailedRun.GetValueOrDefault(operatorId);
            var lastFailedSync = failedFromCheck.HasValue && failedFromRun.HasValue
                ? (failedFromCheck.Value >= failedFromRun.Value ? failedFromCheck : failedFromRun)
                : failedFromCheck ?? failedFromRun;

            result[operatorId] = new DerivedSummary(
                latestCheck is null ? OperatorHealthStatus.Unknown : (OperatorHealthStatus)latestCheck.Status,
                latestCheck?.LatencyMs,
                latestFail?.ErrorCode,
                latestFail?.ErrorMessage,
                lastFailedSync,
                lastPositionSync.GetValueOrDefault(operatorId),
                latestTs);
        }

        return result;
    }

    private async Task<bool> CanIncludeCredentialsAsync(CancellationToken cancellationToken)
    {
        if (Principal.PrincipalType == PrincipalType.ServiceClient)
        {
            return true;
        }

        if (Principal.PrincipalType != PrincipalType.User || !Principal.UserId.HasValue)
        {
            return false;
        }

        var userId = Principal.UserId.Value;
        // Single combined role+policy decision (cached in Common's IdentityService) instead of
        // two sequential Security round trips per operator read.
        return await identityService.AuthorizeUserAsync(userId, Resources.Credentials, Actions.Custom, cancellationToken);
    }

    public async Task<OperatorVm> GetOperatorAsync(Guid id, CancellationToken cancellationToken)
    {
        var op = await Context.Operators
            .Include(o => o.Credential)
            .FirstOrDefaultAsync(o => o.OperatorId == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Entities.Operator), id.ToString());

        // This read can carry decrypted credential material (see CanIncludeCredentialsAsync) and the
        // query carries no top-level AccountId, so it is bound to the caller's tenant here. A global
        // service client — the Router's sync, replay and provider flows — still satisfies this.
        RequireAccountAccess(op.AccountId);

        var summary = await DeriveSummaryAsync(id, cancellationToken);
        var includeCredentials = await CanIncludeCredentialsAsync(cancellationToken);
        return Map(op, includeCredentials, summary);
    }

    // Resolves the operator that owns the transporter's device. Restored for the Router's
    // position read/replay flows (operatorByTransporter); delegates to GetOperatorAsync so
    // credential gating and the derived telemetry summary behave exactly like the by-id read.
    public async Task<OperatorVm> GetOperatorByTransporterAsync(Guid transporterId, CancellationToken cancellationToken)
    {
        var operatorId = await Context.TransporterDeviceAssignments
            .Where(a => a.TransporterId == transporterId && a.Status == (int)AssignmentStatus.Active)
            .OrderByDescending(a => a.IsPrimary)
            .ThenBy(a => a.Priority)
            .Select(a => (Guid?)a.Device.OperatorId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Entities.Operator), transporterId.ToString());

        return await GetOperatorAsync(operatorId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<OperatorVm>> GetOperatorsAsync(Filters filters, CancellationToken cancellationToken)
    {
        var query = Context.Operators.Include(o => o.Credential).AsQueryable();
        query = filters.Apply(query);
        var items = await query.ToListAsync(cancellationToken);
        var summaries = await DeriveSummariesAsync(items.Select(o => o.OperatorId).ToList(), cancellationToken);
        var includeCredentials = await CanIncludeCredentialsAsync(cancellationToken);
        return items.Select(o => Map(o, includeCredentials, summaries.GetValueOrDefault(o.OperatorId))).ToList();
    }

    // Paged counterpart of GetOperatorsAsync. The health/failure summary is derived per page, so the
    // window is applied in SQL before any telemetry derivation runs.
    public async Task<OperatorsPageVm> GetOperatorsPageAsync(Filters filters, int skip, int take, string? search, CancellationToken cancellationToken)
    {
        var query = filters.Apply(Context.Operators.Include(o => o.Credential).AsQueryable());
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = SearchPattern.Contains(search);
            query = query.Where(o => EF.Functions.ILike(o.Name, term, SearchPattern.Escape));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        // Operator names are not unique, so OperatorId completes the total order Skip/Take needs.
        var items = await query
            .OrderBy(o => o.Name)
            .ThenBy(o => o.OperatorId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        var summaries = await DeriveSummariesAsync(items.Select(o => o.OperatorId).ToList(), cancellationToken);
        var includeCredentials = await CanIncludeCredentialsAsync(cancellationToken);
        return new OperatorsPageVm(
            items.Select(o => Map(o, includeCredentials, summaries.GetValueOrDefault(o.OperatorId))).ToList(),
            totalCount);
    }

    // Minimal operator projection for select controls: no credential material, no derived telemetry
    // summary, unpaged, capped by the caller.
    public async Task<IReadOnlyCollection<OperatorLookupVm>> GetOperatorLookupByAccountAsync(Guid accountId, int fetchSize, CancellationToken cancellationToken)
    {
        var scoped = RequireAccountAccess(accountId);
        return await Context.Operators
            .Where(o => o.AccountId == scoped)
            .OrderBy(o => o.Name)
            .ThenBy(o => o.OperatorId)
            .Take(fetchSize)
            .Select(o => new OperatorLookupVm(o.OperatorId, o.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OperatorVm>> GetOperatorsByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var accountId = await Context.Users
            .Where(u => u.UserId == userId)
            .Select(u => u.AccountId)
            .FirstAsync(cancellationToken);

        var items = await Context.Operators
            .Include(o => o.Credential)
            .Where(o => o.AccountId == accountId)
            .ToListAsync(cancellationToken);
        var summaries = await DeriveSummariesAsync(items.Select(o => o.OperatorId).ToList(), cancellationToken);
        var includeCredentials = await CanIncludeCredentialsAsync(cancellationToken);
        return items.Select(o => Map(o, includeCredentials, summaries.GetValueOrDefault(o.OperatorId))).ToList();
    }
}
