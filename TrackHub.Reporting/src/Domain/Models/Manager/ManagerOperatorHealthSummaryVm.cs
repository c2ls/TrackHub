namespace TrackHub.Reporting.Domain.Models.Manager;

public readonly record struct ManagerOperatorHealthSummaryVm(
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
