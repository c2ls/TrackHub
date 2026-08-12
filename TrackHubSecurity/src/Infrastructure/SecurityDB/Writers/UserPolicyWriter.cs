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

// The UserPolicyWriter class is a sealed class that implements the IUserPolicyWriter interface.
// The TARGET user's owning account is checked against the caller before a grant is written or
// removed — the enforcement point the [AccountScopeEnforcedInHandler] markers on the user-policy
// commands cite.
public sealed class UserPolicyWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IUserPolicyWriter
{
    // This method creates a new user policy asynchronously.
    public async Task<UserPolicyVm> CreateUserPolicyAsync(UserPolicyDto userPolicyDto, CancellationToken cancellationToken)
    {
        await RequireTargetUserAccessAsync(userPolicyDto.UserId, cancellationToken);

        // Create a new UserPolicy object with the provided user ID and policy ID.
        var userPolicy = new UserPolicy
        {
            UserId = userPolicyDto.UserId,
            PolicyId = userPolicyDto.PolicyId
        };

        // Add the user policy to the UserPolicies DbSet in the Context.
        await Context.UserPolicies.AddAsync(userPolicy, cancellationToken);

        // Save the changes to the database.
        await Context.SaveChangesAsync(cancellationToken);

        // Return a new UserPolicyVm object with the user ID and policy ID.
        return new UserPolicyVm(
            userPolicy.UserId,
            userPolicy.PolicyId);
    }

    // This method deletes a user policy asynchronously.
    public async Task DeleteUserPolicyAsync(Guid userId, int policyId, CancellationToken cancellationToken)
    {
        await RequireTargetUserAccessAsync(userId, cancellationToken);

        // Find the user policy with the provided user ID and policy ID in the UserPolicies DbSet.
        var userPolicy = await Context.UserPolicies.FindAsync([policyId, userId], cancellationToken)
            ?? throw new NotFoundException(nameof(UserPolicy), $"{userId},{policyId}");

        // Remove the user policy from the UserPolicies DbSet.
        Context.UserPolicies.Attach(userPolicy);
        Context.UserPolicies.Remove(userPolicy);

        // Save the changes to the database.
        await Context.SaveChangesAsync(cancellationToken);
    }

    private async Task RequireTargetUserAccessAsync(Guid userId, CancellationToken cancellationToken)
    {
        var target = await Context.Users.FindAsync([userId], cancellationToken)
            ?? throw new NotFoundException(nameof(User), $"{userId}");
        RequireAccountAccess(target.AccountId);
    }
}
