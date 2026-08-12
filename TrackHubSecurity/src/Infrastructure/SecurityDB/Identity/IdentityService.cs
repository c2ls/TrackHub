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

namespace TrackHub.Security.Infrastructure.SecurityDB.Identity;

public class IdentityService(IUserReader userReader,
    IResourceActionRoleReader resourceActionRoleReader,
    IResourceActionPolicyReader resourceActionPolicyReader,
    IUserRoleReader userRoleReader,
    IUserPolicyReader userPolicyReader,
    IClientReader clientReader,
    IServiceClientPermissionReader serviceClientPermissionReader) : IIdentityService
{
    // Retrieves the username associated with the given userId asynchronously.
    public async Task<string> GetUserNameAsync(Guid userId, CancellationToken token)
        => await userReader.GetUserNameAsync(userId, token);

    // Checks if the user with the given userId is in the specified role for the given resource and action asynchronously.
    public async Task<bool> IsInRoleAsync(Guid userId, string resource, string action, CancellationToken token)
    {
        var resourceActionRoles = await resourceActionRoleReader.GetResourceActionRolesAsync(resource, action, token);
        var userRoles = await userRoleReader.GetUserRoleNamesAsync(userId, token);
        return resourceActionRoles.Any(role => userRoles.Contains(role));
    }

    // Authorizes the user with the given userId for the specified resource and action asynchronously.
    // Policies are additive restrictions: a resource-action with NO policy rows imposes no policy
    // requirement, so an empty set intentionally evaluates to true (roles remain the primary gate).
    public async Task<bool> AuthorizeAsync(Guid userId, string resource, string action, CancellationToken token)
    {
        var resourceActionPolicies = await resourceActionPolicyReader.GetResourceActionPoliciesAsync(resource, action, token);
        if (!resourceActionPolicies.Any())
        {
            return true;
        }
        var userPolicies = await userPolicyReader.GetUserPolicyNamesAsync(userId, token);
        return resourceActionPolicies.All(policy => userPolicies.Contains(policy));
    }

    // Combined role + policy decision in one in-process evaluation. This backs the
    // `authorizeUser` GraphQL query that every service's authorization pipeline calls,
    // replacing the former two-round-trip isInRole + authorize sequence.
    public async Task<bool> AuthorizeUserAsync(Guid userId, string resource, string action, CancellationToken token)
        => await IsInRoleAsync(userId, resource, action, token)
           && await AuthorizeAsync(userId, resource, action, token);

    // Checks if the given client is valid asynchronously.
    public async Task<bool> IsValidServiceAsync(string? client, CancellationToken token)
        => client != null && await clientReader.IsValidClientAsync(client, token);

    public async Task<bool> IsValidServiceAsync(string? client, string resource, string action, CancellationToken token)
        => client != null
           && await clientReader.IsValidClientAsync(client, token)
           && await serviceClientPermissionReader.HasPermissionAsync(client, resource, action, token);

    public async Task<bool> IsValidServiceAsync(string? client, string resource, string action, Guid? accountId, IReadOnlyCollection<string> scopes, IReadOnlyCollection<string> audiences, CancellationToken token)
        => client != null
           && await clientReader.IsValidClientAsync(client, token)
           && await serviceClientPermissionReader.HasPermissionAsync(client, resource, action, accountId, scopes, audiences, token);

}
