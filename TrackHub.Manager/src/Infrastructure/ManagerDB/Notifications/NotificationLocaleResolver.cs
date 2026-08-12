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

using Common.Domain.Localization;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

/// <summary>
/// Localization comes from the recipient, not the content: a User
/// recipient's own UserSettings.Language drives the message locale; contact endpoints and
/// webhooks (no user identity behind them) fall back to the rule's configured locale, then
/// English. In-app deliveries are never rendered server-side — the portal localizes them.
/// </summary>
public static class NotificationLocaleResolver
{
    public static async Task<string> ResolveAsync(IApplicationDbContext context, string recipientPrincipalType, string recipient, string? ruleLocale, CancellationToken cancellationToken)
    {
        if (recipientPrincipalType == RecipientPrincipalTypes.User && Guid.TryParse(recipient, out var userId))
        {
            var language = await context.UserSettings
                .Where(s => s.UserId == userId)
                .Select(s => s.Language)
                .FirstOrDefaultAsync(cancellationToken);
            var userLocale = Normalize(language);
            if (userLocale != null)
            {
                return userLocale;
            }
        }

        return Normalize(ruleLocale) ?? NotificationLocales.English;
    }

    /// <summary>Accepts "es", "es-CO", "en-US", … and maps to a supported template locale.</summary>
    public static string? Normalize(string? language)
    {
        var code = ResourceLocalizer.NormalizeLanguage(language);
        return code != null && NotificationLocales.IsValid(code) ? code : null;
    }
}
