using TrackHub.Manager.Application.Notifications.Commands;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Notifications;

[TestFixture]
public class NotificationValidatorTests
{
    private static NotificationRuleDto Rule(
        string channelsJson = """["InApp"]""",
        string recipientSelector = "",
        string? throttlingJson = null,
        string? configurationJson = null)
        => new(Guid.NewGuid(), "key", "Notifications", true, "CommunicationLoss", recipientSelector, channelsJson, throttlingJson, configurationJson);

    [Test]
    public void CreateNotificationRule_LegacyEmptyStrings_AreValid()
    {
        var validator = new CreateNotificationRuleCommandValidator();
        var result = validator.Validate(new CreateNotificationRuleCommand(Rule(channelsJson: "", recipientSelector: "")));
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void CreateNotificationRule_UnknownChannel_Fails()
    {
        var validator = new CreateNotificationRuleCommandValidator();
        var result = validator.Validate(new CreateNotificationRuleCommand(Rule(channelsJson: """["Smoke"]""")));
        Assert.That(result.Errors.Any(e => e.PropertyName == "ChannelsJson"), Is.True);
    }

    [Test]
    public void CreateNotificationRule_MalformedChannelsJson_Fails()
    {
        var validator = new CreateNotificationRuleCommandValidator();
        var result = validator.Validate(new CreateNotificationRuleCommand(Rule(channelsJson: "not-json")));
        Assert.That(result.Errors.Any(e => e.PropertyName == "ChannelsJson"), Is.True);
    }

    [Test]
    public void CreateNotificationRule_ContactWithBadEmail_Fails()
    {
        var validator = new CreateNotificationRuleCommandValidator();
        var result = validator.Validate(new CreateNotificationRuleCommand(Rule(
            recipientSelector: """{"contacts":[{"channel":"Email","address":"not-an-email"}]}""")));
        Assert.That(result.Errors.Any(e => e.PropertyName == "RecipientSelector"), Is.True);
    }

    [Test]
    public void CreateNotificationRule_ContactWithNonE164WhatsAppNumber_Fails()
    {
        var validator = new CreateNotificationRuleCommandValidator();
        var result = validator.Validate(new CreateNotificationRuleCommand(Rule(
            recipientSelector: """{"contacts":[{"channel":"WhatsApp","address":"3001234567"}]}""")));
        Assert.That(result.Errors.Any(e => e.PropertyName == "RecipientSelector"), Is.True);
    }

    [Test]
    public void CreateNotificationRule_ValidSelectorAndThrottling_Passes()
    {
        var validator = new CreateNotificationRuleCommandValidator();
        var result = validator.Validate(new CreateNotificationRuleCommand(Rule(
            channelsJson: """["InApp","WhatsApp"]""",
            recipientSelector: """{"roles":["Administrator"],"contacts":[{"channel":"WhatsApp","address":"+573001234567"}]}""",
            throttlingJson: """{"dedupeWindowMinutes":30,"digest":"Hourly","maxPerHour":10}""")));
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void CreateNotificationRule_InvalidDigestCadence_Fails()
    {
        var validator = new CreateNotificationRuleCommandValidator();
        var result = validator.Validate(new CreateNotificationRuleCommand(Rule(throttlingJson: """{"digest":"Weekly"}""")));
        Assert.That(result.Errors.Any(e => e.PropertyName == "ThrottlingJson"), Is.True);
    }

    [Test]
    public void CreateNotificationRule_WebhookChannelWithoutConfiguration_Fails()
    {
        var validator = new CreateNotificationRuleCommandValidator();
        var result = validator.Validate(new CreateNotificationRuleCommand(Rule(channelsJson: """["Webhook"]""")));
        Assert.That(result.Errors.Any(e => e.PropertyName == "ConfigurationJson"), Is.True);
    }

    [Test]
    public void CreateNotificationRule_WebhookChannelWithUrlAndSecret_Passes()
    {
        var validator = new CreateNotificationRuleCommandValidator();
        var result = validator.Validate(new CreateNotificationRuleCommand(Rule(
            channelsJson: """["Webhook"]""",
            configurationJson: """{"webhookUrl":"https://hooks.example.com/x","webhookSecret":"s3cret"}""")));
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void CreateAlertSubscription_EmailWithoutContact_Fails()
    {
        var validator = new CreateAlertSubscriptionCommandValidator();
        var result = validator.Validate(new CreateAlertSubscriptionCommand(
            new AlertSubscriptionDto(Guid.NewGuid(), "User", Guid.NewGuid(), null, "Email", null, true)));
        Assert.That(result.Errors.Any(e => e.PropertyName == "Contact"), Is.True);
    }

    [Test]
    public void CreateAlertSubscription_DriverWhatsAppWithoutContact_PassesForPhoneDefault()
    {
        var validator = new CreateAlertSubscriptionCommandValidator();
        var result = validator.Validate(new CreateAlertSubscriptionCommand(
            new AlertSubscriptionDto(Guid.NewGuid(), "Driver", Guid.NewGuid(), null, "WhatsApp", null, true)));
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void CreateAlertSubscription_UnregisteredEventType_Fails()
    {
        var validator = new CreateAlertSubscriptionCommandValidator();
        var result = validator.Validate(new CreateAlertSubscriptionCommand(
            new AlertSubscriptionDto(Guid.NewGuid(), "User", Guid.NewGuid(), "NotARealEvent", "InApp", null, true)));
        Assert.That(result.Errors.Any(e => e.PropertyName == "EventTypeFilter"), Is.True);
    }

    [Test]
    public void CreateAlertSubscription_WebhookChannel_Fails()
    {
        var validator = new CreateAlertSubscriptionCommandValidator();
        var result = validator.Validate(new CreateAlertSubscriptionCommand(
            new AlertSubscriptionDto(Guid.NewGuid(), "User", Guid.NewGuid(), null, "Webhook", null, true)));
        Assert.That(result.Errors.Any(e => e.PropertyName == "Channel"), Is.True);
    }

    [Test]
    public void CreateNotificationTemplate_MissingAccountId_Fails()
    {
        var validator = new CreateNotificationTemplateCommandValidator();
        var result = validator.Validate(new CreateNotificationTemplateCommand(
            new NotificationTemplateDto(null, "CommunicationLoss", "Email", "en", null, "body", true)));
        Assert.That(result.Errors.Any(e => e.PropertyName == "AccountId"), Is.True);
    }

    [Test]
    public void CreateNotificationTemplate_UnsupportedLocale_Fails()
    {
        var validator = new CreateNotificationTemplateCommandValidator();
        var result = validator.Validate(new CreateNotificationTemplateCommand(
            new NotificationTemplateDto(Guid.NewGuid(), "CommunicationLoss", "Email", "fr", null, "body", true)));
        Assert.That(result.Errors.Any(e => e.PropertyName == "Locale"), Is.True);
    }

    [Test]
    public void SendTestNotification_EmailWithInvalidContact_Fails()
    {
        var validator = new SendTestNotificationCommandValidator();
        var result = validator.Validate(new SendTestNotificationCommand(Guid.NewGuid(), "Email", "nope"));
        Assert.That(result.Errors.Any(e => e.PropertyName == "Contact"), Is.True);
    }

    [Test]
    public void SendTestNotification_PushChannel_Fails()
    {
        var validator = new SendTestNotificationCommandValidator();
        var result = validator.Validate(new SendTestNotificationCommand(Guid.NewGuid(), "Push", null));
        Assert.That(result.Errors.Any(e => e.PropertyName == "Channel"), Is.True);
    }
}
