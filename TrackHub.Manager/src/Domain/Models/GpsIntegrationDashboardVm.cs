namespace TrackHub.Manager.Domain.Models;

public readonly record struct GpsIntegrationDashboardVm(
    int OperatorsTotal,
    int OperatorsEnabled,
    int OperatorsHealthy,
    int OperatorsDegraded,
    int OperatorsOffline,
    int DevicesTotal,
    int DevicesNew,
    int DevicesAvailable,
    int DevicesAssigned,
    int DevicesIgnored,
    int DevicesRemoved,
    int RecentlyAddedDevicesLast24h,
    int UnassignedDevicesCount,
    int SyncRunsSucceededLast24h,
    int SyncRunsFailedLast24h,
    DateTimeOffset? LastAutomaticSyncAt,
    DateTimeOffset? LastManualSyncAt,
    double AverageSyncDurationSeconds,
    IReadOnlyCollection<DeviceProviderStatusCountVm> DeviceCountsByProviderStatus);
