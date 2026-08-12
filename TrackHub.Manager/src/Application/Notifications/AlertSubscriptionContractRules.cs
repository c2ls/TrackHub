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

using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.Notifications;

/// <summary>Shared validation for the create/update subscription commands.</summary>
public static class AlertSubscriptionContractRules
{
    // Subscriptions target person channels; Webhook is rule-level and Push is not yet available.
    private static readonly IReadOnlyCollection<string> SubscribableChannels =
        [NotificationChannels.InApp, NotificationChannels.Email, NotificationChannels.WhatsApp];

    public static void Apply<T>(AbstractValidator<T> validator, Func<T, AlertSubscriptionDto> selector)
    {
        validator.RuleFor(x => selector(x).AccountId).NotEmpty().OverridePropertyName("AccountId");
        validator.RuleFor(x => selector(x).PrincipalId).NotEmpty().OverridePropertyName("PrincipalId");
        validator.RuleFor(x => selector(x).PrincipalType)
            .Must(t => t is RecipientPrincipalTypes.User or RecipientPrincipalTypes.Driver)
            .OverridePropertyName("PrincipalType")
            .WithMessage("principalType must be User or Driver.");
        validator.RuleFor(x => selector(x).Channel)
            .Must(SubscribableChannels.Contains)
            .OverridePropertyName("Channel")
            .WithMessage("channel must be InApp, Email, or WhatsApp.");
        validator.RuleFor(x => selector(x).EventTypeFilter)
            .Must(f => f == null || AlertEventTypes.All.Contains(f))
            .OverridePropertyName("EventTypeFilter")
            .WithMessage("eventTypeFilter must be a registered alert event type or null for all types.");
        validator.RuleFor(x => selector(x)).Must(HaveValidContact).OverridePropertyName("Contact")
            .WithMessage("Email subscriptions require a valid email contact; WhatsApp contacts must be E.164 (driver subscriptions may omit it to default from the driver's phone).");
    }

    public static bool HaveValidContact(AlertSubscriptionDto dto) => dto.Channel switch
    {
        NotificationChannels.Email => NotificationRuleContracts.IsEmail(dto.Contact),
        NotificationChannels.WhatsApp => dto.Contact == null
            ? dto.PrincipalType == RecipientPrincipalTypes.Driver
            : NotificationRuleContracts.IsE164(dto.Contact),
        _ => true
    };
}
