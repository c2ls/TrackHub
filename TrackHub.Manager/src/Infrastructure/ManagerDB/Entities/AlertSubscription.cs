using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class AlertSubscription(Guid accountId, string principalType, Guid principalId, string? eventTypeFilter, string channel, string? contact, bool enabled) : BaseAuditableEntity
{
    public Guid AlertSubscriptionId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public string PrincipalType { get; set; } = principalType;
    public Guid PrincipalId { get; set; } = principalId;
    /// <summary>Null = all subscribed event types.</summary>
    public string? EventTypeFilter { get; set; } = eventTypeFilter;
    public string Channel { get; set; } = channel;
    /// <summary>Email address or E.164 phone; null for InApp/Push where the principal id is the address.</summary>
    public string? Contact { get; set; } = contact;
    public bool Enabled { get; set; } = enabled;
}
