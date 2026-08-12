namespace TrackHub.Reporting.Domain.Models;

public readonly record struct GpsSyncStatisticsRowVm(
    DateTimeOffset Date,
    string Operator,
    int Runs,
    int Successes,
    int Failures,
    double AvgDurationMs,
    int TotalPositions);
