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


using Common.Application.Interfaces;
using TrackHub.Security.Domain.Records;
using TrackHub.Security.Infrastructure.Interfaces;

namespace TrackHub.Security.Infrastructure.Writers;

// This class represents a writer for the UserRole entity in the security database. The TARGET
// user's owning account is checked against the caller before a grant is written or removed — the
// enforcement point the [AccountScopeEnforcedInHandler] markers on the user-role commands cite.
public sealed class UserRoleWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IUserRoleWriter
{
    // Creates a new UserRole asynchronously and returns the created UserRoleVm.
    public async Task<UserRoleVm> CreateUserRoleAsync(UserRoleDto userRoleDto, CancellationToken cancellationToken)
    {
        await RequireTargetUserAccessAsync(userRoleDto.UserId, cancellationToken);

        var userRole = new UserRole
        {
            UserId = userRoleDto.UserId,
            RoleId = userRoleDto.RoleId
        };

        await Context.UserRoles.AddAsync(userRole, cancellationToken);
        await Context.SaveChangesAsync(cancellationToken);

        return new UserRoleVm(
            userRole.UserId,
            userRole.RoleId);
    }

    // Deletes a UserRole asynchronously based on the provided userId and roleId.
    public async Task DeleteUserRoleAsync(Guid userId, int roleId, CancellationToken cancellationToken)
    {
        await RequireTargetUserAccessAsync(userId, cancellationToken);

        var userRole = await Context.UserRoles.FindAsync([roleId, userId], cancellationToken)
            ?? throw new NotFoundException(nameof(UserRole), $"{userId},{roleId}");

        Context.UserRoles.Attach(userRole);
        Context.UserRoles.Remove(userRole);
        await Context.SaveChangesAsync(cancellationToken);
    }

    private async Task RequireTargetUserAccessAsync(Guid userId, CancellationToken cancellationToken)
    {
        var target = await Context.Users.FindAsync([userId], cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"{userId}");
        RequireAccountAccess(target.AccountId);
    }
}
