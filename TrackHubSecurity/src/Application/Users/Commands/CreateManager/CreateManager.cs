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

namespace TrackHub.Security.Application.Users.Commands.CreateManager;

[Authorize(Resource = Resources.Administrative, Action = Actions.Write)]
public readonly record struct CreateManagerCommand(CreateUserDto User, Guid AccountId) : IRequest<UserVm>;

public class CreateManagerCommandHandler(IUserWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<CreateManagerCommand, UserVm>
{

    // Handle the Create Manager command
    // This method creates a new user with the role of manager
    public async Task<UserVm> Handle(CreateManagerCommand request, CancellationToken cancellationToken)
    {
        var user = await writer.CreateUserAsync(request.User, request.AccountId, cancellationToken);
        await publisher.Publish(new ManagerCreated.Notification(user.UserId), cancellationToken);
        // Replicate the user in the management service
        var shrankUser = new UserShrankDto(user.UserId, user.Username, request.AccountId);
        await publisher.Publish(new UserCreated.Notification(shrankUser), cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "CreateManager", "User", user.UserId.ToString(), request.AccountId, null, user.Username), cancellationToken);
        return user;
    }
}
