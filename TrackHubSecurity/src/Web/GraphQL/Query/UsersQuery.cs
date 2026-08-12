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

using TrackHub.Security.Application.Users.Queries.Get;
using TrackHub.Security.Application.Users.Queries.GetAuthorizedActions;
using TrackHub.Security.Application.Users.Queries.GetByAccount;
using TrackHub.Security.Application.Users.Queries.GetByPolicy;
using TrackHub.Security.Application.Users.Queries.GetByRole;
using TrackHub.Security.Application.Users.Queries.GetAll;
using TrackHub.Security.Application.Users.Queries.GetPermission;
using TrackHub.Security.Domain.Models;

namespace TrackHub.Security.Web.GraphQL.Query;

public partial class Query
{
    public async Task<UserVm> GetUser([Service] ISender sender, [AsParameters] GetUserQuery query)
        => await sender.Send(query);

    public async Task<IReadOnlyCollection<UserVm>> GetUsers([Service] ISender sender, [AsParameters] GetUsersQuery query)
        => await sender.Send(query);

    public async Task<UserVm> GetCurrentUser([Service] ISender sender)
        => await sender.Send(new GetCurrentUserQuery());

    public async Task<IReadOnlyCollection<UserVm>> GetUsersByAccount([Service] ISender sender, [AsParameters] GetUsersByAccountQuery query)
        => await sender.Send(query);

    public async Task<IReadOnlyCollection<UserVm>> GetUsersByRole([Service] ISender sender, [AsParameters] GetUsersByRoleQuery query)
        => await sender.Send(query);

    public async Task<IReadOnlyCollection<UserVm>> GetUsersByPolicy([Service] ISender sender, [AsParameters] GetUsersByPolicyQuery query)
        => await sender.Send(query);

    public async Task<IReadOnlyCollection<ResourceActionVm>> GetAuthorizedActions([Service] ISender sender, [AsParameters] GetAuthorizedActionsQuery query)
        => await sender.Send(query);

    public async Task<bool> UserIsAdmin([Service] ISender sender)
        => await sender.Send(new UserIsAdminQuery());

    public async Task<bool> UserIsManager([Service] ISender sender)
        => await sender.Send(new UserIsManagerQuery());

}
