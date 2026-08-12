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

namespace TrackHub.AuthorityServer.Infrastructure.Writers;

public sealed class DriverCredentialWriter(SecurityDbContext context) : IDriverCredentialWriter
{
    public async Task RecordDriverCredentialLoginSuccessAsync(Guid driverCredentialId, CancellationToken cancellationToken)
    {
        var credential = await context.DriverCredentials.FirstAsync(x => x.DriverCredentialId == driverCredentialId, cancellationToken);
        credential.FailedAttempts = 0;
        credential.LockedUntil = null;
        credential.LastLoginAt = DateTimeOffset.UtcNow;
        credential.LastModified = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RecordDriverCredentialLoginFailureAsync(Guid driverCredentialId, int failedAttempts, DateTimeOffset? lockedUntil, CancellationToken cancellationToken)
    {
        var credential = await context.DriverCredentials.FirstAsync(x => x.DriverCredentialId == driverCredentialId, cancellationToken);
        credential.FailedAttempts = failedAttempts;
        credential.LockedUntil = lockedUntil;
        credential.LastModified = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}
