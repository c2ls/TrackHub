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

namespace TrackHub.Security.Application.Users.Commands.Delete;

[Authorize(Resource = Resources.Users, Action = Actions.Delete)]
public readonly record struct DeleteUserCommand(Guid Id) : IRequest;

public class DeleteUserCommandHandler(IUserWriter writer, IUser user, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<DeleteUserCommand>
{
    private Guid UserId { get; } = Guid.TryParse(user.Id, out var userId) ? userId : throw new UnauthorizedAccessException();

    // Handle method to delete a user
    // The user cannot delete itself
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == UserId)
        {
            throw new UnauthorizedAccessException("You cannot delete yourself");
        }
        await writer.DeleteUserAsync(request.Id, cancellationToken);
        await publisher.Publish(new UserDeleted.Notification(request.Id), cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "DeleteUser", "User", request.Id.ToString(), principal.AccountId), cancellationToken);
    }

}
