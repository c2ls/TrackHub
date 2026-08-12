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


using TrackHub.Security.Domain.Records;
using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;

namespace TrackHub.Security.Infrastructure.SecurityDB.Writers;

// This class represents a writer for the UserRole entity in the security database.
public sealed class UserRoleWriter(IApplicationDbContext context) : IUserRoleWriter
{
    // Creates a new UserRole asynchronously and returns the created UserRoleVm.
    public async Task<UserRoleVm> CreateUserRoleAsync(UserRoleDto userRoleDto, CancellationToken cancellationToken)
    {
        var userRole = new UserRole
        {
            UserId = userRoleDto.UserId,
            RoleId = userRoleDto.RoleId
        };

        await context.UserRoles.AddAsync(userRole, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new UserRoleVm(
            userRole.UserId,
            userRole.RoleId);
    }

    // Deletes a UserRole asynchronously based on the provided userId and roleId.
    public async Task DeleteUserRoleAsync(Guid userId, int roleId, CancellationToken cancellationToken)
    {
        var userRole = await context.UserRoles.FindAsync([roleId, userId], cancellationToken)
            ?? throw new NotFoundException(nameof(UserRole), $"{userId},{roleId}");

        context.UserRoles.Attach(userRole);
        context.UserRoles.Remove(userRole);
        await context.SaveChangesAsync(cancellationToken);
    }
}
