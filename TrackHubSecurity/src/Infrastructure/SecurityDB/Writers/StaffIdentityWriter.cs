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

public sealed class StaffIdentityWriter(IApplicationDbContext context, ICurrentPrincipal principal, IManagerStaffMemberReader staff)
    : AccountScopedDataAccess(context, principal), IStaffIdentityWriter
{
    public async Task<StaffCredentialVm> CreateStaffCredentialAsync(StaffCredentialDto credential, CancellationToken cancellationToken)
    {
        var accountId = await RequireAccountAccessAsync(credential.AccountId, cancellationToken);
        await RequireStaffOfAccountAsync(credential.StaffId, accountId, cancellationToken);
        var entity = new StaffCredential(credential.StaffId, accountId, NormalizeLogin(credential.Login), credential.Password.HashPassword(), credential.Active)
        {
            ResetRequired = credential.ResetRequired
        };
        await Context.StaffCredentials.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task ActivateStaffCredentialAsync(Guid staffCredentialId, string password, CancellationToken cancellationToken)
    {
        var entity = await GetCredentialForWriteAsync(staffCredentialId, cancellationToken);
        entity.PasswordHash = password.HashPassword();
        entity.Active = true;
        entity.ResetRequired = false;
        entity.FailedAttempts = 0;
        entity.LockedUntil = null;
        entity.VerifiedAt = DateTimeOffset.UtcNow;
        entity.SecurityStamp = Guid.NewGuid();
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task LockStaffCredentialAsync(Guid staffCredentialId, DateTimeOffset lockedUntil, CancellationToken cancellationToken)
    {
        var entity = await GetCredentialForWriteAsync(staffCredentialId, cancellationToken);
        entity.LockedUntil = lockedUntil;
        entity.SecurityStamp = Guid.NewGuid();
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetStaffCredentialAsync(Guid staffCredentialId, string password, bool resetRequired, CancellationToken cancellationToken)
    {
        var entity = await GetCredentialForWriteAsync(staffCredentialId, cancellationToken);
        entity.PasswordHash = password.HashPassword();
        entity.ResetRequired = resetRequired;
        entity.FailedAttempts = 0;
        entity.LockedUntil = null;
        entity.SecurityStamp = Guid.NewGuid();
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeStaffCredentialAsync(Guid staffCredentialId, CancellationToken cancellationToken)
    {
        var entity = await GetCredentialForWriteAsync(staffCredentialId, cancellationToken);
        Revoke(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeStaffCredentialsAsync(Guid staffId, Guid accountId, CancellationToken cancellationToken)
    {
        var credentials = await Context.StaffCredentials
            .AsTracking()
            .Where(x => x.StaffId == staffId && x.AccountId == accountId && x.Active)
            .ToListAsync(cancellationToken);
        credentials.ForEach(Revoke);
        await Context.SaveChangesAsync(cancellationToken);
    }

    private static void Revoke(StaffCredential entity)
    {
        entity.Active = false;
        entity.SecurityStamp = Guid.NewGuid();
    }

    public async Task<StaffDeviceRegistrationVm> RegisterStaffDeviceAsync(StaffDeviceRegistrationDto device, CancellationToken cancellationToken)
    {
        var accountId = await RequireAccountAccessAsync(device.AccountId, cancellationToken);
        await RequireStaffOfAccountAsync(device.StaffId, accountId, cancellationToken);
        var entity = new StaffDeviceRegistration(device.StaffId, accountId, device.DeviceId, device.DeviceName, device.Platform, device.AppVersion, device.PushToken, device.RefreshTokenFamilyId);
        await Context.StaffDeviceRegistrations.AddAsync(entity, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);
        return ToVm(entity);
    }

    public async Task UpdateStaffDevicePushTokenAsync(Guid staffDeviceRegistrationId, string? pushToken, string? appVersion, CancellationToken cancellationToken)
    {
        var entity = await GetDeviceForWriteAsync(staffDeviceRegistrationId, cancellationToken);
        entity.PushToken = pushToken;
        entity.AppVersion = appVersion;
        entity.LastSeenAt = DateTimeOffset.UtcNow;
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeStaffDeviceAsync(Guid staffDeviceRegistrationId, string revokedBy, CancellationToken cancellationToken)
    {
        var entity = await GetDeviceForWriteAsync(staffDeviceRegistrationId, cancellationToken);
        entity.Active = false;
        entity.RevokedAt = DateTimeOffset.UtcNow;
        entity.RevokedBy = revokedBy;
        await Context.SaveChangesAsync(cancellationToken);
    }

    private async Task<StaffCredential> GetCredentialForWriteAsync(Guid staffCredentialId, CancellationToken cancellationToken)
    {
        var entity = await Context.StaffCredentials
            .AsTracking().FirstOrDefaultAsync(x => x.StaffCredentialId == staffCredentialId, cancellationToken);
        if (entity is null || !await HasAccountAccessAsync(entity.AccountId, cancellationToken))
        {
            throw new NotFoundException(nameof(StaffCredential), staffCredentialId.ToString());
        }

        return entity;
    }

    private async Task<StaffDeviceRegistration> GetDeviceForWriteAsync(Guid staffDeviceRegistrationId, CancellationToken cancellationToken)
    {
        var entity = await Context.StaffDeviceRegistrations
            .AsTracking().FirstOrDefaultAsync(x => x.StaffDeviceRegistrationId == staffDeviceRegistrationId, cancellationToken);
        if (entity is null || !await HasAccountAccessAsync(entity.AccountId, cancellationToken))
        {
            throw new NotFoundException(nameof(StaffDeviceRegistration), staffDeviceRegistrationId.ToString());
        }

        return entity;
    }

    private async Task RequireStaffOfAccountAsync(Guid staffId, Guid accountId, CancellationToken cancellationToken)
    {
        if (!await staff.StaffBelongsToAccountAsync(staffId, accountId, cancellationToken))
        {
            throw new NotFoundException(nameof(StaffCredential.StaffId), staffId.ToString());
        }
    }

    private static string NormalizeLogin(string login) => login.Trim().ToUpperInvariant();
    private static StaffCredentialVm ToVm(StaffCredential x) => new(x.StaffCredentialId, x.StaffId, x.AccountId, x.NormalizedLogin, x.FailedAttempts, x.LockedUntil, x.VerifiedAt, x.LastLoginAt, x.Active, x.ResetRequired, x.LastModified);
    private static StaffDeviceRegistrationVm ToVm(StaffDeviceRegistration x) => new(x.StaffDeviceRegistrationId, x.StaffId, x.AccountId, x.DeviceId, x.DeviceName, x.Platform, x.AppVersion, MaskPushToken(x.PushToken), x.Active, x.RegisteredAt, x.LastSeenAt, x.RevokedAt, x.RevokedBy, x.LastModified);

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
