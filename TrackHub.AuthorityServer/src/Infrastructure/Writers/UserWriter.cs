// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
using TrackHub.AuthorityServer.Infrastructure.Entities;

namespace TrackHub.AuthorityServer.Infrastructure.Writers;

public sealed class UserWriter(SecurityDbContext context) : IUserWriter
{

    // Records a failed login: sets the rolling attempt counter and an optional timed lock.
    public async Task RecordLoginFailureAsync(Guid userId, int loginAttempts, DateTimeOffset? lockedUntil, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"{userId}");

        user.LoginAttempts = loginAttempts;
        user.LockedUntil = lockedUntil;

        await context.SaveChangesAsync(cancellationToken);
    }

    // Records a successful login: resets the rolling attempt counter and clears any lock.
    public async Task RecordLoginSuccessAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"{userId}");

        user.LoginAttempts = 0;
        user.LockedUntil = null;

        await context.SaveChangesAsync(cancellationToken);
    }

}
