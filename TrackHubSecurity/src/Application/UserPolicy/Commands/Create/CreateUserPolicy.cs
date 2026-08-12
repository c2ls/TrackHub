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

namespace TrackHub.Security.Application.UserPolicy.Commands.Create;

[Authorize(Resource = Resources.Users, Action = Actions.Write)]
// Enforcement: UserPolicyWriter loads the TARGET user and calls RequireAccountAccess on its owning
// account before writing the grant.
[AccountScopeEnforcedInHandler]
public readonly record struct CreateUserPolicyCommand(UserPolicyDto UserPolicy) : IRequest<UserPolicyVm>;

public class CreateUserPolicyCommandHandler(IUserPolicyWriter writer, IPublisher publisher, ICurrentPrincipal principal) : IRequestHandler<CreateUserPolicyCommand, UserPolicyVm>
{
    // This method handles the execution of the CreateUserPolicyCommand.
    public async Task<UserPolicyVm> Handle(CreateUserPolicyCommand request, CancellationToken cancellationToken)
    {
        var vm = await writer.CreateUserPolicyAsync(request.UserPolicy, cancellationToken);
        await publisher.Publish(SecurityAudit.Event(principal, "UserPolicyAssigned", "UserPolicy", $"{request.UserPolicy.UserId}:{request.UserPolicy.PolicyId}", principal.AccountId), cancellationToken);
        return vm;
    }

}
