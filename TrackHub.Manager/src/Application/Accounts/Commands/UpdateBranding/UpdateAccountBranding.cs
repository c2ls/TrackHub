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

using System.Text.RegularExpressions;
using Common.Application.Interfaces;
using TrackHub.Manager.Application.Accounts.Events;

namespace TrackHub.Manager.Application.Accounts.Commands.UpdateBranding;

// Account-Administrator own-account branding upsert.
[Authorize(Resource = Resources.Accounts, Action = Actions.Edit)]
public readonly record struct UpdateAccountBrandingCommand(AccountBrandingDto Branding)
    : IRequest<AccountBrandingVm>;

public class UpdateAccountBrandingCommandHandler(IAccountBrandingWriter writer, IPublisher publisher, ICurrentPrincipal principal)
    : IRequestHandler<UpdateAccountBrandingCommand, AccountBrandingVm>
{
    public async Task<AccountBrandingVm> Handle(UpdateAccountBrandingCommand request, CancellationToken cancellationToken)
    {
        var vm = await writer.UpsertBrandingAsync(request.Branding, cancellationToken);

        var actorId = principal.UserId?.ToString() ?? principal.ClientId ?? principal.SubjectId;
        await publisher.Publish(new BrandingChanged.Notification(
            request.Branding.AccountId, actorId, principal.CorrelationId), cancellationToken);

        return vm;
    }
}

public sealed partial class UpdateAccountBrandingValidator : AbstractValidator<UpdateAccountBrandingCommand>
{
    public UpdateAccountBrandingValidator(IAccountBrandingReader brandingReader)
    {
        RuleFor(x => x.Branding.AccountId).NotEmpty();
        RuleFor(x => x.Branding.DisplayName)
            .NotEmpty()
            .MaximumLength(ColumnMetadata.DefaultNameLength);

        RuleFor(x => x.Branding.PrimaryColor)
            .NotEmpty()
            .Must(c => c is not null && HexColorRegex().IsMatch(c))
            .WithMessage("PrimaryColor must be a hex color in the form #RRGGBB.");

        // Logo, if present, must be a Document owned by the same account with owner type AccountBranding.
        RuleFor(x => x.Branding)
            .MustAsync(async (b, ct) => !b.LogoDocumentId.HasValue
                || await brandingReader.LogoDocumentBelongsToAccountAsync(b.AccountId, b.LogoDocumentId.Value, ct))
            .WithMessage("The logo document must be an AccountBranding document owned by the same account.")
            .OverridePropertyName("Branding.LogoDocumentId");
    }

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColorRegex();
}
