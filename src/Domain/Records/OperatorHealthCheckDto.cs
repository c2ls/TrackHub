using TrackHub.Telemetry.Domain.Enums;

namespace TrackHub.Telemetry.Domain.Records;

public readonly record struct OperatorHealthCheckDto(
    Guid AccountId,
    Guid OperatorId,
    OperatorHealthCheckType CheckType,
    OperatorHealthStatus Status,
    int? LatencyMs,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string? ErrorCode,
    string? ErrorMessage,
    int RetryCount,
    string? CorrelationId);
