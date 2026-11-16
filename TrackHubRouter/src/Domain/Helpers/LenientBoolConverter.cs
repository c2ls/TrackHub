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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace TrackHub.Router.Domain.Helpers;

// A flag a provider types loosely can arrive as true/false, 0/1 or a quoted value.
public sealed class LenientBoolConverter : JsonConverter<bool?>
{
    public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => reader.GetDouble() != 0,
            JsonTokenType.String => reader.GetString()?.Trim().ToLowerInvariant() switch
            {
                "true" or "1" => true,
                "false" or "0" => false,
                _ => null
            },
            _ => null
        };

    public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
    {
        if (value is { } flag)
        {
            writer.WriteBooleanValue(flag);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
