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

namespace TrackHub.Security.Application.Users.Commands.Update;
[Authorize(Resource = Resources.Users, Action = Actions.Custom)]
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
            await publisher.Publish(SecurityAudit.Event(principal, "UserPasswordChanged", "User", request.User.UserId.ToString(), principal.AccountId), cancellationToken);
        }
        else
        {
            throw new UnauthorizedAccessException();
        }
    }
}
