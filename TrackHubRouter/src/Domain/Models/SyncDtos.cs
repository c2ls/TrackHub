namespace TrackHub.Router.Domain.Models;

public readonly record struct OperatorSyncRunDto(
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

public readonly record struct OperatorHealthCheckDto(
    Guid AccountId,
    Guid OperatorId,
    string CheckType,
    string Status,
    int? LatencyMs,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorCode,
    string? ErrorMessage,
    int RetryCount,
    string? CorrelationId);

public readonly record struct SynchronizedDeviceDto(
    Guid AccountId,
    Guid OperatorId,
    string Serial,
    string Name,
    int Identifier,
    string? ProviderDisplayName,
    short DeviceTypeId,
    string? Description,
    string? ProviderMetadataHash,
    string? ProviderStatus);

public readonly record struct AlertEventDto(
    Guid AccountId,
    string EventType,
    string Severity,
    string SourceModule,
    string ResourceType,
    string ResourceId,
    string Status,
    string? PayloadJson,
    string DeduplicationKey);
