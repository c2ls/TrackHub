namespace TrackHub.Manager.Domain.Interfaces;

public interface INotificationTemplateWriter
{
    Task<NotificationTemplateVm> CreateNotificationTemplateAsync(NotificationTemplateDto template, CancellationToken cancellationToken);
    Task UpdateNotificationTemplateAsync(Guid notificationTemplateId, NotificationTemplateDto template, CancellationToken cancellationToken);
    Task DeleteNotificationTemplateAsync(Guid notificationTemplateId, CancellationToken cancellationToken);
}
