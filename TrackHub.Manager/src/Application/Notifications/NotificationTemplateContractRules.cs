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

/// <summary>Shared validation for the create/update template commands.</summary>
public static class NotificationTemplateContractRules
{
    public static void Apply<T>(AbstractValidator<T> validator, Func<T, NotificationTemplateDto> selector)
    {
        // Account overrides only through this surface; platform defaults are resource-synthesized.
        validator.RuleFor(x => selector(x).AccountId).NotNull().OverridePropertyName("AccountId")
            .WithMessage("Templates created through this surface are account overrides and require an accountId.");
        validator.RuleFor(x => selector(x).TemplateKey).NotEmpty().MaximumLength(255).OverridePropertyName("TemplateKey");
        validator.RuleFor(x => selector(x).Channel).Must(NotificationChannels.IsValid).OverridePropertyName("Channel")
            .WithMessage("channel must be one of InApp, Email, Webhook, WhatsApp, Push.");
        validator.RuleFor(x => selector(x).Locale).Must(NotificationLocales.IsValid).OverridePropertyName("Locale")
            .WithMessage("locale must be en or es.");
        validator.RuleFor(x => selector(x).Subject).MaximumLength(512).OverridePropertyName("Subject");
        validator.RuleFor(x => selector(x).Body).NotEmpty().OverridePropertyName("Body");
    }
}
