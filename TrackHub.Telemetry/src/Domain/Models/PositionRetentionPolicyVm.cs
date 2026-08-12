namespace TrackHub.Telemetry.Domain.Models;

public readonly record struct PositionRetentionPolicyVm(
    bool HistoryEnabled,
    int RetentionDays,
    string EffectiveSource);
