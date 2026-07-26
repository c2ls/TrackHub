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

using Common.Application.Extensions;
using Common.Application.GraphQL.Inputs;

namespace TrackHub.Manager.Application.Operators.Queries.GetMaster;

[Authorize(Resource = Resources.OperatorsMaster, Action = Actions.Read)]
[AllowCrossAccount("Master surface: the Router/SyncWorker global service identity (no account claim) enumerates operators across every account to drive the device-sync loop; the portal systemadmin console shares it. The OperatorsMaster resource gates access.")]
public readonly record struct GetOperatorMasterQuery(FiltersInput Filter) : IRequest<IReadOnlyCollection<OperatorVm>>;

public class GetOperatorsMasterQueryHandler(IOperatorReader reader) : IRequestHandler<GetOperatorMasterQuery, IReadOnlyCollection<OperatorVm>>
{
    public async Task<IReadOnlyCollection<OperatorVm>> Handle(GetOperatorMasterQuery request, CancellationToken cancellationToken)
        => await reader.GetOperatorsAsync(request.Filter.GetFilters(), cancellationToken);

}
