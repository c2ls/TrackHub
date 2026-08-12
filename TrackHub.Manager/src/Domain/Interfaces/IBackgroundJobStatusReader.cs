namespace TrackHub.Manager.Domain.Interfaces;

/// <summary>
/// Deliberately platform-scoped (unscoped) reads for the administrator tier of the status page —
/// this does NOT go through <c>AccountScopedDataAccess</c>. Access is gated upstream by
/// <c>[Authorize(Administrative, Read)]</c>, which only the Administrator role holds.
/// </summary>
public interface IBackgroundJobStatusReader
{
    Task<IReadOnlyCollection<BackgroundJobStatusVm>> GetBackgroundJobStatusAsync(CancellationToken cancellationToken);
}
