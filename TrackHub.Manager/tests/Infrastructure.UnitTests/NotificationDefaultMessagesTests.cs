using TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

namespace Infrastructure.UnitTests;

// Guards the resx manifest wiring: a wrong embedded-resource name would silently fall back to
// empty strings at runtime.
[TestFixture]
public class NotificationDefaultMessagesTests
{
    [Test]
    public void Get_EnglishAndSpanishBodies_ResolveAndDiffer()
    {
        var english = NotificationDefaultMessages.Get(NotificationDefaultMessages.DefaultAlertBody, "en");
        var spanish = NotificationDefaultMessages.Get(NotificationDefaultMessages.DefaultAlertBody, "es");

        Assert.That(english, Is.Not.Empty);
        Assert.That(spanish, Is.Not.Empty);
        Assert.That(english, Is.Not.EqualTo(spanish));
        Assert.That(english, Does.Contain("{eventType}"));
        Assert.That(spanish, Does.Contain("{eventType}"));
    }

    [TestCase(NotificationDefaultMessages.TestNotificationSubject)]
    [TestCase(NotificationDefaultMessages.TestNotificationBody)]
    [TestCase(NotificationDefaultMessages.NotificationDigestSubject)]
    [TestCase(NotificationDefaultMessages.NotificationDigestBody)]
    [TestCase(NotificationDefaultMessages.DefaultAlertSubject)]
    public void Get_EveryKey_ResolvesInBothLocales(string key)
    {
        Assert.That(NotificationDefaultMessages.Get(key, "en"), Is.Not.Empty);
        Assert.That(NotificationDefaultMessages.Get(key, "es"), Is.Not.Empty);
    }

    [Test]
    public void Get_UnknownLocale_FallsBackToNeutral()
    {
        var value = NotificationDefaultMessages.Get(NotificationDefaultMessages.DefaultAlertSubject, "xx-invalid!");
        Assert.That(value, Is.Not.Empty);
    }
}
