namespace TrackHub.Manager.Domain.Interfaces;

public interface IPlatformAnnouncementWriter
{
    Task<PlatformAnnouncementVm> CreatePlatformAnnouncementAsync(PlatformAnnouncementDto announcement, CancellationToken cancellationToken);
    Task UpdatePlatformAnnouncementAsync(Guid platformAnnouncementId, PlatformAnnouncementDto announcement, CancellationToken cancellationToken);
    Task DeletePlatformAnnouncementAsync(Guid platformAnnouncementId, CancellationToken cancellationToken);
}
