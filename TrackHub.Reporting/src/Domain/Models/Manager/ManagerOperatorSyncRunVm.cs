namespace TrackHub.Reporting.Domain.Models.Manager;

public readonly record struct ManagerOperatorSyncRunVm(
    Guid OperatorSyncRunId,
    Guid AccountId,
    Guid OperatorId,
    string TriggerType,
    string Result,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    int DevicesSeen,
    int DevicesAdded,
    int DevicesUpdated,
    int DevicesRemoved,
    int DevicesIgnored,
    int PositionsRead,
    int PositionsAccepted,
    int PositionsRejected,
    string? ErrorCode,
    string? ErrorMessage,
    string? CorrelationId);
