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

[Authorize(Resource = Resources.Users, Action = Actions.Edit)]
public readonly record struct UpdateUserCommand(UpdateUserDto User) : IRequest;
public class UpdateUserCommandHandler(IUserWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<UpdateUserCommand>
{

    /// <summary>
    /// Handle the UpdateUserCommand
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The task result</returns>
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Update the user asynchronously
        await writer.UpdateUserAsync(request.User, cancellationToken);

        // Create a shrank version of the updated user
        var user = new UpdateUserShrankDto(request.User.UserId, request.User.Username, request.User.Active);

        // Publish a notification for the user update
        await publisher.Publish(new UserUpdated.Notification(request.User.UserId, user), cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "UpdateUser", "User", request.User.UserId.ToString(), principal.AccountId), cancellationToken);
    }
}
