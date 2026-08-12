using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Manager.Application.AlertEvents.Commands;
using TrackHub.Manager.Application.AlertEvents.Events;
using TrackHub.Manager.Application.Notifications.Commands;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Notifications;

[TestFixture]
public class NotificationCommandHandlerTests
{
    private static Mock<IFeatureFlagService> FeatureFlags(bool emailEnabled = true, bool whatsAppEnabled = true)
    {
        var featureFlags = new Mock<IFeatureFlagService>();
        featureFlags.Setup(f => f.IsEnabledAsync(It.IsAny<Guid>(), "notifications.email", It.IsAny<CancellationToken>())).ReturnsAsync(emailEnabled);
        featureFlags.Setup(f => f.IsEnabledAsync(It.IsAny<Guid>(), "notifications.whatsapp", It.IsAny<CancellationToken>())).ReturnsAsync(whatsAppEnabled);
        return featureFlags;
    }

    private static NotificationRuleDto Rule(string channelsJson) =>
        new(Guid.NewGuid(), "key", "Notifications", true, "CommunicationLoss", "", channelsJson, null, null);

    [Test]
    public void CreateNotificationRule_EmailChannelWithoutEntitlement_ThrowsFeatureDisabled()
    {
        var writer = new Mock<INotificationWriter>();
        var handler = new CreateNotificationRuleCommandHandler(writer.Object, FeatureFlags(emailEnabled: false).Object);

        Assert.ThrowsAsync<FeatureDisabledException>(async () =>
            await handler.Handle(new CreateNotificationRuleCommand(Rule("""["InApp","Email"]""")), CancellationToken.None));
        writer.Verify(w => w.CreateNotificationRuleAsync(It.IsAny<NotificationRuleDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task CreateNotificationRule_EntitledChannels_CallsWriter()
    {
        var writer = new Mock<INotificationWriter>();
        var handler = new CreateNotificationRuleCommandHandler(writer.Object, FeatureFlags().Object);

        await handler.Handle(new CreateNotificationRuleCommand(Rule("""["InApp","Email","WhatsApp"]""")), CancellationToken.None);

        writer.Verify(w => w.CreateNotificationRuleAsync(It.IsAny<NotificationRuleDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RecordAlertEvent_PublishesAlertEventRecordedNotification()
    {
        var accountId = Guid.NewGuid();
        var dto = new AlertEventDto(accountId, "CommunicationLoss", "Warning", "Notifications", "Transporter", "id", "Open", null, "dedup");
        var vm = new AlertEventVm(Guid.NewGuid(), accountId, dto.EventType, dto.Severity, dto.SourceModule, dto.ResourceType, dto.ResourceId, dto.Status, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, dto.DeduplicationKey, DateTimeOffset.UtcNow);
        var writer = new Mock<IAlertEventWriter>();
        writer.Setup(w => w.RecordAlertEventAsync(dto, It.IsAny<CancellationToken>())).ReturnsAsync(vm);
        var publisher = new Mock<IPublisher>();
        var handler = new RecordAlertEventCommandHandler(writer.Object, publisher.Object);

        var result = await handler.Handle(new RecordAlertEventCommand(dto), CancellationToken.None);

        Assert.That(result.AlertEventId, Is.EqualTo(vm.AlertEventId));
        publisher.Verify(p => p.Publish(It.Is<AlertEventRecorded.Notification>(n => n.AlertEvent.AlertEventId == vm.AlertEventId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void SendTestNotification_WhatsAppWithoutEntitlement_ThrowsFeatureDisabled()
    {
        var writer = new Mock<INotificationWriter>();
        var principal = new Mock<ICurrentPrincipal>();
        var handler = new SendTestNotificationCommandHandler(writer.Object, FeatureFlags(whatsAppEnabled: false).Object, principal.Object);

        Assert.ThrowsAsync<FeatureDisabledException>(async () =>
            await handler.Handle(new SendTestNotificationCommand(Guid.NewGuid(), "WhatsApp", "+573001234567"), CancellationToken.None));
    }

    [Test]
    public async Task SendTestNotification_InApp_QueuesDeliveryToCurrentUser()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var writer = new Mock<INotificationWriter>();
        NotificationDeliveryDto? captured = null;
        writer.Setup(w => w.CreateNotificationDeliveryAsync(It.IsAny<NotificationDeliveryDto>(), It.IsAny<CancellationToken>()))
            .Callback<NotificationDeliveryDto, CancellationToken>((d, _) => captured = d)
            .ReturnsAsync(default(NotificationDeliveryVm));
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.UserId).Returns(userId);
        var handler = new SendTestNotificationCommandHandler(writer.Object, FeatureFlags().Object, principal.Object);

        await handler.Handle(new SendTestNotificationCommand(accountId, "InApp", null), CancellationToken.None);

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.Value.Recipient, Is.EqualTo(userId.ToString()));
        Assert.That(captured.Value.Status, Is.EqualTo("Pending"));
    }

    [Test]
    public async Task DeleteAlertSubscription_ReturnsDeletedId()
    {
        var subscriptionId = Guid.NewGuid();
        var writer = new Mock<IAlertSubscriptionWriter>();
        var handler = new DeleteAlertSubscriptionCommandHandler(writer.Object);

        var result = await handler.Handle(new DeleteAlertSubscriptionCommand(subscriptionId), CancellationToken.None);

        Assert.That(result, Is.EqualTo(subscriptionId));
        writer.Verify(w => w.DeleteAlertSubscriptionAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
