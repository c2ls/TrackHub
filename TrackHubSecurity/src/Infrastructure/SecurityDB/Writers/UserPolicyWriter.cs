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

// The UserPolicyWriter class is a sealed class that implements the IUserPolicyWriter interface.
public sealed class UserPolicyWriter(IApplicationDbContext context) : IUserPolicyWriter
{
    // This method creates a new user policy asynchronously.
    public async Task<UserPolicyVm> CreateUserPolicyAsync(UserPolicyDto userPolicyDto, CancellationToken cancellationToken)
    {
        // Create a new UserPolicy object with the provided user ID and policy ID.
        var userPolicy = new UserPolicy
        {
            UserId = userPolicyDto.UserId,
            PolicyId = userPolicyDto.PolicyId
        };

        // Add the user policy to the UserPolicies DbSet in the context.
        await context.UserPolicies.AddAsync(userPolicy, cancellationToken);

        // Save the changes to the database.
        await context.SaveChangesAsync(cancellationToken);

        // Return a new UserPolicyVm object with the user ID and policy ID.
        return new UserPolicyVm(
            userPolicy.UserId,
            userPolicy.PolicyId);
    }

    // This method deletes a user policy asynchronously.
    public async Task DeleteUserPolicyAsync(Guid userId, int policyId, CancellationToken cancellationToken)
    {
        // Find the user policy with the provided user ID and policy ID in the UserPolicies DbSet.
        var userPolicy = await context.UserPolicies.FindAsync([policyId, userId], cancellationToken)
            ?? throw new NotFoundException(nameof(UserPolicy), $"{userId},{policyId}");

        // Remove the user policy from the UserPolicies DbSet.
        context.UserPolicies.Attach(userPolicy);
        context.UserPolicies.Remove(userPolicy);

        // Save the changes to the database.
        await context.SaveChangesAsync(cancellationToken);
    }
}
