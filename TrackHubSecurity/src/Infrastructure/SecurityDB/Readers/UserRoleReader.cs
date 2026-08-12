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

using TrackHub.Security.Infrastructure.SecurityDB.Interfaces;

namespace TrackHub.Security.Infrastructure.SecurityDB.Readers;

// This class implements the IUserRoleReader interface and provides methods to read user roles from the security database.
public sealed class UserRoleReader(IApplicationDbContext context) : IUserRoleReader
{
    // Retrieves the role names associated with a given user ID asynchronously.
    // Parameters:
    // - userId: The ID of the user.
    // - cancellationToken: A cancellation token to cancel the operation if needed.
    // Returns:
    // - A collection of role names associated with the user.
    public async Task<IReadOnlyCollection<string>> GetUserRoleNamesAsync(Guid userId, CancellationToken cancellationToken)
        => await context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId.Equals(userId))
            .Select(ur => ur.Role.Name)
            .ToListAsync(cancellationToken);

    // Retrieves the role IDs associated with a given user ID asynchronously.
    // Parameters:
    // - userId: The ID of the user.
    // - cancellationToken: A cancellation token to cancel the operation if needed.
    // Returns:
    // - A collection of role IDs associated with the user.
    public async Task<IReadOnlyCollection<int>> GetUserRolesIdsAsync(Guid userId, CancellationToken cancellationToken) 
        => await context.UserRoles
            .Where(ur => ur.UserId.Equals(userId))
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

}
