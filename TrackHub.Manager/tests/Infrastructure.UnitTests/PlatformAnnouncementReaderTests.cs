using Common.Domain.Enums;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

// The visibility window is the security boundary for the anonymous endpoint: anything these tests
// let through is world-readable.
[TestFixture]
public class PlatformAnnouncementReaderTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static PlatformAnnouncement Announcement(
        string messageEn, bool active = true,
        DateTimeOffset? startsAt = null, DateTimeOffset? endsAt = null,
        AnnouncementSeverity severity = AnnouncementSeverity.Info)
        => new(messageEn, null, (int)severity, startsAt, endsAt, active);

    [Test]
    public async Task GetVisibleAsync_ExcludesInactiveAndOutOfWindowRows()
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = NewContext(nameof(GetVisibleAsync_ExcludesInactiveAndOutOfWindowRows));
        await context.PlatformAnnouncements.AddRangeAsync(
            Announcement("open-ended active"),
            Announcement("started, not ended", startsAt: now.AddHours(-1), endsAt: now.AddHours(1)),
            Announcement("draft", active: false),
            Announcement("future", startsAt: now.AddHours(1)),
            Announcement("expired", startsAt: now.AddHours(-2), endsAt: now.AddHours(-1)));
        await context.SaveChangesAsync(CancellationToken.None);

        var visible = await new PlatformAnnouncementReader(context).GetVisiblePlatformAnnouncementsAsync(now, CancellationToken.None);

        Assert.That(visible.Select(x => x.MessageEn), Is.EquivalentTo(new[] { "open-ended active", "started, not ended" }));
    }

    [Test]
    public async Task GetVisibleAsync_TreatsStartsAtAsInclusiveAndEndsAtAsExclusive()
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = NewContext(nameof(GetVisibleAsync_TreatsStartsAtAsInclusiveAndEndsAtAsExclusive));
        await context.PlatformAnnouncements.AddRangeAsync(
            Announcement("starting exactly now", startsAt: now),
            Announcement("ending exactly now", startsAt: now.AddHours(-1), endsAt: now));
        await context.SaveChangesAsync(CancellationToken.None);

        var visible = await new PlatformAnnouncementReader(context).GetVisiblePlatformAnnouncementsAsync(now, CancellationToken.None);

        Assert.That(visible.Select(x => x.MessageEn), Is.EquivalentTo(new[] { "starting exactly now" }));
    }

    [Test]
    public async Task GetVisibleAsync_OrdersMostSevereFirst()
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = NewContext(nameof(GetVisibleAsync_OrdersMostSevereFirst));
        await context.PlatformAnnouncements.AddRangeAsync(
            Announcement("info", severity: AnnouncementSeverity.Info),
            Announcement("critical", severity: AnnouncementSeverity.Critical),
            Announcement("warning", severity: AnnouncementSeverity.Warning));
        await context.SaveChangesAsync(CancellationToken.None);

        var visible = await new PlatformAnnouncementReader(context).GetVisiblePlatformAnnouncementsAsync(now, CancellationToken.None);

        Assert.That(visible.First().MessageEn, Is.EqualTo("critical"));
    }

    [Test]
    public async Task GetAllAsync_ReturnsDraftsAndExpiredRows()
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = NewContext(nameof(GetAllAsync_ReturnsDraftsAndExpiredRows));
        await context.PlatformAnnouncements.AddRangeAsync(
            Announcement("draft", active: false),
            Announcement("expired", startsAt: now.AddHours(-2), endsAt: now.AddHours(-1)));
        await context.SaveChangesAsync(CancellationToken.None);

        var all = await new PlatformAnnouncementReader(context).GetPlatformAnnouncementsAsync(0, 50, CancellationToken.None);

        Assert.That(all, Has.Count.EqualTo(2), "the administrator surface sees every row, unlike the anonymous one");
    }

    [Test]
    public async Task Writer_RoundTripsSeverityScheduleAndDelete()
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = NewContext(nameof(Writer_RoundTripsSeverityScheduleAndDelete));
        var writer = new PlatformAnnouncementWriter(context);

        var created = await writer.CreatePlatformAnnouncementAsync(
            new PlatformAnnouncementDto("Maintenance window", "Ventana de mantenimiento", AnnouncementSeverity.Critical, now, now.AddHours(2), true),
            CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(created.Severity, Is.EqualTo(AnnouncementSeverity.Critical));
            Assert.That(created.MessageEs, Is.EqualTo("Ventana de mantenimiento"));
            Assert.That(created.EndsAt, Is.EqualTo(now.AddHours(2)));
        }

        await writer.UpdatePlatformAnnouncementAsync(created.PlatformAnnouncementId,
            new PlatformAnnouncementDto("Resolved", null, AnnouncementSeverity.Info, null, null, false), CancellationToken.None);

        var updated = await context.PlatformAnnouncements.SingleAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated.MessageEn, Is.EqualTo("Resolved"));
            Assert.That(updated.MessageEs, Is.Null);
            Assert.That(updated.Severity, Is.EqualTo((int)AnnouncementSeverity.Info));
            Assert.That(updated.Active, Is.False);
        }

        await writer.DeletePlatformAnnouncementAsync(created.PlatformAnnouncementId, CancellationToken.None);
        Assert.That(await context.PlatformAnnouncements.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task DeactivatedAnnouncement_DisappearsFromTheAnonymousSurface()
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = NewContext(nameof(DeactivatedAnnouncement_DisappearsFromTheAnonymousSurface));
        var writer = new PlatformAnnouncementWriter(context);
        var reader = new PlatformAnnouncementReader(context);

        var created = await writer.CreatePlatformAnnouncementAsync(
            new PlatformAnnouncementDto("Investigating", null, AnnouncementSeverity.Warning, null, null, true), CancellationToken.None);
        Assert.That(await reader.GetVisiblePlatformAnnouncementsAsync(now, CancellationToken.None), Has.Count.EqualTo(1));

        await writer.UpdatePlatformAnnouncementAsync(created.PlatformAnnouncementId,
            new PlatformAnnouncementDto("Investigating", null, AnnouncementSeverity.Warning, null, null, false), CancellationToken.None);

        Assert.That(await reader.GetVisiblePlatformAnnouncementsAsync(now, CancellationToken.None), Is.Empty);
    }
}
