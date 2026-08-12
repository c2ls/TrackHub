namespace TrackHub.Manager.Domain.Records;

public readonly record struct AccountFeatureDto(Guid AccountId, string FeatureKey, bool Enabled, string Tier, string Source, DateTimeOffset? EffectiveFrom, DateTimeOffset? EffectiveTo, string? ConfigurationJson);
