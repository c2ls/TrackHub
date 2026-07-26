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

using TrackHub.Manager.Application.Operators.Queries.Get;
using TrackHub.Manager.Application.Operators.Queries.GetByAccount;
using TrackHub.Manager.Application.Operators.Queries.GetByTransporter;
using TrackHub.Manager.Application.Operators.Queries.GetMaster;
using TrackHub.Manager.Application.Operators.Queries.GetByUser;

namespace TrackHub.Manager.Web.GraphQL.Query;

public partial class Query
{
    public async Task<OperatorVm> GetOperator([Service] ISender sender, [AsParameters] GetOperatorQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<IReadOnlyCollection<OperatorVm>> GetOperatorsMaster([Service] ISender sender, [AsParameters] GetOperatorMasterQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<OperatorsPageVm> GetOperatorsByCurrentAccount([Service] ISender sender, [AsParameters] GetOperatorByCurrentAccountQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);

    public async Task<IReadOnlyCollection<OperatorLookupVm>> GetOperatorLookup([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetOperatorLookupByAccountQuery(), cancellationToken);

    public async Task<IReadOnlyCollection<OperatorVm>> GetOperatorsByUser([Service] ISender sender, CancellationToken cancellationToken)
        => await sender.Send(new GetOperatorByUserQuery(), cancellationToken);

    public async Task<OperatorVm> GetOperatorByTransporter([Service] ISender sender, [AsParameters] GetOperatorByTransporterQuery query, CancellationToken cancellationToken)
        => await sender.Send(query, cancellationToken);
}
