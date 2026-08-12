using Common.Application.Attributes;
using FluentAssertions;

namespace Common.Application.Tests.Attributes;

public class CachingAttributeTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var attr = new CachingAttribute();
        attr.SlidingExpiration.Should().Be(TimeSpan.FromSeconds(60));
        attr.AbsoluteExpiration.Should().BeNull();
        attr.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        var attr = new CachingAttribute
        {
            SlidingExpiration = TimeSpan.FromMinutes(5),
            AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(1),
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };
        attr.SlidingExpiration.Should().Be(TimeSpan.FromMinutes(5));
        attr.AbsoluteExpiration.Should().NotBeNull();
        attr.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(30));
    }
}
