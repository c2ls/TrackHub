// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
using TrackHub.Security.Application.Users.Events;

namespace TrackHub.Security.Application.Users.Commands.Update;
[Authorize(Resource = Resources.Users, Action = Actions.Custom)]
// Enforcement: the handler allows self or a same-account manager (UserReader.IsManagerAsync), and
// UserWriter.UpdatePasswordAsync additionally calls RequireAccountAccess on the loaded row.
[AccountScopeEnforcedInHandler]
public readonly record struct UpdatePasswordCommand(UserPasswordDto User) : IRequest;
public class UpdatePasswordCommandHandler(IUserWriter writer, IUserReader reader, IUser user, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<UpdatePasswordCommand>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    // Handle the UpdateUserCommand
    // Update the user password if the user is the same user or the requesting user ia a manager
    public async Task Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        var isManager = false;
        if (request.User.UserId != UserId)
        {
            // Check if the requesting user is a manager of the user to update
            isManager = await reader.IsManagerAsync(request.User.UserId, UserId, cancellationToken);
        }
        if (request.User.UserId == UserId || isManager)
        {
            // Update the user asynchronously
            await writer.UpdatePasswordAsync(request.User, cancellationToken);

            // UpdatePasswordAsync also sets the subject active. When a MANAGER sets/resets a
            // subject's password (the invite-activation flow), propagate that activation to the
            // Manager replica (app.users) so the vw_users views stop hiding the user — mirroring
            // UpdateUser's UserUpdated publish. Only in the manager path: the propagated Manager
            // updateUser requires Users/Edit, which the manager holds but a self-service caller
            // (Users/Custom only) does not — and a self password change never needs the replica
            // flipped, since the caller is already an active, visible user.
            if (isManager)
            {
                var username = await reader.GetUserNameAsync(request.User.UserId, cancellationToken);
                var updatedUser = new UpdateUserShrankDto(request.User.UserId, username, true);
                await publisher.Publish(new UserUpdated.Notification(request.User.UserId, updatedUser), cancellationToken);
            }

            await publisher.Publish(SecurityAudit.Event(principal, "UserPasswordChanged", "User", request.User.UserId.ToString(), principal.AccountId), cancellationToken);
        }
        else
        {
            throw new UnauthorizedAccessException();
        }
    }
}
