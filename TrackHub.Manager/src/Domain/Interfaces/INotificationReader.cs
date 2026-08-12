namespace TrackHub.Manager.Domain.Interfaces;

public interface INotificationReader
{
    Task<IReadOnlyCollection<NotificationRuleVm>> GetNotificationRulesAsync(Guid accountId, int skip, int take, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NotificationDeliveryVm>> GetNotificationDeliveriesAsync(Guid accountId, string? status, string? channel, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<MyNotificationVm>> GetMyNotificationsAsync(bool unreadOnly, int skip, int take, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<DeliveryHealthVm>> GetDeliveryHealthAsync(Guid accountId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
}
