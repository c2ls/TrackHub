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

namespace TrackHub.Security.Application.DriverIdentity.Queries;

[Authorize(Resource = Resources.Drivers, Action = Actions.Read)]
public readonly record struct GetDriverCredentialsQuery(Guid AccountId, Guid? DriverId = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<DriverCredentialVm>>;
public class GetDriverCredentialsQueryHandler(IDriverIdentityReader reader) : IRequestHandler<GetDriverCredentialsQuery, IReadOnlyCollection<DriverCredentialVm>>
{
    public async Task<IReadOnlyCollection<DriverCredentialVm>> Handle(GetDriverCredentialsQuery request, CancellationToken cancellationToken)
        => await reader.GetDriverCredentialsAsync(request.AccountId, request.DriverId, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.Drivers, Action = Actions.Read)]
public readonly record struct GetDriverDevicesQuery(Guid AccountId, Guid? DriverId = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<DriverDeviceRegistrationVm>>;
public class GetDriverDevicesQueryHandler(IDriverIdentityReader reader) : IRequestHandler<GetDriverDevicesQuery, IReadOnlyCollection<DriverDeviceRegistrationVm>>
{
    public async Task<IReadOnlyCollection<DriverDeviceRegistrationVm>> Handle(GetDriverDevicesQuery request, CancellationToken cancellationToken)
        => await reader.GetDriverDevicesAsync(request.AccountId, request.DriverId, request.Skip, request.Take, cancellationToken);
}
