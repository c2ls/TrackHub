using Common.Domain.Constants;
using Microsoft.Extensions.Logging.Abstractions;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Services;

namespace Infrastructure.UnitTests;

[TestFixture]
public class AlertRuleEvaluatorTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static AlertRuleEvaluator NewEvaluator(ApplicationDbContext context)
        => new(context as IApplicationDbContext, NullLogger<AlertRuleEvaluator>.Instance);

    private static AlertEventVm Event(Guid accountId, string eventType = "CommunicationLoss")
        => new(Guid.NewGuid(), accountId, eventType, "Warning", "Notifications", "Transporter", Guid.NewGuid().ToString(),
            "Open", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, $"test:{Guid.NewGuid():N}", DateTimeOffset.UtcNow);

    private static async Task SeedFeatureAsync(ApplicationDbContext context, Guid accountId, string featureKey)
    {
        await context.AccountFeatures.AddAsync(new AccountFeature(accountId, featureKey, true, "standard", "manual", null, null, null));
        await context.SaveChangesAsync(CancellationToken.None);
    }

    [Test]
    public async Task EvaluateAsync_MatchingRuleWithSelectorRecipients_CreatesOnePendingDeliveryPerRecipientChannel()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(EvaluateAsync_MatchingRuleWithSelectorRecipients_CreatesOnePendingDeliveryPerRecipientChannel));
        await SeedFeatureAsync(context, accountId, FeatureKeys.Notifications);
        await SeedFeatureAsync(context, accountId, FeatureKeys.NotificationsEmail);
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        await context.NotificationRules.AddAsync(new NotificationRule(accountId, "comm-loss", "Notifications", true, "CommunicationLoss",
            $$"""{"userIds":["{{userId}}"],"roles":["Administrator"],"contacts":[{"channel":"Email","address":"ops@example.com"}],"subscribers":false}""",
            """["InApp","Email"]""", null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var created = await NewEvaluator(context).EvaluateAsync(Event(accountId), CancellationToken.None);

        Assert.That(created, Is.EqualTo(3));
        var deliveries = context.NotificationDeliveries.ToList();
        Assert.That(deliveries, Has.Count.EqualTo(3));
        Assert.That(deliveries, Has.All.Property("Status").EqualTo(DeliveryStatuses.Pending));
        Assert.That(deliveries.Count(d => d.Channel == NotificationChannels.InApp && d.RecipientPrincipalType == RecipientPrincipalTypes.User && d.Recipient == userId.ToString()), Is.EqualTo(1));
        Assert.That(deliveries.Count(d => d.Channel == NotificationChannels.InApp && d.RecipientPrincipalType == RecipientPrincipalTypes.Role && d.Recipient == "Administrator"), Is.EqualTo(1));
        Assert.That(deliveries.Count(d => d.Channel == NotificationChannels.Email && d.RecipientPrincipalType == RecipientPrincipalTypes.Contact && d.Recipient == "ops@example.com"), Is.EqualTo(1));
    }

    [Test]
    public async Task EvaluateAsync_NotificationsFeatureDisabled_CreatesNothing()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(EvaluateAsync_NotificationsFeatureDisabled_CreatesNothing));
        await context.NotificationRules.AddAsync(new NotificationRule(accountId, "comm-loss", "Notifications", true, "CommunicationLoss",
            """{"roles":["Administrator"]}""", """["InApp"]""", null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var created = await NewEvaluator(context).EvaluateAsync(Event(accountId), CancellationToken.None);

        Assert.That(created, Is.Zero);
        Assert.That(context.NotificationDeliveries.Any(), Is.False);
    }

    [Test]
    public async Task EvaluateAsync_DisabledRule_CreatesNothing()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(EvaluateAsync_DisabledRule_CreatesNothing));
        await SeedFeatureAsync(context, accountId, FeatureKeys.Notifications);
        await context.NotificationRules.AddAsync(new NotificationRule(accountId, "comm-loss", "Notifications", false, "CommunicationLoss",
            """{"roles":["Administrator"]}""", """["InApp"]""", null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var created = await NewEvaluator(context).EvaluateAsync(Event(accountId), CancellationToken.None);

        Assert.That(created, Is.Zero);
    }

    [Test]
    public async Task EvaluateAsync_EmailChannelWithoutEntitlement_SkipsEmailKeepsInApp()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(EvaluateAsync_EmailChannelWithoutEntitlement_SkipsEmailKeepsInApp));
        await SeedFeatureAsync(context, accountId, FeatureKeys.Notifications);
        await context.NotificationRules.AddAsync(new NotificationRule(accountId, "comm-loss", "Notifications", true, "CommunicationLoss",
            """{"roles":["Administrator"],"contacts":[{"channel":"Email","address":"ops@example.com"}],"subscribers":false}""",
            """["InApp","Email"]""", null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var created = await NewEvaluator(context).EvaluateAsync(Event(accountId), CancellationToken.None);

        Assert.That(created, Is.EqualTo(1));
        Assert.That(context.NotificationDeliveries.Single().Channel, Is.EqualTo(NotificationChannels.InApp));
    }

    [Test]
    public async Task EvaluateAsync_SameRuleAndEventWithinDedupeWindow_SuppressesRepeatDeliveries()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(EvaluateAsync_SameRuleAndEventWithinDedupeWindow_SuppressesRepeatDeliveries));
        await SeedFeatureAsync(context, accountId, FeatureKeys.Notifications);
        await context.NotificationRules.AddAsync(new NotificationRule(accountId, "comm-loss", "Notifications", true, "CommunicationLoss",
            """{"roles":["Administrator"],"subscribers":false}""", """["InApp"]""", null, null));
        await context.SaveChangesAsync(CancellationToken.None);
        var alertEvent = Event(accountId);
        var evaluator = NewEvaluator(context);

        var first = await evaluator.EvaluateAsync(alertEvent, CancellationToken.None);
        var second = await evaluator.EvaluateAsync(alertEvent, CancellationToken.None);

        Assert.That(first, Is.EqualTo(1));
        Assert.That(second, Is.Zero);
        Assert.That(context.NotificationDeliveries.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task EvaluateAsync_EnabledSubscriptionForEventType_AddsSubscriberDelivery()
    {
        var accountId = Guid.NewGuid();
        var subscriberId = Guid.NewGuid();
        await using var context = NewContext(nameof(EvaluateAsync_EnabledSubscriptionForEventType_AddsSubscriberDelivery));
        await SeedFeatureAsync(context, accountId, FeatureKeys.Notifications);
        await context.AlertSubscriptions.AddAsync(new AlertSubscription(accountId, RecipientPrincipalTypes.User, subscriberId, "CommunicationLoss", NotificationChannels.InApp, null, true));
        await context.NotificationRules.AddAsync(new NotificationRule(accountId, "comm-loss", "Notifications", true, "CommunicationLoss",
            """{"roles":["Administrator"]}""", """["InApp"]""", null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var created = await NewEvaluator(context).EvaluateAsync(Event(accountId), CancellationToken.None);

        Assert.That(created, Is.EqualTo(2));
        Assert.That(context.NotificationDeliveries.Count(d => d.RecipientPrincipalType == RecipientPrincipalTypes.User && d.Recipient == subscriberId.ToString()), Is.EqualTo(1));
    }

    [Test]
    public async Task EvaluateAsync_DigestThrottling_CreatesDeferredDeliveries()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(EvaluateAsync_DigestThrottling_CreatesDeferredDeliveries));
        await SeedFeatureAsync(context, accountId, FeatureKeys.Notifications);
        await context.NotificationRules.AddAsync(new NotificationRule(accountId, "comm-loss", "Notifications", true, "CommunicationLoss",
            """{"roles":["Administrator"],"subscribers":false}""", """["InApp"]""", """{"digest":"Hourly"}""", null));
        await context.SaveChangesAsync(CancellationToken.None);

        await NewEvaluator(context).EvaluateAsync(Event(accountId), CancellationToken.None);

        Assert.That(context.NotificationDeliveries.Single().Status, Is.EqualTo(DeliveryStatuses.Deferred));
    }

    [Test]
    public async Task EvaluateAsync_SuspendedAccount_CreatesNothing()
    {
        // The Account row is seeded in its own context: Account's navigation collections are
        // fixed-size, so tracking it alongside related rows breaks EF InMemory fix-up.
        Guid accountId;
        await using (var seedContext = NewContext(nameof(EvaluateAsync_SuspendedAccount_CreatesNothing)))
        {
            var account = new Account("suspended", "suspended account", 1, active: false);
            await seedContext.Accounts.AddAsync(account);
            await seedContext.SaveChangesAsync(CancellationToken.None);
            accountId = account.AccountId;
        }

        await using var context = NewContext(nameof(EvaluateAsync_SuspendedAccount_CreatesNothing));
        await SeedFeatureAsync(context, accountId, FeatureKeys.Notifications);
        await context.NotificationRules.AddAsync(new NotificationRule(accountId, "comm-loss", "Notifications", true, "CommunicationLoss",
            """{"roles":["Administrator"],"subscribers":false}""", """["InApp"]""", null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var created = await NewEvaluator(context).EvaluateAsync(Event(accountId), CancellationToken.None);

        Assert.That(created, Is.Zero);
        Assert.That(context.NotificationDeliveries.Any(), Is.False);
    }

    [Test]
    public async Task EvaluateAsync_SelectorUserOutsideAccount_IsDropped()
    {
        var accountId = Guid.NewGuid();
        var foreignUserId = Guid.NewGuid();
        await using var context = NewContext(nameof(EvaluateAsync_SelectorUserOutsideAccount_IsDropped));
        await SeedFeatureAsync(context, accountId, FeatureKeys.Notifications);
        await context.Users.AddAsync(new User(foreignUserId, "mallory", true, Guid.NewGuid()));
        await context.NotificationRules.AddAsync(new NotificationRule(accountId, "comm-loss", "Notifications", true, "CommunicationLoss",
            $$"""{"userIds":["{{foreignUserId}}"],"subscribers":false}""", """["InApp"]""", null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var created = await NewEvaluator(context).EvaluateAsync(Event(accountId), CancellationToken.None);

        Assert.That(created, Is.Zero);
    }
}
