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

namespace TrackHub.Manager.Infrastructure.ManagerDB.Notifications;

/// <summary>
/// Localized built-in fallback message texts for the notification layer, sourced from
/// NotificationDefaultMessages.resx (+ .es.resx satellite) through the shared
/// <see cref="ResourceLocalizer"/> — no translation strings live in code or the database.
/// Used by <c>NotificationMessageRenderer</c> as the last resolution step and by
/// <c>NotificationTemplateReader</c> to synthesize the read-only platform defaults.
/// </summary>
public static class NotificationDefaultMessages
{
    public const string DefaultAlertSubject = nameof(DefaultAlertSubject);
    public const string DefaultAlertBody = nameof(DefaultAlertBody);
    public const string TestNotificationSubject = nameof(TestNotificationSubject);
    public const string TestNotificationBody = nameof(TestNotificationBody);
    public const string NotificationDigestSubject = nameof(NotificationDigestSubject);
    public const string NotificationDigestBody = nameof(NotificationDigestBody);

    private static readonly ResourceLocalizer Localizer = new(
        "TrackHub.Manager.Infrastructure.ManagerDB.Notifications.NotificationDefaultMessages",
        typeof(NotificationDefaultMessages).Assembly);

    public static string Get(string key, string locale) => Localizer.GetString(key, locale);
}
