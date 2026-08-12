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

namespace TrackHub.Security.Application.Identity.Queries.IsValidService;

// Authorization-pipeline primitive: every service validates incoming service tokens through this
// query under the token being validated — including account-less global service identities, which
// is why the tenant guard must not require an account here.
[PlatformScoped("Authorization pipeline: a service client validating its own registration; IdentityCallerGuard.EnsureCallerIsSubjectService binds the subject to the calling client. The client registry is platform-owned.")]
public readonly record struct IsValidServiceQuery(string? Client) : IRequest<bool>;

public class GetUsersQueryHandler(IIdentityService service, IUser user) : IRequestHandler<IsValidServiceQuery, bool>
{
    // Handles the IsValidServiceQuery by calling the IsValidServiceAsync method of the IIdentityService
    public async Task<bool> Handle(IsValidServiceQuery request, CancellationToken cancellationToken)
    {
        IdentityCallerGuard.EnsureCallerIsSubjectService(user, request.Client, "IsValidService");
        return await service.IsValidServiceAsync(request.Client, cancellationToken);
    }
}
