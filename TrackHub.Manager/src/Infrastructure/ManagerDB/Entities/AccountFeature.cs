using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class AccountFeature(Guid accountId, string featureKey, bool enabled, string tier, string source, DateTimeOffset? effectiveFrom, DateTimeOffset? effectiveTo, string? configurationJson) : BaseAuditableEntity
{
    public Guid AccountFeatureId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public string FeatureKey { get; set; } = featureKey;
    public bool Enabled { get; set; } = enabled;
    public string Tier { get; set; } = tier;
    public string Source { get; set; } = source;
    public DateTimeOffset? EffectiveFrom { get; set; } = effectiveFrom;
    public DateTimeOffset? EffectiveTo { get; set; } = effectiveTo;
    public string? ConfigurationJson { get; set; } = configurationJson;
}
