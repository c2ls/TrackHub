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

namespace TrackHub.Manager.Application.DeviceTransporter.Queries.GetById;

[Authorize(Resource = Resources.Devices, Action = Actions.Read)]
[AllowCrossAccount("Router resolves device→transporter pairs during position routing; from the SyncWorker host that call runs under the global router identity with no account claim. Tenant callers are NOT unguarded by this: DeviceTransporterReader.GetDeviceTransporterAsync resolves the transporter's owning account and RequireAccountAccess checks it before returning device identifiers.")]
public readonly record struct GetDeviceTransporterByIdQuery(Guid TransporterId) : IRequest<DeviceTransporterVm>;

public class GetDeviceByUserByOperatorQueryHandler(IDeviceTransporterReader reader) : IRequestHandler<GetDeviceTransporterByIdQuery, DeviceTransporterVm>
{
    public async Task<DeviceTransporterVm> Handle(GetDeviceTransporterByIdQuery request, CancellationToken cancellationToken)
        => await reader.GetDeviceTransporterAsync(request.TransporterId, cancellationToken);

}
