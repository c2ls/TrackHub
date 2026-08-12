using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class AlertSubscriptionWriterTests
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

    [Test]
    public async Task CreateAlertSubscriptionAsync_AdminForAccountUser_Creates()
    {
        var accountId = Guid.NewGuid();
        var subscriberId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateAlertSubscriptionAsync_AdminForAccountUser_Creates));
        await context.Users.AddAsync(new User(subscriberId, "bob", true, accountId));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new AlertSubscriptionWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));
        var result = await writer.CreateAlertSubscriptionAsync(
            new AlertSubscriptionDto(accountId, RecipientPrincipalTypes.User, subscriberId, "CommunicationLoss", NotificationChannels.InApp, null, true), CancellationToken.None);

        Assert.That(result.PrincipalId, Is.EqualTo(subscriberId));
        Assert.That(context.AlertSubscriptions.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task CreateAlertSubscriptionAsync_Duplicate_ThrowsConflict()
    {
        var accountId = Guid.NewGuid();
        var subscriberId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateAlertSubscriptionAsync_Duplicate_ThrowsConflict));
        await context.Users.AddAsync(new User(subscriberId, "bob", true, accountId));
        await context.AlertSubscriptions.AddAsync(new AlertSubscription(accountId, RecipientPrincipalTypes.User, subscriberId, "CommunicationLoss", NotificationChannels.InApp, null, true));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new AlertSubscriptionWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));

        Assert.ThrowsAsync<ConflictException>(async () => await writer.CreateAlertSubscriptionAsync(
            new AlertSubscriptionDto(accountId, RecipientPrincipalTypes.User, subscriberId, "CommunicationLoss", NotificationChannels.InApp, null, true), CancellationToken.None));
    }

    [Test]
    public async Task CreateAlertSubscriptionAsync_NonAdminForAnotherPrincipal_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        var selfId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateAlertSubscriptionAsync_NonAdminForAnotherPrincipal_ThrowsForbidden));
        await context.Users.AddAsync(new User(selfId, "alice", true, accountId));
        await context.Users.AddAsync(new User(otherId, "bob", true, accountId));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new AlertSubscriptionWriter(context as IApplicationDbContext, Principal(accountId, userId: selfId, role: "Operator"));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await writer.CreateAlertSubscriptionAsync(
            new AlertSubscriptionDto(accountId, RecipientPrincipalTypes.User, otherId, null, NotificationChannels.InApp, null, true), CancellationToken.None));
    }

    [Test]
    public async Task CreateAlertSubscriptionAsync_NonAdminForSelf_Creates()
    {
        var accountId = Guid.NewGuid();
        var selfId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateAlertSubscriptionAsync_NonAdminForSelf_Creates));
        await context.Users.AddAsync(new User(selfId, "alice", true, accountId));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new AlertSubscriptionWriter(context as IApplicationDbContext, Principal(accountId, userId: selfId, role: "Operator"));
        var result = await writer.CreateAlertSubscriptionAsync(
            new AlertSubscriptionDto(accountId, RecipientPrincipalTypes.User, selfId, null, NotificationChannels.InApp, null, true), CancellationToken.None);

        Assert.That(result.PrincipalId, Is.EqualTo(selfId));
    }

    [Test]
    public async Task CreateAlertSubscriptionAsync_PrincipalOutsideAccount_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        var foreignUserId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateAlertSubscriptionAsync_PrincipalOutsideAccount_ThrowsForbidden));
        await context.Users.AddAsync(new User(foreignUserId, "mallory", true, Guid.NewGuid()));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new AlertSubscriptionWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await writer.CreateAlertSubscriptionAsync(
            new AlertSubscriptionDto(accountId, RecipientPrincipalTypes.User, foreignUserId, null, NotificationChannels.InApp, null, true), CancellationToken.None));
    }

    [Test]
    public async Task CreateAlertSubscriptionAsync_DriverWhatsAppWithoutContact_DefaultsFromDriverPhone()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateAlertSubscriptionAsync_DriverWhatsAppWithoutContact_DefaultsFromDriverPhone));
        var driver = new Driver(accountId, "Dave", "+573001234567", null, null, true, null, null, null, null);
        await context.Drivers.AddAsync(driver);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new AlertSubscriptionWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));
        var result = await writer.CreateAlertSubscriptionAsync(
            new AlertSubscriptionDto(accountId, RecipientPrincipalTypes.Driver, driver.DriverId, null, NotificationChannels.WhatsApp, null, true), CancellationToken.None);

        Assert.That(result.Contact, Is.EqualTo("+573001234567"));
    }

    [Test]
    public async Task DeleteAlertSubscriptionAsync_Admin_HardDeletes()
    {
        var accountId = Guid.NewGuid();
        var subscriberId = Guid.NewGuid();
        await using var context = NewContext(nameof(DeleteAlertSubscriptionAsync_Admin_HardDeletes));
        await context.Users.AddAsync(new User(subscriberId, "bob", true, accountId));
        var subscription = new AlertSubscription(accountId, RecipientPrincipalTypes.User, subscriberId, null, NotificationChannels.InApp, null, true);
        await context.AlertSubscriptions.AddAsync(subscription);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new AlertSubscriptionWriter(context as IApplicationDbContext, Principal(accountId, userId: Guid.NewGuid(), role: "Administrator"));
        await writer.DeleteAlertSubscriptionAsync(subscription.AlertSubscriptionId, CancellationToken.None);

        Assert.That(context.AlertSubscriptions.Any(), Is.False);
    }
}
