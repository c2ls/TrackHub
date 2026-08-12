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

using Common.Application.Interfaces;
using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.Notifications;

/// <summary>
/// Per-channel billable entitlements: Email and WhatsApp each require their own
/// feature key. Checked when a rule/subscription/test configures the channel and re-checked at
/// dispatch time by the dispatcher.
/// </summary>
public static class NotificationChannelEntitlements
{
    public static string? RequiredFeatureKey(string channel) => channel switch
    {
        NotificationChannels.Email => FeatureKeys.NotificationsEmail,
        NotificationChannels.WhatsApp => FeatureKeys.NotificationsWhatsApp,
        _ => null
    };

    public static async Task RequireConfiguredChannelsAsync(IFeatureFlagService featureFlags, Guid accountId, IEnumerable<string> channels, CancellationToken cancellationToken)
    {
        foreach (var channel in channels.Distinct())
        {
            var featureKey = RequiredFeatureKey(channel);
            if (featureKey != null && !await featureFlags.IsEnabledAsync(accountId, featureKey, cancellationToken))
            {
                throw new FeatureDisabledException(featureKey, accountId);
            }
        }
    }
}
