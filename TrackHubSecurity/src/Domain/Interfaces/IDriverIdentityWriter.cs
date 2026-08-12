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

public interface IDriverIdentityWriter
{
    Task<DriverCredentialVm> CreateDriverCredentialAsync(DriverCredentialDto credential, CancellationToken cancellationToken);
    Task ActivateDriverCredentialAsync(Guid driverCredentialId, string password, CancellationToken cancellationToken);
    Task LockDriverCredentialAsync(Guid driverCredentialId, DateTimeOffset lockedUntil, CancellationToken cancellationToken);
    Task ResetDriverCredentialAsync(Guid driverCredentialId, string password, bool resetRequired, CancellationToken cancellationToken);
    Task RevokeDriverCredentialAsync(Guid driverCredentialId, CancellationToken cancellationToken);
    Task<DriverDeviceRegistrationVm> RegisterDriverDeviceAsync(DriverDeviceRegistrationDto device, CancellationToken cancellationToken);
    Task UpdateDriverDevicePushTokenAsync(Guid driverDeviceRegistrationId, string? pushToken, string? appVersion, CancellationToken cancellationToken);
    Task RevokeDriverDeviceAsync(Guid driverDeviceRegistrationId, string revokedBy, CancellationToken cancellationToken);
}
