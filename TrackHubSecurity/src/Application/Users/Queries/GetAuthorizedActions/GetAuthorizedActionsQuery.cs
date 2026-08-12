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

namespace TrackHub.Security.Application.Users.Queries.GetAuthorizedActions;

[Authorize(Resource = Resources.Accounts, Action = Actions.Read)]
public readonly record struct GetAuthorizedActionsQuery(Guid UserId) : IRequest<IReadOnlyCollection<ResourceActionVm>>;

public class GetAuthorizedActionsQueryHandler(
    IUserRoleReader userRoleReader,
    IUserPolicyReader userPolicyReader,
    IResourceActionRoleReader resourceActionRoleReader,
    IResourceActionPolicyReader resourceActionPolicyReader) : IRequestHandler<GetAuthorizedActionsQuery, IReadOnlyCollection<ResourceActionVm>>
{
    /// <summary>
    /// Handle method to process the GetAuthorizedActionsQuery request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>a list of ResourceActionVm objects</returns>
    public async Task<IReadOnlyCollection<ResourceActionVm>> Handle(GetAuthorizedActionsQuery request, CancellationToken cancellationToken)
    {
        // Get the user roles for the specified user ID
        var userRoles = await userRoleReader.GetUserRolesIdsAsync(request.UserId, cancellationToken);

        // Get the user policies for the specified user ID
        var userPolicies = await userPolicyReader.GetUserPolicyIdsAsync(request.UserId, cancellationToken);

        // Get the role authorized actions based on the user roles
        var roleAuthorizedActions = await resourceActionRoleReader.GetRoleAuthorizedActionsAsync(userRoles, cancellationToken);

        // Get the policy authorized actions based on the user policies
        var policyAuthorizedActions = await resourceActionPolicyReader.GetPolicyAuthorizedActionsAsync(userPolicies, cancellationToken);

        // Combine the role authorized actions and policy authorized actions, remove duplicates, and convert to a list
        var allAuthorizedActions = roleAuthorizedActions.Union(policyAuthorizedActions).Distinct().ToList();

        // Return the list of all authorized actions
        return allAuthorizedActions;
    }

}
