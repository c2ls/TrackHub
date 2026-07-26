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

using TrackHub.Manager.Application.Users.Events;

namespace TrackHub.Manager.Application.Users.Commands.Create;

[Authorize(Resource = Resources.Users, Action = Actions.Write)]
[AllowCrossAccount("Security replicates user provisioning here under the ORIGINATING caller's token. The ordinary path is same-account; the cross-account case is the Administrator onboarding a NEW tenant (Security's createManager relay), whose token names the platform operator's account while the replica row belongs to the new tenant. Tenant callers are NOT unguarded by this: UserWriter.RequireReplicaAccess checks the row's account (same-account / global service / Administrator only).")]
public readonly record struct CreateUserCommand(UserDto User) : IRequest<UserVm>;

public class CreateUserCommandHandler(IUserWriter writer, IPublisher publisher) : IRequestHandler<CreateUserCommand, UserVm>
{
    public async Task<UserVm> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    { 
        var user = await writer.CreateUserAsync(request.User, cancellationToken);
        await publisher.Publish(new UserCreated.Notification(user.UserId), cancellationToken);
        return user;
    }
}
