namespace TrackHub.Telemetry.Domain.Models;

/// <summary>
/// Platform-wide (unscoped) recency of the SyncWorker's observable side effects. The worker is a
/// plain Generic Host with no HTTP listener, so its liveness is derived from the rows it writes
/// every cycle rather than from a health endpoint (spec 28 ST-04).
/// <para>
/// <see cref="HasEnabledGpsIntegration"/> distinguishes "the worker is down" from "there is nothing
/// to sync": Telemetry can only see account-level <c>gps.integration</c> enablement (per-operator
/// enablement lives in Manager), which is the correct granularity for that question.
/// </para>
/// </summary>
public readonly record struct PlatformSyncActivityVm(
    DateTimeOffset? LastSyncRunAt,
    DateTimeOffset? LastHealthCheckAt,
    int SyncRunsLastHour,
    int HealthChecksLastHour,
    bool HasEnabledGpsIntegration);
