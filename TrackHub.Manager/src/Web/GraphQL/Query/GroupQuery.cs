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

using TrackHub.Manager.Application.Groups.Queries.Get;
using TrackHub.Manager.Application.Groups.Queries.GetByAccount;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<GroupVm> GetGroup([Service] ISender sender, [AsParameters] GetGroupQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<GroupsPageVm> GetGroupsByAccount([Service] ISender sender, [AsParameters] GetGroupByAccountQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<IReadOnlyCollection<GroupLookupVm>> GetGroupLookup([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetGroupLookupByAccountQuery(), cancellationToken);
}
