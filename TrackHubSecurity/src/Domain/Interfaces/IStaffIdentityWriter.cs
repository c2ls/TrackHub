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

namespace TrackHub.Security.Domain.Interfaces;

public interface IStaffIdentityWriter
{
    Task<StaffCredentialVm> CreateStaffCredentialAsync(StaffCredentialDto credential, CancellationToken cancellationToken);
    Task ActivateStaffCredentialAsync(Guid staffCredentialId, string password, CancellationToken cancellationToken);
    Task LockStaffCredentialAsync(Guid staffCredentialId, DateTimeOffset lockedUntil, CancellationToken cancellationToken);
    Task ResetStaffCredentialAsync(Guid staffCredentialId, string password, bool resetRequired, CancellationToken cancellationToken);
    Task RevokeStaffCredentialAsync(Guid staffCredentialId, CancellationToken cancellationToken);
    Task RevokeStaffCredentialsAsync(Guid staffId, Guid accountId, CancellationToken cancellationToken);
    Task<StaffDeviceRegistrationVm> RegisterStaffDeviceAsync(StaffDeviceRegistrationDto device, CancellationToken cancellationToken);
    Task UpdateStaffDevicePushTokenAsync(Guid staffDeviceRegistrationId, string? pushToken, string? appVersion, CancellationToken cancellationToken);
    Task RevokeStaffDeviceAsync(Guid staffDeviceRegistrationId, string revokedBy, CancellationToken cancellationToken);
}
