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

using TrackHub.Manager.Application.Transporters.Queries.Get;
using TrackHub.Manager.Application.Transporters.Queries.GetByAccount;
using TrackHub.Manager.Application.Transporters.Queries.GetByGroup;
using TrackHub.Manager.Application.Transporters.Queries.GetByUser;
using TrackHub.Manager.Application.Transporters.Queries.GetLookup;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<TransporterVm> GetTransporter([Service] ISender sender, [AsParameters] GetTransporterQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<TransportersPageVm> GetTransportersByAccount([Service] ISender sender, [AsParameters] GetTransportersByAccountQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<TransportersPageVm> GetTransportersByGroup([Service] ISender sender, [AsParameters] GetTransporterByGroupQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<TransportersPageVm> GetTransportersByUser([Service] ISender sender, [AsParameters] GetTransporterByUserQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<IReadOnlyCollection<TransporterLookupVm>> GetTransporterLookupByAccount([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetTransporterLookupByAccountQuery(), cancellationToken);

    public async Task<IReadOnlyCollection<TransporterLookupVm>> GetTransporterLookupByUser([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetTransporterLookupByUserQuery(), cancellationToken);

}
