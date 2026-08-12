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

using FluentValidation;
using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.AccountSupportGrants.Commands;

public sealed class CreateAccountSupportGrantCommandValidator : AbstractValidator<CreateAccountSupportGrantCommand>
{
    private static readonly TimeSpan MaximumWindow = TimeSpan.FromHours(24);

    public CreateAccountSupportGrantCommandValidator()
    {
        RuleFor(v => v.AccountSupportGrant.AccountId).NotEmpty();
        RuleFor(v => v.AccountSupportGrant.SupportUserId).NotEmpty();
        RuleFor(v => v.AccountSupportGrant.Reason).NotEmpty().MaximumLength(500);
        RuleFor(v => v.AccountSupportGrant.TicketReference).NotEmpty().MaximumLength(150);

        RuleFor(v => v.AccountSupportGrant.AccessLevel)
            .Must(SupportAccessLevels.IsValid)
            .WithMessage($"AccessLevel must be '{SupportAccessLevels.ReadOnly}' or '{SupportAccessLevels.Full}'.");

        RuleFor(v => v.AccountSupportGrant.EndsAt)
            .GreaterThan(v => v.AccountSupportGrant.StartsAt)
            .WithMessage("EndsAt must be later than StartsAt.");

        RuleFor(v => v.AccountSupportGrant)
            .Must(g => g.EndsAt - g.StartsAt <= MaximumWindow)
            .WithMessage("A support grant window may not exceed 24 hours.");
    }
}
