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
using System.Text.RegularExpressions;

namespace TrackHub.Manager.Domain.Records;

/// <summary>
/// Formalized JSON contracts for the NotificationRule columns. Existing rows written
/// as '' by the legacy portal dialog parse as empty (no channels / no recipients) — no migration.
/// </summary>
public static partial class NotificationRuleContracts
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [GeneratedRegex(@"^\+[1-9]\d{6,14}$")]
    private static partial Regex E164Regex();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();

    public static bool IsE164(string? value) => value != null && E164Regex().IsMatch(value);
    public static bool IsEmail(string? value) => value != null && EmailRegex().IsMatch(value);

    /// <summary>Parses ChannelsJson. Empty/blank input yields an empty list; invalid JSON throws JsonException.</summary>
    public static IReadOnlyCollection<string> ParseChannels(string? channelsJson)
        => string.IsNullOrWhiteSpace(channelsJson)
            ? []
            : JsonSerializer.Deserialize<List<string>>(channelsJson, Options) ?? [];

    public static RecipientSelectorModel ParseRecipientSelector(string? recipientSelector)
        => string.IsNullOrWhiteSpace(recipientSelector)
            ? new RecipientSelectorModel()
            : JsonSerializer.Deserialize<RecipientSelectorModel>(recipientSelector, Options) ?? new RecipientSelectorModel();

    public static ThrottlingModel ParseThrottling(string? throttlingJson)
        => string.IsNullOrWhiteSpace(throttlingJson)
            ? new ThrottlingModel()
            : JsonSerializer.Deserialize<ThrottlingModel>(throttlingJson, Options) ?? new ThrottlingModel();

    public static RuleConfigurationModel ParseConfiguration(string? configurationJson)
        => string.IsNullOrWhiteSpace(configurationJson)
            ? new RuleConfigurationModel()
            : JsonSerializer.Deserialize<RuleConfigurationModel>(configurationJson, Options) ?? new RuleConfigurationModel();
}
