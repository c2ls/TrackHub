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

using TrackHub.AuthorityServer.Domain.Interfaces;
using TrackHub.AuthorityServer.Domain.Models;

namespace TrackHub.AuthorityServer.Infrastructure.Readers;

public sealed class DriverCredentialReader(SecurityDbContext context) : IDriverCredentialReader
{
    public async Task<DriverCredentialAuthenticationVm?> GetDriverCredentialByLoginAsync(string normalizedLogin, CancellationToken cancellationToken)
        => await context.DriverCredentials
            .AsNoTracking()
            .Where(x => x.NormalizedLogin == normalizedLogin && x.Active)
            .Select(x => new DriverCredentialAuthenticationVm(
                x.DriverCredentialId,
                x.DriverId,
                x.AccountId,
                x.PasswordHash,
                x.FailedAttempts,
                x.LockedUntil,
                x.VerifiedAt,
                x.Active,
                x.ResetRequired))
            .SingleOrDefaultAsync(cancellationToken);

    // Used for refresh-token subject re-validation: the driver must still hold an active credential.
    public async Task<bool> HasActiveCredentialAsync(Guid driverId, CancellationToken cancellationToken)
        => await context.DriverCredentials
            .AsNoTracking()
            .AnyAsync(x => x.DriverId == driverId && x.Active, cancellationToken);
}
