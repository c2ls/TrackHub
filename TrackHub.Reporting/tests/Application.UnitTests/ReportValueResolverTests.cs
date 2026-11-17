using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Application.UnitTests;

[TestFixture]
public class ReportValueResolverTests
{
    private static readonly ReportColumn TripStatus = new("TripStatus", typeof(string), "TripStatus");

    [TestCase("InProgress", "es", "En curso")]
    [TestCase("IN_PROGRESS", "es", "En curso")]
    [TestCase("inprogress", "en", "In progress")]
    [TestCase("InProgress", "", "In progress")]
    [TestCase("Unheard", "es", "Unheard")]
    public void TranslatesVocabularyValues(string value, string language, string expected)
        => Assert.That(ReportValueResolver.Localize(value, TripStatus, ReportCulture.Resolve(language)), Is.EqualTo(expected));

    [Test]
    public void LeavesOtherValuesAlone()
    {
        var spanish = CultureInfo.GetCultureInfo("es");
        Assert.That(ReportValueResolver.Localize("InProgress", new ReportColumn("TripCode", typeof(string)), spanish), Is.EqualTo("InProgress"));
        Assert.That(ReportValueResolver.Localize(null, TripStatus, spanish), Is.Null);
        Assert.That(ReportValueResolver.Localize(string.Empty, TripStatus, spanish), Is.EqualTo(string.Empty));
        Assert.That(ReportValueResolver.Localize(3, TripStatus, spanish), Is.EqualTo(3));
    }

    [Test]
    public void EveryAnnotatedColumnHasTheSameLabelsInEveryLanguage()
    {
        var vocabularies = typeof(ReportColumn).Assembly.GetTypes()
            .SelectMany(t => t.GetProperties())
            .Select(p => p.GetCustomAttribute<ReportValuesAttribute>()?.Vocabulary)
            .OfType<string>()
            .ToHashSet();
        var resources = new ResourceManager("TrackHub.Reporting.Domain.Resources.Resources", typeof(ReportColumn).Assembly);
        HashSet<string> Keys(string language) => resources.GetResourceSet(CultureInfo.GetCultureInfo(language), true, false)!
            .Cast<DictionaryEntry>().Select(e => (string)e.Key)
            .Where(k => vocabularies.Contains(k.Split('_')[0])).ToHashSet();

        Assert.That(vocabularies, Has.Count.GreaterThan(10));
        Assert.That(vocabularies.Where(v => !Keys("en").Any(k => k.StartsWith(v + "_"))), Is.Empty);
        Assert.That(Keys("es"), Is.EquivalentTo(Keys("en")));
    }
}
