namespace TrackHub.Manager.Domain.Models;

public readonly record struct AccountFeatureVm(Guid AccountFeatureId, Guid AccountId, string FeatureKey, bool Enabled, string Tier, string Source, DateTimeOffset? EffectiveFrom, DateTimeOffset? EffectiveTo, string? ConfigurationJson, DateTimeOffset LastModified);
