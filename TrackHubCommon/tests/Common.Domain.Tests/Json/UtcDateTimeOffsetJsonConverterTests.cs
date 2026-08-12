using System.Text.Json;
using Common.Domain.Json;
using FluentAssertions;

namespace Common.Domain.Tests.Json;

/// <summary>
/// Locks in the UTC-timestamp ingestion policy: external timestamps must materialize as UTC
/// DateTimeOffset values regardless of wire shape, never shifted by the machine-local offset.
/// </summary>
public class UtcDateTimeOffsetJsonConverterTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        Converters =
        {
            new UtcDateTimeOffsetJsonConverter(),
            new UtcNullableDateTimeOffsetJsonConverter()
        }
    };

    [Fact]
    public void Naive_string_is_assumed_utc_not_machine_local()
    {
        var result = JsonSerializer.Deserialize<DateTimeOffset>("\"2024-06-01T12:00:00\"", Options);

        result.Offset.Should().Be(TimeSpan.Zero, "naive timestamps must be treated as UTC");
        result.Should().Be(new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Offset_bearing_string_is_normalized_to_utc()
    {
        var result = JsonSerializer.Deserialize<DateTimeOffset>("\"2024-06-01T12:00:00+02:00\"", Options);

        result.Offset.Should().Be(TimeSpan.Zero);
        result.Should().Be(new DateTimeOffset(2024, 6, 1, 10, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Zulu_string_is_utc()
    {
        var result = JsonSerializer.Deserialize<DateTimeOffset>("\"2024-06-01T12:00:00Z\"", Options);

        result.Should().Be(new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Numeric_value_is_treated_as_unix_epoch_seconds_utc()
    {
        // 2024-06-01T12:00:00Z == 1717243200 unix seconds
        var result = JsonSerializer.Deserialize<DateTimeOffset>("1717243200", Options);

        result.Should().Be(new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Nullable_null_deserializes_to_null()
    {
        var result = JsonSerializer.Deserialize<DateTimeOffset?>("null", Options);

        result.Should().BeNull();
    }

    [Fact]
    public void Nullable_naive_string_is_assumed_utc()
    {
        var result = JsonSerializer.Deserialize<DateTimeOffset?>("\"2024-06-01T12:00:00\"", Options);

        result.Should().NotBeNull();
        result!.Value.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Write_emits_utc_normalized_value()
    {
        var value = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.FromHours(2));

        var json = JsonSerializer.Serialize(value, Options);

        json.Should().Be("\"2024-06-01T10:00:00+00:00\"");
    }
}
