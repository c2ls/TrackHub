namespace TrackHub.Manager.Domain.Interfaces;

public interface INotificationTemplateReader
{
    /// <summary>Merged view: account overrides plus platform defaults (AccountId = null) not overridden.</summary>
    Task<IReadOnlyCollection<NotificationTemplateVm>> GetNotificationTemplatesAsync(Guid accountId, CancellationToken cancellationToken);
}
