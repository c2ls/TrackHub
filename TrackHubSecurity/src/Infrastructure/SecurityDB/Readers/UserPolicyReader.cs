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

// This class represents a reader for user policies in the security database.
public sealed class UserPolicyReader(IApplicationDbContext context) : IUserPolicyReader
{

    // Retrieves the names of the policies associated with a user.
    public async Task<IReadOnlyCollection<string>> GetUserPolicyNamesAsync(Guid userId, CancellationToken cancellationToken)
        => await context.UserPolicies
            .Include(up => up.Policy)
            .Where(up => up.UserId.Equals(userId))
            .Select(up => up.Policy.Name)
            .ToListAsync(cancellationToken);

    // Retrieves the IDs of the policies associated with a user.
    public async Task<IReadOnlyCollection<int>> GetUserPolicyIdsAsync(Guid userId, CancellationToken cancellationToken)
        => await context.UserPolicies
            .Where(up => up.UserId.Equals(userId))
            .Select(up => up.PolicyId)
            .ToListAsync(cancellationToken);

}
