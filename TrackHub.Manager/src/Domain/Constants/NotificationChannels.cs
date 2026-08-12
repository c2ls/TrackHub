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

namespace TrackHub.Manager.Domain.Constants;

/// <summary>Notification channels. Stored as strings in ChannelsJson and deliveries.</summary>
public static class NotificationChannels
{
    public const string InApp = nameof(InApp);
    public const string Email = nameof(Email);
    public const string Webhook = nameof(Webhook);
    public const string WhatsApp = nameof(WhatsApp);
    public const string Push = nameof(Push);

    public static readonly IReadOnlyCollection<string> All = [InApp, Email, Webhook, WhatsApp, Push];

    public static bool IsValid(string? value) => value != null && All.Contains(value);

    /// <summary>Channels whose delivery Recipient is a contact endpoint (personal data — masked in list VMs).</summary>
    public static bool RecipientIsContact(string channel) => channel is Email or WhatsApp;
}
