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

namespace TrackHub.Router.Domain.Helpers;

// Builds and reads the JSON-object string carried in AttributesVm.Extra. Provider mappers use
// From(...) to project surplus provider signals into the open bag under canonical keys; consumers
// use ToDictionary(...) to read them back (router-audit A-03).
public static class AttributesExtra
{
    // Builds the Extra JSON string from key/value signals, dropping null/whitespace values.
    // Returns null when nothing survives (so an empty bag never travels as "{}").
    public static string? From(params (string Key, string? Value)[] signals)
        => From((IEnumerable<KeyValuePair<string, string?>>)signals
            .Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)));

    public static string? From(IEnumerable<KeyValuePair<string, string?>> signals)
    {
        var kept = signals
            .Where(s => !string.IsNullOrWhiteSpace(s.Key) && !string.IsNullOrWhiteSpace(s.Value))
            .ToDictionary(s => s.Key, s => s.Value);

        return kept.Count == 0 ? null : JsonSerializer.Serialize(kept);
    }

    // Parses an Extra JSON string back into a dictionary; empty/invalid input yields an empty map.
    public static IReadOnlyDictionary<string, string?> ToDictionary(string? extra)
    {
        if (string.IsNullOrWhiteSpace(extra))
        {
            return new Dictionary<string, string?>();
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string?>>(extra)
                ?? new Dictionary<string, string?>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, string?>();
        }
    }
}
