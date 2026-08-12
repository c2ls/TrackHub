namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsProviderSyncHistoryRowVm(
    Guid OperatorId,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    string Trigger,
    string Result,
    int DevicesSynced,
    int Added,
    int Updated,
    int Removed,
    int Ignored,
    int FailedDevices,
    int PositionsRetrieved,
    string? CorrelationId);
