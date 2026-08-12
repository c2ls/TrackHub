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

using TrackHub.Manager.Application.Users.Queries.Get;
using TrackHub.Manager.Application.Users.Queries.GetByGroup;
using TrackHub.Manager.Application.Users.Queries.GetSettings;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<UserVm> GetUser([Service] ISender sender, [AsParameters] GetUserQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<UserSettingsVm> GetUserSettings([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetUserSettingsQuery(), cancellationToken);

    public async Task<UsersPageVm> GetUsersByGroup([Service] ISender sender, [AsParameters] GetUsersByGroupQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<IReadOnlyCollection<UserLookupVm>> GetUserLookup([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetUserLookupByAccountQuery(), cancellationToken);
}
