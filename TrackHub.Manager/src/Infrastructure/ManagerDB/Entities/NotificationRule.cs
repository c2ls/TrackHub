using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class NotificationRule(Guid accountId, string ruleKey, string ruleType, bool enabled, string triggerEvent, string recipientSelector, string channelsJson, string? throttlingJson, string? configurationJson) : BaseAuditableEntity
{
    public Guid NotificationRuleId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public string RuleKey { get; set; } = ruleKey;
    public string RuleType { get; set; } = ruleType;
    public bool Enabled { get; set; } = enabled;
    public string TriggerEvent { get; set; } = triggerEvent;
    public string RecipientSelector { get; set; } = recipientSelector;
    public string ChannelsJson { get; set; } = channelsJson;
    public string? ThrottlingJson { get; set; } = throttlingJson;
    public string? ConfigurationJson { get; set; } = configurationJson;
}
