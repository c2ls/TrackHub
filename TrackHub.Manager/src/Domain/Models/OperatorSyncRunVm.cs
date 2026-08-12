using TrackHub.Manager.Domain.Enums;

namespace TrackHub.Manager.Domain.Models;

public readonly record struct OperatorSyncRunVm(
    Guid OperatorSyncRunId,
    Guid AccountId,
    Guid OperatorId,
    SyncTriggerType TriggerType,
    OperatorSyncResult Result,
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
