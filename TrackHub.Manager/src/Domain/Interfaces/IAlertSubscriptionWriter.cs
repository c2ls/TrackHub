namespace TrackHub.Manager.Domain.Interfaces;

public interface IAlertSubscriptionWriter
{
    Task<AlertSubscriptionVm> CreateAlertSubscriptionAsync(AlertSubscriptionDto subscription, CancellationToken cancellationToken);
    Task UpdateAlertSubscriptionAsync(Guid alertSubscriptionId, AlertSubscriptionDto subscription, CancellationToken cancellationToken);
    Task DeleteAlertSubscriptionAsync(Guid alertSubscriptionId, CancellationToken cancellationToken);
}
