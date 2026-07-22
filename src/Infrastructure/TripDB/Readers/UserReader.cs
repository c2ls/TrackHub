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

namespace TrackHub.TripManagement.Infrastructure.TripDB.Readers;

/// <summary>Resolves the acting user account from the local trip.vw_users view.</summary>
public sealed class UserReader(IApplicationDbContext context) : IUserReader
{
    public async Task<UserVm> GetUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken)
            ?? throw new NotFoundException($"{userId}", nameof(VwUser));

        return new UserVm(user.UserId, user.AccountId, user.Username);
    }
}
