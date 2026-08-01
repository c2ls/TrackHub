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

namespace TrackHub.Security.Infrastructure.Identity;

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

    // Policies are additive GRANTS, not restrictions: true when the user holds AT LEAST ONE policy
    // mapped to this resource/action. A resource-action with no policy rows grants nothing extra and
    // returns false, so the role stays the only path unless a policy is deliberately attached.
    //
    // This is the per-user elevation lever: the role matrix stays a small set of coherent bundles,
    // and a single user is raised above their role (e.g. one dispatcher who may delete trips) by
    // attaching a policy instead of widening the role for everyone who holds it. It composes the
    // other way too — a policy can grant an action no role has.
    //
    // Restriction semantics ("holding the policy is REQUIRED to reach the resource") were the prior
    // reading, and they inverted the operator's intent: attaching a policy to a resource/action
    // silently revoked it from every caller who did not hold that policy — Administrator's grant-all
    // included, since nothing here bypasses the check. Nothing depended on that behaviour
    // (resource_action_policy and user_policy were both empty), and GetAuthorizedActionsQuery already
    // reported permissions as role UNION policy — grant semantics — so enforcement now matches the
    // answer the platform was already giving.
    public async Task<bool> AuthorizeAsync(Guid userId, string resource, string action, CancellationToken token)
    {
        var resourceActionPolicies = await resourceActionPolicyReader.GetResourceActionPoliciesAsync(resource, action, token);
        if (resourceActionPolicies.Count == 0)
        {
            return false;
        }
        var userPolicies = await userPolicyReader.GetUserPolicyNamesAsync(userId, token);
        return resourceActionPolicies.Any(userPolicies.Contains);
    }

    // Combined role + policy decision in one in-process evaluation. This backs the
    // `authorizeUser` GraphQL query that every service's authorization pipeline calls,
    // replacing the former two-round-trip isInRole + authorize sequence.
    // Role OR policy — either path alone is sufficient.
    public async Task<bool> AuthorizeUserAsync(Guid userId, string resource, string action, CancellationToken token)
        => await IsInRoleAsync(userId, resource, action, token)
           || await AuthorizeAsync(userId, resource, action, token);

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
