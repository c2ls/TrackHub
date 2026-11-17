namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsProviderSyncHistoryRowVm(
    string Operator,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    [property: ReportValues("SyncTrigger")] string Trigger,
    [property: ReportValues("SyncResult")] string Result,
    int DevicesSynced,
    int Added,
    int Updated,
    int Removed,
    int Ignored,
    int FailedDevices,
    int PositionsRetrieved,
    string? CorrelationId);
