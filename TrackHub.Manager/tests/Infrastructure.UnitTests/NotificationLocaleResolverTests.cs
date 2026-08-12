using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

namespace Infrastructure.UnitTests;

[TestFixture]
public class NotificationLocaleResolverTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    [Test]
    public async Task ResolveAsync_UserRecipientWithLanguageSetting_UsesTheirLanguage()
    {
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(ResolveAsync_UserRecipientWithLanguageSetting_UsesTheirLanguage));
        await context.UserSettings.AddAsync(new UserSettings(userId) { Language = "es" });
        await context.SaveChangesAsync(CancellationToken.None);

        var locale = await NotificationLocaleResolver.ResolveAsync(context as IApplicationDbContext, RecipientPrincipalTypes.User, userId.ToString(), "en", CancellationToken.None);

        Assert.That(locale, Is.EqualTo("es"));
    }

    [Test]
    public async Task ResolveAsync_UserLanguageWithRegionSuffix_NormalizesToSupportedLocale()
    {
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(ResolveAsync_UserLanguageWithRegionSuffix_NormalizesToSupportedLocale));
        await context.UserSettings.AddAsync(new UserSettings(userId) { Language = "es-CO" });
        await context.SaveChangesAsync(CancellationToken.None);

        var locale = await NotificationLocaleResolver.ResolveAsync(context as IApplicationDbContext, RecipientPrincipalTypes.User, userId.ToString(), null, CancellationToken.None);

        Assert.That(locale, Is.EqualTo("es"));
    }

    [Test]
    public async Task ResolveAsync_UserWithoutSettings_FallsBackToRuleLocale()
    {
        await using var context = NewContext(nameof(ResolveAsync_UserWithoutSettings_FallsBackToRuleLocale));

        var locale = await NotificationLocaleResolver.ResolveAsync(context as IApplicationDbContext, RecipientPrincipalTypes.User, Guid.NewGuid().ToString(), "es", CancellationToken.None);

        Assert.That(locale, Is.EqualTo("es"));
    }

    [Test]
    public async Task ResolveAsync_ContactRecipient_UsesRuleLocaleThenEnglish()
    {
        await using var context = NewContext(nameof(ResolveAsync_ContactRecipient_UsesRuleLocaleThenEnglish));

        var withRuleLocale = await NotificationLocaleResolver.ResolveAsync(context as IApplicationDbContext, RecipientPrincipalTypes.Contact, "a@b.com", "es", CancellationToken.None);
        var withoutRuleLocale = await NotificationLocaleResolver.ResolveAsync(context as IApplicationDbContext, RecipientPrincipalTypes.Contact, "a@b.com", null, CancellationToken.None);
        var unsupportedRuleLocale = await NotificationLocaleResolver.ResolveAsync(context as IApplicationDbContext, RecipientPrincipalTypes.Contact, "a@b.com", "fr", CancellationToken.None);

        Assert.That(withRuleLocale, Is.EqualTo("es"));
        Assert.That(withoutRuleLocale, Is.EqualTo("en"));
        Assert.That(unsupportedRuleLocale, Is.EqualTo("en"));
    }
}
