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

namespace TrackHub.Security.Application.StaffIdentity.Queries;

[Authorize(Resource = Resources.Staff, Action = Actions.Read)]
// Enforcement: the reader filters on AccountId; StaffId only narrows within it.
[AccountScopeEnforcedInHandler]
public readonly record struct GetStaffCredentialsQuery(Guid AccountId, Guid? StaffId = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<StaffCredentialVm>>;
public class GetStaffCredentialsQueryHandler(IStaffIdentityReader reader) : IRequestHandler<GetStaffCredentialsQuery, IReadOnlyCollection<StaffCredentialVm>>
{
    public async Task<IReadOnlyCollection<StaffCredentialVm>> Handle(GetStaffCredentialsQuery request, CancellationToken cancellationToken)
        => await reader.GetStaffCredentialsAsync(request.AccountId, request.StaffId, request.Skip, request.Take, cancellationToken);
}

[Authorize(Resource = Resources.StaffOperations, Action = Actions.Read)]
// Enforcement: the reader filters on AccountId; StaffId only narrows within it.
[AccountScopeEnforcedInHandler]
public readonly record struct GetStaffDevicesQuery(Guid AccountId, Guid? StaffId = null, int Skip = 0, int Take = 50) : IRequest<IReadOnlyCollection<StaffDeviceRegistrationVm>>;
public class GetStaffDevicesQueryHandler(IStaffIdentityReader reader) : IRequestHandler<GetStaffDevicesQuery, IReadOnlyCollection<StaffDeviceRegistrationVm>>
{
    public async Task<IReadOnlyCollection<StaffDeviceRegistrationVm>> Handle(GetStaffDevicesQuery request, CancellationToken cancellationToken)
        => await reader.GetStaffDevicesAsync(request.AccountId, request.StaffId, request.Skip, request.Take, cancellationToken);
}
