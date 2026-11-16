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

namespace TrackHub.Router.Infrastructure.Webfleet.Helpers;

internal static class WebfleetResponse
{
    internal const int NoDataCode = 63;

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    // Data is always a JSON array; the guide does not document the JSON error shape, so any other body is an error.
    public static void EnsureSuccess(JsonElement body, string action)
    {
        if (body.ValueKind == JsonValueKind.Array)
        {
            return;
        }

        if (body.ValueKind != JsonValueKind.Object)
        {
            throw new WebfleetApiException(null, action, $"unexpected {body.ValueKind} response");
        }

        var code = ReadCode(body);
        if (code != NoDataCode)
        {
            throw new WebfleetApiException(code, action, ReadMessage(body) ?? body.GetRawText());
        }
    }

    public static IReadOnlyList<T> ReadList<T>(JsonElement body)
        => body.ValueKind == JsonValueKind.Array
            ? body.Deserialize<List<T>>(Options) ?? []
            : [];

    private static int? ReadCode(JsonElement body)
        => FindProperty(body, "errorCode") switch
        {
            { ValueKind: JsonValueKind.Number } number when number.TryGetInt32(out var value) => value,
            { ValueKind: JsonValueKind.String } text when int.TryParse(text.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) => value,
            _ => null
        };

    private static string? ReadMessage(JsonElement body)
        => FindProperty(body, "errorMsg") is { ValueKind: JsonValueKind.String } message ? message.GetString() : null;

    private static JsonElement? FindProperty(JsonElement body, string name)
        => body.EnumerateObject()
            .Where(property => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .Select(property => (JsonElement?)property.Value)
            .FirstOrDefault();
}
