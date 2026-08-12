using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

// Platform-wide, not account-scoped: see PlatformAnnouncementReader. Write access is restricted to
// the Administrator role by [Authorize(Administrative, Write)] on the commands.
public sealed class PlatformAnnouncementWriter(IApplicationDbContext context) : IPlatformAnnouncementWriter
{
    public async Task<PlatformAnnouncementVm> CreatePlatformAnnouncementAsync(PlatformAnnouncementDto announcement, CancellationToken cancellationToken)
    {
        var entity = new PlatformAnnouncement(
            announcement.MessageEn, announcement.MessageEs, (int)announcement.Severity,
            announcement.StartsAt, announcement.EndsAt, announcement.Active);

        await context.PlatformAnnouncements.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task UpdatePlatformAnnouncementAsync(Guid platformAnnouncementId, PlatformAnnouncementDto announcement, CancellationToken cancellationToken)
    {
        var entity = await context.PlatformAnnouncements.FirstAsync(x => x.PlatformAnnouncementId == platformAnnouncementId, cancellationToken);

        context.PlatformAnnouncements.Attach(entity);
        entity.MessageEn = announcement.MessageEn;
        entity.MessageEs = announcement.MessageEs;
        entity.Severity = (int)announcement.Severity;
        entity.StartsAt = announcement.StartsAt;
        entity.EndsAt = announcement.EndsAt;
        entity.Active = announcement.Active;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePlatformAnnouncementAsync(Guid platformAnnouncementId, CancellationToken cancellationToken)
    {
        var entity = await context.PlatformAnnouncements.FirstAsync(x => x.PlatformAnnouncementId == platformAnnouncementId, cancellationToken);
        context.PlatformAnnouncements.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static PlatformAnnouncementVm ToVm(PlatformAnnouncement x)
        => new(x.PlatformAnnouncementId, x.MessageEn, x.MessageEs, (AnnouncementSeverity)x.Severity, x.StartsAt, x.EndsAt, x.Active, x.LastModified);
}
