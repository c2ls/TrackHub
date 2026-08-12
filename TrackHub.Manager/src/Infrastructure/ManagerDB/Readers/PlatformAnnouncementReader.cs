using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

// Platform-wide by construction: announcements are not account-scoped data, so this reader
// deliberately does NOT derive from AccountScopedDataAccess and takes no principal. The management
// read is gated by [Authorize(Administrative, Read)]; the visibility read is intentionally anonymous
// and therefore filters to Active + in-window rows in the query itself.
public sealed class PlatformAnnouncementReader(IApplicationDbContext context) : IPlatformAnnouncementReader
{
    // The projections below are written inline rather than through a shared ToVm(entity) helper:
    // a method call inside an EF projection is not translatable, and relying on top-level client
    // evaluation would silently materialize whole entities. Inline projections keep the SELECT
    // column-scoped, matching every other reader in this service.

    public async Task<IReadOnlyCollection<PlatformAnnouncementVm>> GetPlatformAnnouncementsAsync(int skip, int take, CancellationToken cancellationToken)
        => await context.PlatformAnnouncements
            .OrderByDescending(x => x.StartsAt ?? x.Created).ThenByDescending(x => x.Created)
            .Skip(Math.Max(0, skip)).Take(Math.Clamp(take <= 0 ? 50 : take, 1, 500))
            .Select(x => new PlatformAnnouncementVm(
                x.PlatformAnnouncementId, x.MessageEn, x.MessageEs,
                (AnnouncementSeverity)x.Severity, x.StartsAt, x.EndsAt, x.Active, x.LastModified))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<PlatformAnnouncementVm>> GetVisiblePlatformAnnouncementsAsync(DateTimeOffset asOf, CancellationToken cancellationToken)
        => await context.PlatformAnnouncements
            .Where(x => x.Active
                && (x.StartsAt == null || x.StartsAt <= asOf)
                && (x.EndsAt == null || asOf < x.EndsAt))
            .OrderByDescending(x => x.Severity).ThenByDescending(x => x.StartsAt ?? x.Created)
            .Select(x => new PlatformAnnouncementVm(
                x.PlatformAnnouncementId, x.MessageEn, x.MessageEs,
                (AnnouncementSeverity)x.Severity, x.StartsAt, x.EndsAt, x.Active, x.LastModified))
            .ToListAsync(cancellationToken);
}
