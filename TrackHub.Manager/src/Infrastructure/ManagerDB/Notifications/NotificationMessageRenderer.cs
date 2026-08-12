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

using System.Text;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

/// <summary>
/// Key-addressed template resolution and token substitution.
/// Resolution order: account override → platform default (AccountId = null) → built-in fallback.
/// Channel-safe content rule: bodies carry a short summary and a portal link, never file contents
/// or secrets.
/// </summary>
public static class NotificationMessageRenderer
{
    public const string TestTemplateKey = "TestNotification";
    public const string DigestTemplateKey = "NotificationDigest";

    public static async Task<(string? Subject, string Body)> RenderAsync(
        IApplicationDbContext context,
        Guid accountId,
        string templateKey,
        string channel,
        string locale,
        IReadOnlyDictionary<string, string> tokens,
        CancellationToken cancellationToken)
    {
        if (!NotificationLocales.IsValid(locale))
        {
            locale = NotificationLocales.English;
        }

        var template = await ResolveAsync(context, accountId, templateKey, channel, locale, cancellationToken);
        var subject = Substitute(template?.Subject ?? FallbackSubject(templateKey, locale), tokens);
        var body = Substitute(template?.Body ?? FallbackBody(templateKey, locale), tokens);
        return (subject, body);
    }

    private static async Task<NotificationTemplate?> ResolveAsync(IApplicationDbContext context, Guid accountId, string templateKey, string channel, string locale, CancellationToken cancellationToken)
    {
        var candidates = await context.NotificationTemplates
            .Where(t => t.TemplateKey == templateKey && t.Channel == channel && t.Locale == locale && t.Active
                && (t.AccountId == accountId || t.AccountId == null))
            .ToListAsync(cancellationToken);
        return candidates.FirstOrDefault(t => t.AccountId == accountId)
            ?? candidates.FirstOrDefault(t => t.AccountId == null);
    }

    public static string Substitute(string text, IReadOnlyDictionary<string, string> tokens)
    {
        var builder = new StringBuilder(text);
        foreach (var (token, value) in tokens)
        {
            builder.Replace($"{{{token}}}", value);
        }
        return builder.ToString();
    }

    // Fallback texts come from NotificationDefaultMessages.resx (locale-resolved) — never from code.
    private static string FallbackSubject(string templateKey, string locale) => NotificationDefaultMessages.Get(templateKey switch
    {
        TestTemplateKey => NotificationDefaultMessages.TestNotificationSubject,
        DigestTemplateKey => NotificationDefaultMessages.NotificationDigestSubject,
        _ => NotificationDefaultMessages.DefaultAlertSubject
    }, locale);

    private static string FallbackBody(string templateKey, string locale) => NotificationDefaultMessages.Get(templateKey switch
    {
        TestTemplateKey => NotificationDefaultMessages.TestNotificationBody,
        DigestTemplateKey => NotificationDefaultMessages.NotificationDigestBody,
        _ => NotificationDefaultMessages.DefaultAlertBody
    }, locale);
}
