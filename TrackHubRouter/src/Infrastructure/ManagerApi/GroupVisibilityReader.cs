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

namespace TrackHub.Router.Infrastructure.ManagerApi;

// Validates that the requesting user's groups include the target transporter.
// Runs under the user's token; Manager scopes the check to the user's account.
public class GroupVisibilityReader(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IGroupVisibilityReader
{
    internal const string ValidateGroupVisibilityQuery = @"
                query($accountId: UUID!, $userId: UUID!, $resourceType: String!, $resourceId: String!) {
                    validateGroupVisibility(query: { accountId: $accountId, userId: $userId, resourceType: $resourceType, resourceId: $resourceId })
                }";

    public async Task<bool> ValidateGroupVisibilityAsync(Guid accountId, Guid userId, string resourceType, string resourceId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = ValidateGroupVisibilityQuery,
            Variables = new { accountId, userId, resourceType, resourceId }
        };
        return await QueryAsync<bool>(request, cancellationToken);
    }
}
