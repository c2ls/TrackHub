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

namespace TrackHub.Manager.Application.DeviceTransporter.Queries.GetMaster;

[Authorize(Resource = Resources.DevicesMaster, Action = Actions.Read)]
[AllowCrossAccount("Master surface: the Router/SyncWorker global service identity (no account claim) reads device-transporter pairs across every account for position routing; the portal systemadmin console shares it. The DevicesMaster resource gates access.")]
public readonly record struct GetDeviceTransporterMasterQuery(FiltersInput Filter) : IRequest<IReadOnlyCollection<DeviceTransporterVm>>;

public class GetDeviceTransporterQueryHandler(IDeviceTransporterReader reader) : IRequestHandler<GetDeviceTransporterMasterQuery, IReadOnlyCollection<DeviceTransporterVm>>
{
    public async Task<IReadOnlyCollection<DeviceTransporterVm>> Handle(GetDeviceTransporterMasterQuery request, CancellationToken cancellationToken)
        => await reader.GetDeviceTransportersAsync(request.Filter.GetFilters(), cancellationToken);

}
