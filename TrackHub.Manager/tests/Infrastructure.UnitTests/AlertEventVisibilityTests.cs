using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class AlertEventVisibilityTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid? accountId, PrincipalType principalType = PrincipalType.User, Guid? userId = null, string? role = null)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(principalType);
        principal.SetupGet(p => p.UserId).Returns(userId);
        principal.SetupGet(p => p.Role).Returns(role);
        return principal.Object;
    }

    private static AlertEvent Event(Guid accountId, string resourceType, string resourceId)
        => new(accountId, "CommunicationLoss", "Warning", "Notifications", resourceType, resourceId, "Open", null, $"k:{Guid.NewGuid():N}");

    [Test]
    public async Task GetAlertEventsAsync_NonPrivilegedUser_SeesOnlyGroupVisibleTransporterEvents()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var visibleTransporterId = Guid.NewGuid();
        var hiddenTransporterId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetAlertEventsAsync_NonPrivilegedUser_SeesOnlyGroupVisibleTransporterEvents));
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        await context.AlertEvents.AddAsync(Event(accountId, "Transporter", visibleTransporterId.ToString()));
        await context.AlertEvents.AddAsync(Event(accountId, "Transporter", hiddenTransporterId.ToString()));
        // Account-level event (no group-mappable resource): administrators/managers only.
        await context.AlertEvents.AddAsync(Event(accountId, "Operator", Guid.NewGuid().ToString()));
        await context.SaveChangesAsync(CancellationToken.None);

        var visibleTransporters = new Mock<IVisibleTransporterReader>();
        visibleTransporters
            .Setup(v => v.GetVisibleTransporterIdsAsync(userId, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<Guid> { visibleTransporterId });

        var reader = new AlertEventReader(context as IApplicationDbContext, Principal(accountId, userId: userId, role: "Operator"), visibleTransporters.Object);
        var result = await reader.GetAlertEventsAsync(accountId, null, null, 0, 50, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Single().ResourceId, Is.EqualTo(visibleTransporterId.ToString()));
    }

    [Test]
    public async Task GetAlertEventsAsync_Administrator_SeesAllAccountEvents()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetAlertEventsAsync_Administrator_SeesAllAccountEvents));
        await context.AlertEvents.AddAsync(Event(accountId, "Transporter", Guid.NewGuid().ToString()));
        await context.AlertEvents.AddAsync(Event(accountId, "Operator", Guid.NewGuid().ToString()));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new AlertEventReader(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"), Mock.Of<IVisibleTransporterReader>());
        var result = await reader.GetAlertEventsAsync(accountId, null, null, 0, 50, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task RecordAlertEventAsync_TransporterOutsideAccount_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(RecordAlertEventAsync_TransporterOutsideAccount_ThrowsForbidden));
        var foreignTransporter = new Transporter("other", 1, Guid.NewGuid());
        await context.Transporters.AddAsync(foreignTransporter);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new AlertEventWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await writer.RecordAlertEventAsync(
            new AlertEventDto(accountId, "CommunicationLoss", "Warning", "Notifications", "Transporter", foreignTransporter.TransporterId.ToString(), "Open", null, "dedup-1"), CancellationToken.None));
    }

    [Test]
    public async Task RecordAlertEventAsync_TransporterInAccount_Records()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(RecordAlertEventAsync_TransporterInAccount_Records));
        var transporter = new Transporter("mine", 1, accountId);
        await context.Transporters.AddAsync(transporter);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new AlertEventWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));
        var result = await writer.RecordAlertEventAsync(
            new AlertEventDto(accountId, "CommunicationLoss", "Warning", "Notifications", "Transporter", transporter.TransporterId.ToString(), "Open", null, "dedup-2"), CancellationToken.None);

        Assert.That(result.ResourceId, Is.EqualTo(transporter.TransporterId.ToString()));
    }

    [Test]
    public async Task RecordAlertEventAsync_UnmappedResourceType_PassesThrough()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(RecordAlertEventAsync_UnmappedResourceType_PassesThrough));

        var writer = new AlertEventWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));
        var result = await writer.RecordAlertEventAsync(
            new AlertEventDto(accountId, "GeofenceEntered", "Info", "Geofencing", "Geofence", Guid.NewGuid().ToString(), "Open", null, "dedup-3"), CancellationToken.None);

        Assert.That(result.AlertEventId, Is.Not.EqualTo(Guid.Empty));
    }
}
