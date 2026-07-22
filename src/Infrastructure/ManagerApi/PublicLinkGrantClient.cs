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
/// Public-link lifecycle delegated to Manager under <c>trip_client</c> — this module never hashes
/// a token, counts an access or writes the <c>PublicLinkAccessed</c> audit event itself
/// (spec 11 §7.8, §18.10). <c>subjectTokenIdHash</c> is deliberately sent as null so Manager
/// generates the token and returns its one-time plaintext.
/// </summary>
public class PublicLinkGrantClient(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager, asService: true)), IPublicLinkGrantClient
{
    internal const string CreatePublicLinkGrantMutation = @"
                mutation($command: CreatePublicLinkGrantCommandInput!) {
                    createPublicLinkGrant(command: $command) {
                        publicLinkGrantId
                        expiresAt
                        token
                    }
                }";

    internal const string RevokePublicLinkGrantMutation = @"
                mutation($command: RevokePublicLinkGrantCommandInput!) {
                    revokePublicLinkGrant(command: $command)
                }";

    internal const string ResolvePublicLinkGrantMutation = @"
                mutation($command: ResolvePublicLinkGrantCommandInput!) {
                    resolvePublicLinkGrant(command: $command) {
                        resolution
                        publicLinkGrantId
                        resourceId
                    }
                }";

    private const string FoundResolution = "FOUND";
    private const string ExpiredResolution = "EXPIRED";

    public async Task<PublicLinkGrantResultVm> CreateAsync(
        Guid accountId,
        string resourceType,
        string resourceId,
        string scopes,
        string purpose,
        DateTimeOffset expiresAt,
        string createdByPrincipalId,
        CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = CreatePublicLinkGrantMutation,
            Variables = new
            {
                command = new
                {
                    publicLinkGrant = new
                    {
                        accountId,
                        resourceType,
                        resourceId,
                        scopes,
                        purpose,
                        subjectTokenIdHash = (string?)null,
                        expiresAt,
                        createdByPrincipalId
                    }
                }
            }
        };
        var grant = await MutationAsync<PublicLinkGrantResponse>(request, cancellationToken);
        return new PublicLinkGrantResultVm(grant.PublicLinkGrantId, grant.Token, grant.ExpiresAt);
    }

    public async Task RevokeAsync(Guid publicLinkGrantId, string revokedBy, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = RevokePublicLinkGrantMutation,
            Variables = new { command = new { publicLinkGrantId, revokedBy } }
        };
        await MutationAsync<bool>(request, cancellationToken);
    }

    public async Task<PublicLinkResolutionVm> ResolveAsync(
        Guid publicLinkGrantId,
        Guid accountId,
        string resourceType,
        string resourceId,
        string scope,
        string token,
        CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = ResolvePublicLinkGrantMutation,
            Variables = new
            {
                command = new
                {
                    publicLinkGrantId,
                    accountId,
                    resourceType,
                    resourceId,
                    scope,
                    token
                }
            }
        };
        var resolved = await MutationAsync<PublicLinkResolutionResponse>(request, cancellationToken);
        return new PublicLinkResolutionVm(MapResolution(resolved.Resolution), resolved.PublicLinkGrantId, resolved.ResourceId);
    }

    // NOT_FOUND is the default: an unknown literal must never widen into a disclosure.
    private static PublicTripResolution MapResolution(string? resolution) => resolution switch
    {
        FoundResolution => PublicTripResolution.Found,
        ExpiredResolution => PublicTripResolution.Expired,
        _ => PublicTripResolution.NotFound
    };
}

internal sealed record PublicLinkGrantResponse(Guid PublicLinkGrantId, DateTimeOffset ExpiresAt, string? Token);

internal sealed record PublicLinkResolutionResponse(string? Resolution, Guid? PublicLinkGrantId, string? ResourceId);
