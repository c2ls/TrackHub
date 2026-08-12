using Common.Domain.Extensions;
using FluentAssertions;

namespace Common.Domain.Tests.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void ToIso8601String_DateTimeOffset_FormatsCorrectly()
    {
        var date = new DateTimeOffset(2026, 1, 15, 10, 30, 45, TimeSpan.Zero);
        date.ToIso8601String().Should().Be("2026-01-15T10:30:45Z");
    }

    [Fact]
    public void ToIso8601String_NonUtcOffset_NormalizesToUtc()
    {
        var date = new DateTimeOffset(2026, 1, 15, 10, 30, 45, TimeSpan.FromHours(-5));
        date.ToIso8601String().Should().Be("2026-01-15T15:30:45Z");
    }

    [Fact]
    public void FormatDateTime_NullableDateTimeOffset_Null_ReturnsEmpty()
    {
        DateTimeOffset? date = null;
        date.FormatDateTime().Should().BeEmpty();
    }

    [Fact]
    public void FormatDateTime_NullableDateTimeOffset_HasValue_FormatsCorrectly()
    {
        DateTimeOffset? date = new DateTimeOffset(2026, 3, 20, 14, 25, 30, TimeSpan.Zero);
        date.FormatDateTime().Should().Be("2026-03-20 14:25:30");
    }

    [Fact]
    public void FormatDateTime_DateTimeOffset_FormatsCorrectly()
    {
        var date = new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.Zero);
        date.FormatDateTime().Should().Be("2026-12-31 23:59:59");
    }

    [Fact]
    public void FormatDate_NullableDateTimeOffset_Null_ReturnsEmpty()
    {
        DateTimeOffset? date = null;
        date.FormatDate().Should().BeEmpty();
    }

    [Fact]
    public void FormatDate_NullableDateTimeOffset_HasValue_FormatsCorrectly()
    {
        DateTimeOffset? date = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero);
        date.FormatDate().Should().Be("2026-06-15");
    }

    [Fact]
    public void FormatDate_DateTimeOffset_FormatsCorrectly()
    {
        var date = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        date.FormatDate().Should().Be("2026-01-01");
    }
}
