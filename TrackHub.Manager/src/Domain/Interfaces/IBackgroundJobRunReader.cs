namespace TrackHub.Manager.Domain.Interfaces;

public interface IBackgroundJobRunReader
{
    Task<IReadOnlyCollection<BackgroundJobRunVm>> GetBackgroundJobRunsAsync(Guid? accountId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken cancellationToken);
}
