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

namespace TrackHub.Security.Application.Identity.Queries.IsInRole;

[PlatformScoped("Authorization pipeline: role-authorization primitive underlying authorizeUser; IdentityCallerGuard.EnsureCallerIsSubjectUserOrService binds user callers to themselves. Trusted internal services may evaluate any user.")]
public readonly record struct IsInRoleQuery(Guid UserId, string Resource, string Action) : IRequest<bool>;

public class GetUsersQueryHandler(IIdentityService service, IUser user) : IRequestHandler<IsInRoleQuery, bool>
{
    // Handles the IsInRoleQuery by calling the IsInRoleAsync method of the IIdentityService
    public async Task<bool> Handle(IsInRoleQuery request, CancellationToken cancellationToken)
    {
        IdentityCallerGuard.EnsureCallerIsSubjectUserOrService(user, request.UserId, "IsInRole");
        return await service.IsInRoleAsync(request.UserId, request.Resource, request.Action, cancellationToken);
    }
}
