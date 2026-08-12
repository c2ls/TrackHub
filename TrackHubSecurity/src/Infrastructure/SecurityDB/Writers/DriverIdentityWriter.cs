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

using Common.Application.Interfaces;
using Common.Domain.Extensions;
using TrackHub.Security.Domain.Records;
using TrackHub.Security.Infrastructure.Entities;
using TrackHub.Security.Infrastructure.Interfaces;

namespace TrackHub.Security.Infrastructure.Writers;

public sealed class DriverIdentityWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IDriverIdentityWriter
{
    public async Task<DriverCredentialVm> CreateDriverCredentialAsync(DriverCredentialDto credential, CancellationToken cancellationToken)
    {
        var entity = new DriverCredential(credential.DriverId, RequireAccountAccess(credential.AccountId), NormalizeLogin(credential.Login), credential.Password.HashPassword(), credential.Active)
        {
            ResetRequired = credential.ResetRequired
        };
        await Context.DriverCredentials.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task ActivateDriverCredentialAsync(Guid driverCredentialId, string password, CancellationToken cancellationToken)
    {
        var entity = await GetCredentialForWriteAsync(driverCredentialId, cancellationToken);
        entity.PasswordHash = password.HashPassword();
        entity.Active = true;
        entity.ResetRequired = false;
        entity.FailedAttempts = 0;
        entity.LockedUntil = null;
        entity.VerifiedAt = DateTimeOffset.UtcNow;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task LockDriverCredentialAsync(Guid driverCredentialId, DateTimeOffset lockedUntil, CancellationToken cancellationToken)
    {
        var entity = await GetCredentialForWriteAsync(driverCredentialId, cancellationToken);
        entity.LockedUntil = lockedUntil;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetDriverCredentialAsync(Guid driverCredentialId, string password, bool resetRequired, CancellationToken cancellationToken)
    {
        var entity = await GetCredentialForWriteAsync(driverCredentialId, cancellationToken);
        entity.PasswordHash = password.HashPassword();
        entity.ResetRequired = resetRequired;
        entity.FailedAttempts = 0;
        entity.LockedUntil = null;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeDriverCredentialAsync(Guid driverCredentialId, CancellationToken cancellationToken)
    {
        var entity = await GetCredentialForWriteAsync(driverCredentialId, cancellationToken);
        entity.Active = false;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<DriverDeviceRegistrationVm> RegisterDriverDeviceAsync(DriverDeviceRegistrationDto device, CancellationToken cancellationToken)
    {
        var entity = new DriverDeviceRegistration(device.DriverId, RequireAccountAccess(device.AccountId), device.DeviceId, device.DeviceName, device.Platform, device.AppVersion, device.PushToken, device.RefreshTokenFamilyId);
        await Context.DriverDeviceRegistrations.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task UpdateDriverDevicePushTokenAsync(Guid driverDeviceRegistrationId, string? pushToken, string? appVersion, CancellationToken cancellationToken)
    {
        var entity = await GetDeviceForWriteAsync(driverDeviceRegistrationId, cancellationToken);
        entity.PushToken = pushToken;
        entity.AppVersion = appVersion;
        entity.LastSeenAt = DateTimeOffset.UtcNow;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeDriverDeviceAsync(Guid driverDeviceRegistrationId, string revokedBy, CancellationToken cancellationToken)
    {
        var entity = await GetDeviceForWriteAsync(driverDeviceRegistrationId, cancellationToken);
        entity.Active = false;
        entity.RevokedAt = DateTimeOffset.UtcNow;
        entity.RevokedBy = revokedBy;
        await Context.SaveChangesAsync(cancellationToken);
    }

    private async Task<DriverCredential> GetCredentialForWriteAsync(Guid driverCredentialId, CancellationToken cancellationToken)
    {
        var entity = await Context.DriverCredentials.FirstAsync(x => x.DriverCredentialId == driverCredentialId, cancellationToken);
        RequireAccountAccess(entity.AccountId);
        Context.DriverCredentials.Attach(entity);
        return entity;
    }

    private async Task<DriverDeviceRegistration> GetDeviceForWriteAsync(Guid driverDeviceRegistrationId, CancellationToken cancellationToken)
    {
        var entity = await Context.DriverDeviceRegistrations.FirstAsync(x => x.DriverDeviceRegistrationId == driverDeviceRegistrationId, cancellationToken);
        RequireAccountAccess(entity.AccountId);
        Context.DriverDeviceRegistrations.Attach(entity);
        return entity;
    }

    private static string NormalizeLogin(string login) => login.Trim().ToUpperInvariant();
    private static DriverCredentialVm ToVm(DriverCredential x) => new(x.DriverCredentialId, x.DriverId, x.AccountId, x.NormalizedLogin, x.FailedAttempts, x.LockedUntil, x.VerifiedAt, x.LastLoginAt, x.Active, x.ResetRequired, x.LastModified);
    private static DriverDeviceRegistrationVm ToVm(DriverDeviceRegistration x) => new(x.DriverDeviceRegistrationId, x.DriverId, x.AccountId, x.DeviceId, x.DeviceName, x.Platform, x.AppVersion, MaskPushToken(x.PushToken), x.Active, x.RegisteredAt, x.LastSeenAt, x.RevokedAt, x.RevokedBy, x.LastModified);

    // Keep read models free of usable token material: only a trailing fragment is exposed.
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
