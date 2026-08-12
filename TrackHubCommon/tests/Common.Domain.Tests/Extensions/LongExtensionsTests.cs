using Common.Domain.Extensions;
using FluentAssertions;

namespace Common.Domain.Tests.Extensions;

public class LongExtensionsTests
{
    [Fact]
    public void FromUnixTimestamp_ReturnsCorrectDateTimeOffset()
    {
        long timestamp = 1672531200;
        var result = timestamp.FromUnixTimestamp();
        result.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1672531200));
    }

    [Fact]
    public void FromUnixTimestamp_Zero_ReturnsEpoch()
    {
        long timestamp = 0;
        var result = timestamp.FromUnixTimestamp();
        result.Should().Be(DateTimeOffset.UnixEpoch);
    }
}
