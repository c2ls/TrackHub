using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class NotificationTemplateTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid? accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(p => p.UserId).Returns(Guid.NewGuid());
        principal.SetupGet(p => p.Role).Returns("Administrator");
        return principal.Object;
    }

    [Test]
    public async Task GetNotificationTemplatesAsync_MergedView_SynthesizesResourceDefaultsAndOverridesWin()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetNotificationTemplatesAsync_MergedView_SynthesizesResourceDefaultsAndOverridesWin));
        await context.NotificationTemplates.AddAsync(new NotificationTemplate(accountId, "TestNotification", "Email", "en", "override subject", "override body", true));
        await context.NotificationTemplates.AddAsync(new NotificationTemplate(accountId, "CommunicationLoss", "Email", "es", "asunto propio", "cuerpo propio", true));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new NotificationTemplateReader(context as IApplicationDbContext, Principal(accountId));
        var result = await reader.GetNotificationTemplatesAsync(accountId, CancellationToken.None);

        // Built-in defaults come from the resources, never from the database (nothing is seeded):
        // TestNotification + NotificationDigest × en/es, minus the shadowed (TestNotification, Email, en).
        var defaults = result.Where(t => t.AccountId == null).ToList();
        Assert.That(defaults, Has.Count.EqualTo(3));
        Assert.That(defaults, Has.All.Property("NotificationTemplateId").EqualTo(Guid.Empty));
        Assert.That(defaults.Select(t => t.Body), Has.All.Not.Empty);
        Assert.That(defaults.Any(t => t.TemplateKey == "TestNotification" && t.Locale == "en"), Is.False, "the account override must shadow the built-in default");

        Assert.That(result.Single(t => t.TemplateKey == "TestNotification" && t.Locale == "en").Subject, Is.EqualTo("override subject"));
        Assert.That(result.Single(t => t.TemplateKey == "CommunicationLoss").AccountId, Is.EqualTo(accountId));
    }

    [Test]
    public async Task CreateNotificationTemplateAsync_NonPrivilegedUser_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateNotificationTemplateAsync_NonPrivilegedUser_ThrowsForbidden));
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        await context.SaveChangesAsync(CancellationToken.None);

        var nonPrivileged = new Mock<ICurrentPrincipal>();
        nonPrivileged.SetupGet(p => p.AccountId).Returns(accountId);
        nonPrivileged.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        nonPrivileged.SetupGet(p => p.UserId).Returns(userId);
        nonPrivileged.SetupGet(p => p.Role).Returns("User");
        var writer = new NotificationTemplateWriter(context as IApplicationDbContext, nonPrivileged.Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await writer.CreateNotificationTemplateAsync(
            new NotificationTemplateDto(accountId, "CommunicationLoss", "Email", "en", null, "body", true), CancellationToken.None));
    }

    [Test]
    public async Task CreateNotificationTemplateAsync_DuplicateKeyChannelLocale_ThrowsConflict()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateNotificationTemplateAsync_DuplicateKeyChannelLocale_ThrowsConflict));
        await context.NotificationTemplates.AddAsync(new NotificationTemplate(accountId, "CommunicationLoss", "Email", "en", null, "body", true));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationTemplateWriter(context as IApplicationDbContext, Principal(accountId));

        Assert.ThrowsAsync<ConflictException>(async () => await writer.CreateNotificationTemplateAsync(
            new NotificationTemplateDto(accountId, "CommunicationLoss", "Email", "en", null, "other body", true), CancellationToken.None));
    }

    [Test]
    public async Task UpdateNotificationTemplateAsync_PlatformDefault_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(UpdateNotificationTemplateAsync_PlatformDefault_ThrowsForbidden));
        var platformDefault = new NotificationTemplate(null, "CommunicationLoss", "Email", "en", null, "body", true);
        await context.NotificationTemplates.AddAsync(platformDefault);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationTemplateWriter(context as IApplicationDbContext, Principal(accountId));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await writer.UpdateNotificationTemplateAsync(
            platformDefault.NotificationTemplateId,
            new NotificationTemplateDto(null, "CommunicationLoss", "Email", "en", null, "hacked", true), CancellationToken.None));
    }

    [Test]
    public async Task DeleteNotificationTemplateAsync_AccountOverride_Deletes()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(DeleteNotificationTemplateAsync_AccountOverride_Deletes));
        var template = new NotificationTemplate(accountId, "CommunicationLoss", "Email", "en", null, "body", true);
        await context.NotificationTemplates.AddAsync(template);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new NotificationTemplateWriter(context as IApplicationDbContext, Principal(accountId));
        await writer.DeleteNotificationTemplateAsync(template.NotificationTemplateId, CancellationToken.None);

        Assert.That(context.NotificationTemplates.Any(), Is.False);
    }
}
