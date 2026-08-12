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

namespace TrackHub.Geofencing.Infrastructure.Readers;

// This class represents a reader for retrieving user data from the database.
public sealed class UserReader(IApplicationDbContext context) : IUserReader
{
    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A UserVm object representing the retrieved user.</returns>
    public async Task<UserVm> GetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        // The view hides inactive users; a missing row must read as "user not visible",
        // not as an opaque InvalidOperationException from FirstAsync.
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.UserId.Equals(id), cancellationToken)
            ?? throw new NotFoundException($"{id}", nameof(VwUser));

        return new UserVm(user.UserId, user.Username, user.AccountId);
    }
}

