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

using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Security.Infrastructure.SecurityDB.Entities;
using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;

namespace TrackHub.Security.Infrastructure.SecurityDB.Readers;

public sealed class DriverIdentityReader(IApplicationDbContext context, ICurrentPrincipal principal) : IDriverIdentityReader
{
    private bool CanAccessAllAccounts =>
        principal.PrincipalType == PrincipalType.ServiceClient && !principal.AccountId.HasValue
        || string.Equals(principal.Role, Roles.Administrator, StringComparison.OrdinalIgnoreCase);

    private Guid RequireAccountAccess(Guid accountId)
    {
        if (CanAccessAllAccounts || principal.AccountId == accountId)
        {
            return accountId;
        }

        throw new ForbiddenAccessException();
    }

    private static int PageSize(int take) => Math.Clamp(take <= 0 ? 50 : take, 1, 500);
    private static int Offset(int skip) => Math.Max(0, skip);

    public async Task<IReadOnlyCollection<DriverCredentialVm>> GetDriverCredentialsAsync(Guid accountId, Guid? driverId, int skip, int take, CancellationToken cancellationToken)
        => await context.DriverCredentials
            .Where(x => x.AccountId == RequireAccountAccess(accountId) && (!driverId.HasValue || x.DriverId == driverId.Value))
            .OrderBy(x => x.NormalizedLogin).ThenBy(x => x.DriverCredentialId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => ToVm(x))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<DriverDeviceRegistrationVm>> GetDriverDevicesAsync(Guid accountId, Guid? driverId, int skip, int take, CancellationToken cancellationToken)
        => await context.DriverDeviceRegistrations
            .Where(x => x.AccountId == RequireAccountAccess(accountId) && (!driverId.HasValue || x.DriverId == driverId.Value))
            .OrderByDescending(x => x.LastSeenAt ?? x.RegisteredAt).ThenBy(x => x.DriverDeviceRegistrationId)
            .Skip(Offset(skip)).Take(PageSize(take))
            .Select(x => ToVm(x))
            .ToListAsync(cancellationToken);

    private static DriverCredentialVm ToVm(DriverCredential x) => new(x.DriverCredentialId, x.DriverId, x.AccountId, x.NormalizedLogin, x.FailedAttempts, x.LockedUntil, x.VerifiedAt, x.LastLoginAt, x.Active, x.ResetRequired, x.LastModified);
    private static DriverDeviceRegistrationVm ToVm(DriverDeviceRegistration x) => new(x.DriverDeviceRegistrationId, x.DriverId, x.AccountId, x.DeviceId, x.DeviceName, x.Platform, x.AppVersion, MaskPushToken(x.PushToken), x.Active, x.RegisteredAt, x.LastSeenAt, x.RevokedAt, x.RevokedBy, x.LastModified);

    // Masks the push token, exposing only a trailing fragment so admins can identify a device
    // without the value being usable. Runs client-side in the final projection.
    private static string? MaskPushToken(string? pushToken)
    {
        if (string.IsNullOrEmpty(pushToken))
        {
            return pushToken;
        }

        return pushToken.Length <= 6
            ? new string('*', pushToken.Length)
            : $"******{pushToken[^6..]}";
    }
}
