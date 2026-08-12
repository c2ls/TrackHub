using Common.Domain.Localization;
using FluentAssertions;

namespace Common.Domain.Tests.Localization;

public class ResourceLocalizerTests
{
    private static ResourceLocalizer Localizer()
        => new("Common.Domain.Tests.Localization.TestMessages", typeof(ResourceLocalizerTests).Assembly);

    [Fact]
    public void GetString_ExplicitLocale_ResolvesTheSatelliteTranslation()
    {
        Localizer().GetString("Greeting", "es").Should().Be("Hola");
    }

    [Theory]
    [InlineData("en")]
    [InlineData(null)]
    [InlineData("xx-not-a-culture!")]
    public void GetString_NeutralUnknownOrMissingLocale_FallsBackToTheNeutralResource(string? locale)
    {
        Localizer().GetString("Greeting", locale).Should().Be("Hello");
    }

    [Fact]
    public void GetString_MissingKey_ReturnsEmptyInsteadOfThrowing()
    {
        Localizer().GetString("NoSuchKey", "es").Should().BeEmpty();
    }

    [Theory]
    [InlineData("es", "es")]
    [InlineData("ES", "es")]
    [InlineData("es-CO", "es")]
    [InlineData("en_US", "en")]
    [InlineData("EN-us", "en")]
    public void NormalizeLanguage_KnownShapes_ReturnsTwoLetterLowercaseCode(string input, string expected)
    {
        ResourceLocalizer.NormalizeLanguage(input).Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1")]
    [InlineData("42-XX")]
    public void NormalizeLanguage_EmptyOrUnusable_ReturnsNull(string? input)
    {
        ResourceLocalizer.NormalizeLanguage(input).Should().BeNull();
    }
}
