using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class NotificationReaderTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid? accountId, PrincipalType principalType = PrincipalType.User, Guid? userId = null, Guid? driverId = null, string? role = null)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(principalType);
        principal.SetupGet(p => p.UserId).Returns(userId);
        principal.SetupGet(p => p.DriverId).Returns(driverId);
        principal.SetupGet(p => p.Role).Returns(role);
        return principal.Object;
    }

    private static NotificationReader NewReader(ApplicationDbContext context, ICurrentPrincipal principal, IReadOnlySet<Guid>? visibleTransporterIds = null)
    {
        var visibleTransporters = new Mock<IVisibleTransporterReader>();
        visibleTransporters
            .Setup(v => v.GetVisibleTransporterIdsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(visibleTransporterIds ?? new HashSet<Guid>());
        return new NotificationReader(context as IApplicationDbContext, principal, visibleTransporters.Object);
    }

    [Test]
    public async Task GetNotificationDeliveriesAsync_EmailDelivery_MasksContactEndpoint()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetNotificationDeliveriesAsync_EmailDelivery_MasksContactEndpoint));
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.Email, RecipientPrincipalTypes.Contact, "operations@example.com", DeliveryStatuses.Sent));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = NewReader(context, Principal(accountId));
        var result = await reader.GetNotificationDeliveriesAsync(accountId, null, null, null, null, 0, 50, CancellationToken.None);

        Assert.That(result.Single().Recipient, Is.EqualTo("***.com"));
    }

    [Test]
    public async Task GetNotificationDeliveriesAsync_StatusFilter_ReturnsOnlyMatching()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetNotificationDeliveriesAsync_StatusFilter_ReturnsOnlyMatching));
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, Guid.NewGuid().ToString(), DeliveryStatuses.Sent));
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, Guid.NewGuid().ToString(), DeliveryStatuses.Failed));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = NewReader(context, Principal(accountId));
        var result = await reader.GetNotificationDeliveriesAsync(accountId, DeliveryStatuses.Failed, null, null, null, 0, 50, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Single().Status, Is.EqualTo(DeliveryStatuses.Failed));
    }

    [Test]
    public async Task GetMyNotificationsAsync_User_SeesOwnAndRoleAddressedRowsOnly()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetMyNotificationsAsync_User_SeesOwnAndRoleAddressedRowsOnly));
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, userId.ToString(), DeliveryStatuses.Sent));
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.Role, "Administrator", DeliveryStatuses.Sent));
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, otherUserId.ToString(), DeliveryStatuses.Sent));
        // Role row in ANOTHER account must not leak.
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(Guid.NewGuid(), null, null, NotificationChannels.InApp, RecipientPrincipalTypes.Role, "Administrator", DeliveryStatuses.Sent));
        // Email rows never appear in the in-app feed.
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.Email, RecipientPrincipalTypes.User, userId.ToString(), DeliveryStatuses.Sent));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = NewReader(context, Principal(accountId, userId: userId, role: "Administrator"));
        var result = await reader.GetMyNotificationsAsync(false, 0, 50, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetMyNotificationsAsync_Driver_SeesOnlyDeliveriesAddressedToDriverId()
    {
        var accountId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetMyNotificationsAsync_Driver_SeesOnlyDeliveriesAddressedToDriverId));
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.Driver, driverId.ToString(), DeliveryStatuses.Sent));
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.Driver, Guid.NewGuid().ToString(), DeliveryStatuses.Sent));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = NewReader(context, Principal(accountId, PrincipalType.Driver, driverId: driverId));
        var result = await reader.GetMyNotificationsAsync(false, 0, 50, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetMyNotificationsAsync_NonPrivilegedUser_HidesTransporterEventsOutsideGroupVisibility()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var visibleTransporterId = Guid.NewGuid();
        var hiddenTransporterId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetMyNotificationsAsync_NonPrivilegedUser_HidesTransporterEventsOutsideGroupVisibility));
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        var visibleEvent = new AlertEvent(accountId, "CommunicationLoss", "Warning", "Notifications", "Transporter", visibleTransporterId.ToString(), "Open", null, "k1");
        var hiddenEvent = new AlertEvent(accountId, "CommunicationLoss", "Warning", "Notifications", "Transporter", hiddenTransporterId.ToString(), "Open", null, "k2");
        await context.AlertEvents.AddAsync(visibleEvent);
        await context.AlertEvents.AddAsync(hiddenEvent);
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, visibleEvent.AlertEventId, NotificationChannels.InApp, RecipientPrincipalTypes.User, userId.ToString(), DeliveryStatuses.Sent));
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, hiddenEvent.AlertEventId, NotificationChannels.InApp, RecipientPrincipalTypes.User, userId.ToString(), DeliveryStatuses.Sent));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = NewReader(context, Principal(accountId, userId: userId, role: "Operator"), new HashSet<Guid> { visibleTransporterId });
        var result = await reader.GetMyNotificationsAsync(false, 0, 50, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Single().ResourceId, Is.EqualTo(visibleTransporterId.ToString()));
    }

    [Test]
    public async Task GetMyNotificationsAsync_UnreadOnly_ExcludesReadRows()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetMyNotificationsAsync_UnreadOnly_ExcludesReadRows));
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, userId.ToString(), DeliveryStatuses.Sent) { ReadAt = DateTimeOffset.UtcNow });
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, userId.ToString(), DeliveryStatuses.Sent));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = NewReader(context, Principal(accountId, userId: userId));
        var result = await reader.GetMyNotificationsAsync(true, 0, 50, CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.Single().ReadAt, Is.Null);
    }

    [Test]
    public async Task GetMyNotificationsAsync_ServiceClient_ThrowsForbidden()
    {
        await using var context = NewContext(nameof(GetMyNotificationsAsync_ServiceClient_ThrowsForbidden));
        var reader = NewReader(context, Principal(null, PrincipalType.ServiceClient));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await reader.GetMyNotificationsAsync(false, 0, 50, CancellationToken.None));
    }

    [Test]
    public async Task GetDeliveryHealthAsync_AggregatesByChannelAndStatus()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetDeliveryHealthAsync_AggregatesByChannelAndStatus));
        var now = DateTimeOffset.UtcNow;
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, Guid.NewGuid().ToString(), DeliveryStatuses.Sent) { Attempts = 1, Created = now });
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, Guid.NewGuid().ToString(), DeliveryStatuses.Sent) { Attempts = 3, Created = now });
        await context.NotificationDeliveries.AddAsync(new NotificationDelivery(accountId, null, null, NotificationChannels.Email, RecipientPrincipalTypes.Contact, "a@b.com", DeliveryStatuses.Failed) { Attempts = 5, Created = now });
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = NewReader(context, Principal(accountId));
        var result = await reader.GetDeliveryHealthAsync(accountId, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1), CancellationToken.None);

        Assert.That(result, Has.Count.EqualTo(2));
        var inApp = result.Single(r => r.Channel == NotificationChannels.InApp);
        Assert.That(inApp.Count, Is.EqualTo(2));
        Assert.That(inApp.AverageAttempts, Is.EqualTo(2d));
    }
}
