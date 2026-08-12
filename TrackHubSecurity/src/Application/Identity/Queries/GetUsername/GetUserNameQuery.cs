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

namespace TrackHub.Security.Application.Identity.Queries.GetUsername;

public readonly record struct GetUserNameQuery(Guid UserId) : IRequest<string>;

public class GetUsersQueryHandler(IIdentityService service, IUser user) : IRequestHandler<GetUserNameQuery, string>
{
    // Handles the GetUserNameQuery by retrieving the username from the identity service
    public async Task<string> Handle(GetUserNameQuery request, CancellationToken cancellationToken)
    {
        IdentityCallerGuard.EnsureCallerIsSubjectUserOrService(user, request.UserId, "UserName");
        return await service.GetUserNameAsync(request.UserId, cancellationToken);
    }
}
