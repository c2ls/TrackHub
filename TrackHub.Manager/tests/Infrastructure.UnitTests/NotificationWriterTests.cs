using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class NotificationWriterTests
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

    [Test]
    public async Task CreateNotificationRuleAsync_SelectorUserOutsideAccount_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        var foreignUserId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateNotificationRuleAsync_SelectorUserOutsideAccount_ThrowsForbidden));
        await context.Users.AddAsync(new User(foreignUserId, "mallory", true, Guid.NewGuid()));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await writer.CreateNotificationRuleAsync(
            new TrackHub.Manager.Domain.Records.NotificationRuleDto(accountId, "rule", "Notifications", true, "CommunicationLoss",
                $$"""{"userIds":["{{foreignUserId}}"]}""", """["InApp"]""", null, null), CancellationToken.None));
    }

    [Test]
    public async Task CreateNotificationRuleAsync_SelectorUserInAccount_Creates()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateNotificationRuleAsync_SelectorUserInAccount_Creates));
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));
        var result = await writer.CreateNotificationRuleAsync(
            new TrackHub.Manager.Domain.Records.NotificationRuleDto(accountId, "rule", "Notifications", true, "CommunicationLoss",
                $$"""{"userIds":["{{userId}}"]}""", """["InApp"]""", null, null), CancellationToken.None);

        Assert.That(result.NotificationRuleId, Is.Not.EqualTo(Guid.Empty));
    }

    // Rules/deliveries are admin surfaces: the Notifications action grants are held by every
    // portal role for self-service (feed/mark-read/subscriptions), so the privileged check
    // lives in the writer.
    [Test]
    public async Task CreateNotificationRuleAsync_NonPrivilegedUser_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateNotificationRuleAsync_NonPrivilegedUser_ThrowsForbidden));
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationWriter(context as IApplicationDbContext, Principal(accountId, userId: userId, role: "User"));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await writer.CreateNotificationRuleAsync(
            new TrackHub.Manager.Domain.Records.NotificationRuleDto(accountId, "rule", "Notifications", true, "CommunicationLoss",
                "", """["InApp"]""", null, null), CancellationToken.None));
    }

    [Test]
    public async Task RetryNotificationDeliveryAsync_NonPrivilegedUser_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(RetryNotificationDeliveryAsync_NonPrivilegedUser_ThrowsForbidden));
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        var delivery = new NotificationDelivery(accountId, null, null, NotificationChannels.Email, RecipientPrincipalTypes.Contact, "a@b.com", DeliveryStatuses.Failed);
        await context.NotificationDeliveries.AddAsync(delivery);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationWriter(context as IApplicationDbContext, Principal(accountId, userId: userId, role: "User"));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await writer.RetryNotificationDeliveryAsync(delivery.NotificationDeliveryId, CancellationToken.None));
    }

    [Test]
    public async Task RetryNotificationDeliveryAsync_FailedDelivery_ResetsToPendingPreservingAttempts()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(RetryNotificationDeliveryAsync_FailedDelivery_ResetsToPendingPreservingAttempts));
        var delivery = new NotificationDelivery(accountId, null, null, NotificationChannels.Email, RecipientPrincipalTypes.Contact, "a@b.com", DeliveryStatuses.Failed) { Attempts = 5, Error = "boom" };
        await context.NotificationDeliveries.AddAsync(delivery);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));
        await writer.RetryNotificationDeliveryAsync(delivery.NotificationDeliveryId, CancellationToken.None);

        var updated = context.NotificationDeliveries.Single();
        Assert.That(updated.Status, Is.EqualTo(DeliveryStatuses.Pending));
        Assert.That(updated.Attempts, Is.EqualTo(5));
        Assert.That(updated.Error, Is.Null);
    }

    [Test]
    public async Task RetryNotificationDeliveryAsync_NonFailedDelivery_ThrowsConflict()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(RetryNotificationDeliveryAsync_NonFailedDelivery_ThrowsConflict));
        var delivery = new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, Guid.NewGuid().ToString(), DeliveryStatuses.Sent);
        await context.NotificationDeliveries.AddAsync(delivery);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));

        Assert.ThrowsAsync<ConflictException>(async () => await writer.RetryNotificationDeliveryAsync(delivery.NotificationDeliveryId, CancellationToken.None));
    }

    [Test]
    public async Task MarkNotificationReadAsync_Recipient_SetsReadAt()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(MarkNotificationReadAsync_Recipient_SetsReadAt));
        var delivery = new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, userId.ToString(), DeliveryStatuses.Sent);
        await context.NotificationDeliveries.AddAsync(delivery);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationWriter(context as IApplicationDbContext, Principal(accountId, userId: userId));
        await writer.MarkNotificationReadAsync(delivery.NotificationDeliveryId, CancellationToken.None);

        Assert.That(context.NotificationDeliveries.Single().ReadAt, Is.Not.Null);
    }

    [Test]
    public async Task MarkNotificationReadAsync_NotTheRecipient_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(MarkNotificationReadAsync_NotTheRecipient_ThrowsForbidden));
        var delivery = new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.User, Guid.NewGuid().ToString(), DeliveryStatuses.Sent);
        await context.NotificationDeliveries.AddAsync(delivery);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid()));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await writer.MarkNotificationReadAsync(delivery.NotificationDeliveryId, CancellationToken.None));
    }

    [Test]
    public async Task MarkNotificationReadAsync_RoleAddressedRow_AccountUserWithRoleMayMarkRead()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(MarkNotificationReadAsync_RoleAddressedRow_AccountUserWithRoleMayMarkRead));
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        var delivery = new NotificationDelivery(accountId, null, null, NotificationChannels.InApp, RecipientPrincipalTypes.Role, "Administrator", DeliveryStatuses.Sent);
        await context.NotificationDeliveries.AddAsync(delivery);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationWriter(context as IApplicationDbContext, Principal(accountId, userId: userId, role: "Administrator"));
        await writer.MarkNotificationReadAsync(delivery.NotificationDeliveryId, CancellationToken.None);

        Assert.That(context.NotificationDeliveries.Single().ReadAt, Is.Not.Null);
    }
}
