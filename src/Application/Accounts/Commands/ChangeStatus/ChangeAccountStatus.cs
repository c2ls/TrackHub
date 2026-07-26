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
using Common.Domain.Enums;
using TrackHub.Manager.Application.Accounts.Events;
using TrackHub.Manager.Domain.Constants;

namespace TrackHub.Manager.Application.Accounts.Commands.ChangeStatus;

// SuperAdministrator-only account lifecycle transition. [AllowSuspendedAccount] so a
// non-operational account can be reinstated/cancelled/archived.
[Authorize(Resource = Resources.Administrative, Action = Actions.Write)]
[AllowSuspendedAccount]
[AllowCrossAccount("SuperAdministrator account lifecycle: suspending/reinstating/cancelling an account is by definition performed on an account other than the platform operator's own. Gated by the Administrative/Write platform permission.")]
public readonly record struct ChangeAccountStatusCommand(Guid AccountId, AccountStatus TargetStatus, string? Reason)
    : IRequest<AccountVm>;

public class ChangeAccountStatusCommandHandler(IAccountStatusWriter writer, IPublisher publisher, ICurrentPrincipal principal)
    : IRequestHandler<ChangeAccountStatusCommand, AccountVm>
{
    public async Task<AccountVm> Handle(ChangeAccountStatusCommand request, CancellationToken cancellationToken)
    {
        var (account, previous) = await writer.ChangeStatusAsync(
            request.AccountId, request.TargetStatus, request.Reason, cancellationToken);

        var actorId = principal.UserId?.ToString() ?? principal.ClientId ?? principal.SubjectId;
        await publisher.Publish(new AccountStatusChanged.Notification(
            request.AccountId, previous, request.TargetStatus, request.Reason, actorId, principal.CorrelationId),
            cancellationToken);

        return account;
    }
}

public sealed class ChangeAccountStatusValidator : AbstractValidator<ChangeAccountStatusCommand>
{
    public ChangeAccountStatusValidator(IAccountOperationalStatusReader statusReader)
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.TargetStatus).IsInEnum();

        // Enforce the transition table. A missing account passes here so the writer surfaces a 404
        // instead of a misleading "transition not allowed" 400.
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
            {
                var current = await statusReader.GetAccountStatusAsync(cmd.AccountId, ct);
                return current is null || AccountStatusTransitions.IsAllowed(current.Value, cmd.TargetStatus);
            })
            .WithMessage("The requested account status transition is not allowed.")
            .OverridePropertyName(nameof(ChangeAccountStatusCommand.TargetStatus));

        RuleFor(x => x.Reason)
            .NotEmpty()
            .When(x => AccountStatusTransitions.RequiresReason(x.TargetStatus))
            .WithMessage("A reason is required when suspending or cancelling an account.");
    }
}
