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

using TrackHub.AuthorityServer.Domain.Interfaces;

namespace TrackHub.AuthorityServer.Application.Users.Queries.GetUserRole;

/// <summary>
/// The authenticated user's most privileged role name for the access token's role claim
/// (resource services derive `ICurrentPrincipal.Role` — and with it the privileged
/// account-wide reads and role-addressed notifications — from this claim).
/// </summary>
public readonly record struct GetUserRoleQuery(Guid UserId) : IRequest<string?>;

public class GetUserRoleQueryHandler(IUserReader reader) : IRequestHandler<GetUserRoleQuery, string?>
{
    public async Task<string?> Handle(GetUserRoleQuery request, CancellationToken cancellationToken)
        => await reader.GetUserRoleAsync(request.UserId, cancellationToken);
}
