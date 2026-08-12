namespace TrackHub.Manager.Domain.Interfaces;

public interface IBackgroundJobRunWriter
{
    Task<BackgroundJobRunVm> CreateBackgroundJobRunAsync(BackgroundJobRunDto backgroundJobRun, CancellationToken cancellationToken);
}
