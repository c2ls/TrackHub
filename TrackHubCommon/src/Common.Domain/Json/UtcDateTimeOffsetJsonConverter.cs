// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Domain.Json;

// UTC-timestamp ingestion policy (rules.md → Timestamps).
//
// External payloads must be materialized as UTC DateTimeOffset values at the JSON boundary.
// The default System.Text.Json behaviour for a zone-less string is to apply the MACHINE-LOCAL
// offset, which is forbidden: it silently shifts every naive timestamp by the offset of
// whatever host happens to run the process. This converter fixes all wire shapes uniformly:
//   * a string with an explicit offset ("Z" / "+02:00") -> the offset is honoured and the instant
//     is normalized to UTC (AdjustToUniversal);
//   * a naive string (no offset) -> it is ASSUMED to already be UTC (AssumeUniversal), never local;
//   * a JSON number -> treated as Unix epoch seconds, which is inherently UTC.
// The result always carries a zero offset (UTC).
internal static class UtcDateTimeOffset
{
    private const DateTimeStyles Styles =
        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

    public static DateTimeOffset Read(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                // Epochs are inherently UTC.
                return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64());
            case JsonTokenType.String:
                var value = reader.GetString();
                return string.IsNullOrEmpty(value)
                    ? DateTimeOffset.MinValue
                    : DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, Styles);
            default:
                throw new JsonException($"Unexpected token '{reader.TokenType}' when parsing a UTC timestamp.");
        }
    }

    public static void Write(Utf8JsonWriter writer, DateTimeOffset value)
        => writer.WriteStringValue(value.ToUniversalTime());
}

/// <summary>
/// Deserializes timestamps as UTC <see cref="DateTimeOffset"/> values, assuming UTC for
/// naive (zone-less) inputs instead of the machine-local offset.
/// </summary>
public sealed class UtcDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => UtcDateTimeOffset.Read(ref reader);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        => UtcDateTimeOffset.Write(writer, value);
}

/// <summary>
/// Nullable counterpart of <see cref="UtcDateTimeOffsetJsonConverter"/>.
/// </summary>
public sealed class UtcNullableDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? null : UtcDateTimeOffset.Read(ref reader);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            UtcDateTimeOffset.Write(writer, value.Value);
        }
    }
}
