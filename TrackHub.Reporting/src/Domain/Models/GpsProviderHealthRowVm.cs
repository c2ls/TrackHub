namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsProviderHealthRowVm(
    Guid OperatorId,
    string OperatorName,
    string Status,
    double UptimePercent,
    double? AvgLatencyMs,
    int FailureCount,
    DateTimeOffset? LastCheckAt,
    string? LastFailureCode);
