namespace TrackHub.Manager.Domain.Interfaces;

public interface IAlertSubscriptionReader
{
    Task<IReadOnlyCollection<AlertSubscriptionVm>> GetAlertSubscriptionsAsync(Guid accountId, Guid? principalId, int skip, int take, CancellationToken cancellationToken);
}
