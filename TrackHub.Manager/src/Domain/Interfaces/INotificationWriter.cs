namespace TrackHub.Manager.Domain.Interfaces;

public interface INotificationWriter
{
    Task<NotificationRuleVm> CreateNotificationRuleAsync(NotificationRuleDto notificationRule, CancellationToken cancellationToken);
    Task UpdateNotificationRuleAsync(Guid notificationRuleId, NotificationRuleDto notificationRule, CancellationToken cancellationToken);
    Task DisableNotificationRuleAsync(Guid notificationRuleId, CancellationToken cancellationToken);
    Task<NotificationDeliveryVm> CreateNotificationDeliveryAsync(NotificationDeliveryDto notificationDelivery, CancellationToken cancellationToken);
    Task RetryNotificationDeliveryAsync(Guid notificationDeliveryId, CancellationToken cancellationToken);
    Task MarkNotificationReadAsync(Guid notificationDeliveryId, CancellationToken cancellationToken);
}
