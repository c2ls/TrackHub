namespace TrackHub.Manager.Domain.Interfaces;

public interface IAlertEventReader
{
    Task<IReadOnlyCollection<AlertEventVm>> GetAlertEventsAsync(Guid accountId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken);
}
