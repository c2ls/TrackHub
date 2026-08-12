using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class NotificationDelivery(Guid accountId, Guid? notificationRuleId, Guid? alertEventId, string channel, string recipientPrincipalType, string recipient, string status) : BaseAuditableEntity
{
    public Guid NotificationDeliveryId { get; private set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public Guid? NotificationRuleId { get; set; } = notificationRuleId;
    public Guid? AlertEventId { get; set; } = alertEventId;
    public string Channel { get; set; } = channel;
    public string RecipientPrincipalType { get; set; } = recipientPrincipalType;
    public string Recipient { get; set; } = recipient;
    public string Status { get; set; } = status;
    public int Attempts { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    /// <summary>
    /// Optional pre-rendered content ({"subject","body"}) that overrides template rendering at
    /// dispatch time; used by digest summary deliveries.
    /// </summary>
    public string? PayloadJson { get; set; }
}
