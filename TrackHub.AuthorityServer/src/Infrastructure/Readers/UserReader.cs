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
using TrackHub.AuthorityServer.Domain.Models;
using TrackHub.AuthorityServer.Domain.Records;

namespace TrackHub.AuthorityServer.Infrastructure.Readers;
public sealed class UserReader(SecurityDbContext context) : IUserReader
{
    public async Task<UserVm> GetUserAsync(UserLoginDto userLogin, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.EmailAddress.Equals(userLogin.EmailAddress))
            .Select(u => new UserVm(
                u.UserId,
                u.Username,
                u.Password,
                u.EmailAddress,
                u.Verified,
                u.Active,
                u.LoginAttempts,
                u.LockedUntil,
                u.AccountId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    // The user's most privileged role name for the access token's role claim (the seeded
    // hierarchy is Administrator(1) → Manager(2) → User(3), so the lowest id wins).
    public async Task<string?> GetUserRoleAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Join(context.Roles, ur => ur.RoleId, r => r.RoleId, (ur, r) => new { r.RoleId, r.Name })
            .OrderBy(x => x.RoleId)
            .Select(x => x.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // Loads a user by id for refresh-token subject re-validation (active/locked state).
    public async Task<UserVm> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Select(u => new UserVm(
                u.UserId,
                u.Username,
                u.Password,
                u.EmailAddress,
                u.Verified,
                u.Active,
                u.LoginAttempts,
                u.LockedUntil,
                u.AccountId))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
