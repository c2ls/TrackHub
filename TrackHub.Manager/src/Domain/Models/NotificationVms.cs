namespace TrackHub.Manager.Domain.Models;

public readonly record struct NotificationRuleVm(Guid NotificationRuleId, Guid AccountId, string RuleKey, string RuleType, bool Enabled, string TriggerEvent, string RecipientSelector, string ChannelsJson, string? ThrottlingJson, string? ConfigurationJson, DateTimeOffset LastModified);

public readonly record struct NotificationDeliveryVm(Guid NotificationDeliveryId, Guid AccountId, Guid? NotificationRuleId, Guid? AlertEventId, string Channel, string RecipientPrincipalType, string Recipient, string Status, int Attempts, string? ProviderMessageId, string? Error, DateTimeOffset? SentAt, DateTimeOffset? ReadAt, DateTimeOffset LastModified);

public readonly record struct NotificationTemplateVm(Guid NotificationTemplateId, Guid? AccountId, string TemplateKey, string Channel, string Locale, string? Subject, string Body, bool Active, DateTimeOffset LastModified);

public readonly record struct AlertSubscriptionVm(Guid AlertSubscriptionId, Guid AccountId, string PrincipalType, Guid PrincipalId, string? EventTypeFilter, string Channel, string? Contact, bool Enabled, DateTimeOffset LastModified);

/// <summary>In-app feed item for the current principal.</summary>
public readonly record struct MyNotificationVm(Guid NotificationDeliveryId, Guid? AlertEventId, string? EventType, string? Severity, string? SourceModule, string? ResourceType, string? ResourceId, string? PayloadJson, DateTimeOffset CreatedAt, DateTimeOffset? ReadAt);

/// <summary>Channel/status aggregate for the delivery-health tiles.</summary>
public readonly record struct DeliveryHealthVm(string Channel, string Status, int Count, double AverageAttempts);
