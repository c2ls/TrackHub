namespace TrackHub.Manager.Domain.Records;

public readonly record struct NotificationRuleDto(Guid AccountId, string RuleKey, string RuleType, bool Enabled, string TriggerEvent, string RecipientSelector, string ChannelsJson, string? ThrottlingJson, string? ConfigurationJson);

public readonly record struct NotificationDeliveryDto(Guid AccountId, Guid? NotificationRuleId, Guid? AlertEventId, string Channel, string RecipientPrincipalType, string Recipient, string Status);

public readonly record struct NotificationTemplateDto(Guid? AccountId, string TemplateKey, string Channel, string Locale, string? Subject, string Body, bool Active);

public readonly record struct AlertSubscriptionDto(Guid AccountId, string PrincipalType, Guid PrincipalId, string? EventTypeFilter, string Channel, string? Contact, bool Enabled);

/// <summary>Rendered, channel-agnostic message handed to an INotificationChannelProvider.</summary>
public readonly record struct NotificationMessage(Guid NotificationDeliveryId, Guid AccountId, string Recipient, string? Subject, string Body, string Locale, string? WebhookSecret, string? EventPayloadJson);

public readonly record struct NotificationSendResult(bool Success, string? ProviderMessageId, string? Error);
