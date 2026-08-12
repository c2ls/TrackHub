using Common.Domain.Extensions;
using FluentAssertions;

namespace Common.Domain.Tests.Extensions;

public class DoubleExtensionsTests
{
    [Fact]
    public void FromUnixTimestamp_HasValue_ReturnsCorrectDateTimeOffset()
    {
        double? timestamp = 1672531200; // 2023-01-01 00:00:00 UTC
        var result = timestamp.FromUnixTimestamp();
        result.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1672531200));
    }

    [Fact]
    public void FromUnixTimestamp_Null_ReturnsMinValue()
    {
        double? timestamp = null;
        var result = timestamp.FromUnixTimestamp();
        result.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact]
    public void FromUnixTimestamp_Zero_ReturnsEpoch()
    {
        double? timestamp = 0.0;
        var result = timestamp.FromUnixTimestamp();
        result.Should().Be(DateTimeOffset.UnixEpoch);
    }
}
