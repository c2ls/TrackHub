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

namespace TrackHub.Security.Application.UserRole.Commands.Create;

[Authorize(Resource = Resources.Users, Action = Actions.Write)]
public readonly record struct CreateUserRoleCommand(UserRoleDto UserRole) : IRequest<UserRoleVm>;

public class CreateUserRoleCommandHandler(IUserRoleWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<CreateUserRoleCommand, UserRoleVm>
{
    // This method handles the CreateUserRoleCommand by creating a user role using the provided writer.
    // It returns a UserRoleVm.
    public async Task<UserRoleVm> Handle(CreateUserRoleCommand request, CancellationToken cancellationToken)
    {
        var vm = await writer.CreateUserRoleAsync(request.UserRole, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "UserRoleAssigned", "UserRole", $"{request.UserRole.UserId}:{request.UserRole.RoleId}", principal.AccountId), cancellationToken);
        return vm;
    }

}

