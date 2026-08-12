namespace TrackHub.Manager.Domain.Models;

public readonly record struct PositionRetentionPolicyVm(
    bool HistoryEnabled,
    int RetentionDays,
    string EffectiveSource);
