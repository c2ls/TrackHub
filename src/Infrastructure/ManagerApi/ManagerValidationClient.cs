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

namespace TrackHub.TripManagement.Infrastructure.ManagerApi;

/// <summary>
/// Cross-account reference validation against Manager, under the service's own
/// <c>trip_client</c> identity: driver-to-transporter assignment, group visibility over a
/// referenced resource, and the account's feature keys (spec 11 §5).
/// Manager's <c>resourceId</c> arguments are strings on the wire.
/// </summary>
public class ManagerValidationClient(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager, asService: true)), IManagerValidationClient
{
    internal const string ValidateDriverAssignmentQuery = @"
                query($driverId: UUID!, $resourceType: String!, $resourceId: String!) {
                    validateDriverAssignment(query: { driverId: $driverId, resourceType: $resourceType, resourceId: $resourceId })
                }";

    internal const string ValidateGroupVisibilityQuery = @"
                query($accountId: UUID!, $userId: UUID!, $resourceType: String!, $resourceId: String!) {
                    validateGroupVisibility(query: { accountId: $accountId, userId: $userId, resourceType: $resourceType, resourceId: $resourceId })
                }";

    internal const string ValidateFeatureEnabledQuery = @"
                query($accountId: UUID!, $featureKey: String!) {
                    validateFeatureEnabled(query: { accountId: $accountId, featureKey: $featureKey })
                }";

    public async Task<bool> ValidateDriverAssignmentAsync(Guid driverId, string resourceType, Guid resourceId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = ValidateDriverAssignmentQuery,
            Variables = new { driverId, resourceType, resourceId = resourceId.ToString() }
        };
        return await QueryAsync<bool>(request, cancellationToken);
    }

    public async Task<bool> ValidateGroupVisibilityAsync(Guid accountId, Guid userId, string resourceType, Guid resourceId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = ValidateGroupVisibilityQuery,
            Variables = new { accountId, userId, resourceType, resourceId = resourceId.ToString() }
        };
        return await QueryAsync<bool>(request, cancellationToken);
    }

    public async Task<bool> ValidateFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = ValidateFeatureEnabledQuery,
            Variables = new { accountId, featureKey }
        };
        return await QueryAsync<bool>(request, cancellationToken);
    }
}
