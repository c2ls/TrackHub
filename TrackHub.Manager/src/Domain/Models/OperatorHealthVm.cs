using TrackHub.Manager.Domain.Enums;

namespace TrackHub.Manager.Domain.Models;

public readonly record struct OperatorHealthVm(
    Guid OperatorId,
    OperatorHealthStatus HealthStatus,
    DateTimeOffset? LastSuccessfulSyncAt,
    DateTimeOffset? LastFailedSyncAt,
    DateTimeOffset? LastDeviceSyncAt,
    DateTimeOffset? LastPositionSyncAt,
    string? LastFailureCode,
    string? LastFailureMessage,
    int? LastLatencyMs);

public readonly record struct OperatorHealthSummaryVm(
    Guid OperatorId,
    DateTimeOffset Since,
    int TotalChecks,
    int HealthyChecks,
    int DegradedChecks,
    int OfflineChecks,
    int FailureCount,
    double UptimePercent,
    double? AverageLatencyMs,
    DateTimeOffset? LastCheckAt,
    DateTimeOffset? LastFailureAt,
    string? LastFailureCode);

public readonly record struct OperatorHealthCheckVm(
    Guid OperatorHealthCheckId,
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
