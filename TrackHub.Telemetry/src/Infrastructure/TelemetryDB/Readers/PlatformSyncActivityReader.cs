using Common.Domain.Constants;
using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;

/// <summary>
/// Derives SyncWorker liveness from data recency (spec 28 ST-04). The worker is single-instance, so
/// any recent row proves the process is alive. Intentionally unscoped: it takes no principal and does
/// not derive from <c>AccountScopedDataAccess</c> — the caller is Administrator-only by authorization.
/// </summary>
public sealed class PlatformSyncActivityReader(IApplicationDbContext context) : IPlatformSyncActivityReader
{
    public async Task<PlatformSyncActivityVm> GetAsync(DateTimeOffset since, CancellationToken cancellationToken)
    {
        var lastSyncRunAt = await context.OperatorSyncRuns
            .OrderByDescending(x => x.StartedAt)
            .Select(x => (DateTimeOffset?)x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var lastHealthCheckAt = await context.OperatorHealthChecks
            .OrderByDescending(x => x.StartedAt)
            .Select(x => (DateTimeOffset?)x.StartedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var syncRunsLastHour = await context.OperatorSyncRuns
            .CountAsync(x => x.StartedAt >= since, cancellationToken);

        var healthChecksLastHour = await context.OperatorHealthChecks
            .CountAsync(x => x.StartedAt >= since, cancellationToken);

        // "Is there anything to sync at all?" Telemetry sees account-level feature enablement only
        // (per-operator Enabled lives in Manager, and spec 28 forbids adding an inter-service call
        // for it). An account with gps.integration on AND at least one operator is the honest
        // Telemetry-side answer.
        var now = DateTimeOffset.UtcNow;
        var enabledAccountIds = context.AccountFeatures
            .Where(f => f.FeatureKey == FeatureKeys.GpsIntegration
                && f.Enabled
                && (f.EffectiveFrom == null || f.EffectiveFrom <= now)
                && (f.EffectiveTo == null || now < f.EffectiveTo))
            .Select(f => f.AccountId);

        var hasEnabledGpsIntegration = await context.Operators
            .AnyAsync(o => enabledAccountIds.Contains(o.AccountId), cancellationToken);

        return new PlatformSyncActivityVm(lastSyncRunAt, lastHealthCheckAt, syncRunsLastHour, healthChecksLastHour, hasEnabledGpsIntegration);
    }
}
