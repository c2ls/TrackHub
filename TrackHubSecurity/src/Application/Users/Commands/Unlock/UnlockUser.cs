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
using TrackHub.Security.Application.Audit.Events;

namespace TrackHub.Security.Application.Users.Commands.Unlock;

[Authorize(Resource = Resources.Users, Action = Actions.Edit)]
// Enforcement: UserWriter.UnlockUserAsync loads the row and calls RequireAccountAccess on its
// owning account before clearing the lockout.
[AccountScopeEnforcedInHandler]
public readonly record struct UnlockUserCommand(Guid UserId) : IRequest;

public class UnlockUserCommandHandler(IUserWriter writer, IUserReader reader, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<UnlockUserCommand>
{
    public async Task Handle(UnlockUserCommand request, CancellationToken cancellationToken)
    {
        await writer.UnlockUserAsync(request.UserId, cancellationToken);
        var user = await reader.GetUserAsync(request.UserId, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "UnlockUser", "User", request.UserId.ToString(), user.AccountId), cancellationToken);
    }
}
