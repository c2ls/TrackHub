namespace TrackHub.Manager.Domain.Interfaces;

public interface IPlatformAnnouncementReader
{
    /// <summary>All rows, newest first — the administrator management surface.</summary>
    Task<IReadOnlyCollection<PlatformAnnouncementVm>> GetPlatformAnnouncementsAsync(int skip, int take, CancellationToken cancellationToken);

    /// <summary>
    /// Only the currently-visible rows: Active AND inside the schedule window at <paramref name="asOf"/>.
    /// Backs the anonymous REST endpoint, so it must never surface drafts or out-of-window rows.
    /// </summary>
    Task<IReadOnlyCollection<PlatformAnnouncementVm>> GetVisiblePlatformAnnouncementsAsync(DateTimeOffset asOf, CancellationToken cancellationToken);
}
