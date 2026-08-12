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

using Common.Application.Interfaces;
using GraphQL;
using Common.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Common.Infrastructure;

/// <summary>
/// Represents the implementation of the identity service.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IdentityService"/> class.
/// Authorization decisions (user role+policy and service-client permissions) are cached
/// in-memory for a short TTL (default 30 s, <c>AppSettings:AuthorizationCacheSeconds</c>,
/// 0 disables) — same staleness window already accepted for account status and feature flags.
/// </remarks>
/// <param name="graphQLClient">The GraphQL client factory.</param>
/// <param name="cache">The in-memory cache for authorization decisions.</param>
/// <param name="configuration">The configuration (cache TTL).</param>
public sealed class IdentityService(IGraphQLClientFactory graphQLClient, IMemoryCache cache, IConfiguration configuration)
    : GraphQLService(graphQLClient.CreateClient(Clients.Identity)), IIdentityService
{
    private readonly TimeSpan _decisionTtl = TimeSpan.FromSeconds(
        configuration.GetValue<int?>("AppSettings:AuthorizationCacheSeconds") ?? 30);

    // Single source of truth for the identity documents this client sends against Security;
    // the ServiceContracts tests validate these exact strings against the Security schema.
    internal const string UserNameQuery = @"
                    query($userId: UUID!) {
                        userName(query: { userId: $userId })
                    }";

    internal const string AuthorizeQuery = @"
                    query($action: String!, $resource: String!, $userId: UUID!) {
                        authorize(query: { action: $action, resource: $resource, userId: $userId })
                    }";

    internal const string IsInRoleQuery = @"
                    query($action: String!, $resource: String!, $userId: UUID!) {
                        isInRole(query: { action: $action, resource: $resource, userId: $userId })
                    }";

    internal const string AuthorizeUserQuery = @"
                    query($action: String!, $resource: String!, $userId: UUID!) {
                        authorizeUser(query: { action: $action, resource: $resource, userId: $userId })
                    }";

    internal const string IsValidServiceQuery = @"
                    query($client: String!) {
                        isValidService(query: { client: $client })
                    }";

    internal const string IsValidServiceForResourceQuery = @"
                    query($action: String!, $client: String!, $resource: String!) {
                        isValidServiceForResource(query: { action: $action, client: $client, resource: $resource })
                    }";

    internal const string IsValidServiceForResourceFullQuery = @"
                    query($accountId: UUID, $action: String!, $audiences: [String!], $client: String!, $resource: String!, $scopes: [String!]) {
                        isValidServiceForResource(query: { accountId: $accountId, action: $action, audiences: $audiences, client: $client, resource: $resource, scopes: $scopes })
                    }";

    private async Task<bool> GetOrCreateDecisionAsync(string key, Func<Task<bool>> factory)
    {
        if (_decisionTtl <= TimeSpan.Zero)
        {
            return await factory();
        }

        if (cache.TryGetValue(key, out bool cached))
        {
            return cached;
        }

        var decision = await factory();
        cache.Set(key, decision, _decisionTtl);
        return decision;
    }

    /// <summary>
    /// Retrieves the username associated with the specified user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The username.</returns>
    public Task<string> GetUserNameAsync(Guid userId, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = UserNameQuery,
            Variables = new { userId }
        };
        return QueryAsync<string>(request, token);
    }

    /// <summary>
    /// Checks if the specified user is authorized to perform the specified action on the specified resource.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="resource">The resource.</param>
    /// <param name="action">The action.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if the user is authorized, otherwise false.</returns>
    public Task<bool> AuthorizeAsync(Guid userId, string resource, string action, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = AuthorizeQuery,
            Variables = new { userId, resource, action }
        };
        return QueryAsync<bool>(request, token);
    }

    /// <summary>
    /// Checks if the specified user is in the specified role for the specified resource and action.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="resource">The resource.</param>
    /// <param name="action">The action.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if the user is in the role, otherwise false.</returns>
    public Task<bool> IsInRoleAsync(Guid userId, string resource, string action, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = IsInRoleQuery,
            Variables = new { userId, resource, action }
        };
        return QueryAsync<bool>(request, token);
    }

    /// <summary>
    /// Combined role + policy authorization decision, evaluated by Security in a single call
    /// and cached for the decision TTL. Used by the authorization pipeline.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="resource">The resource.</param>
    /// <param name="action">The action.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>True if the user is authorized, otherwise false.</returns>
    public Task<bool> AuthorizeUserAsync(Guid userId, string resource, string action, CancellationToken token)
        => GetOrCreateDecisionAsync($"authz:user:{userId}:{resource}:{action}", () =>
        {
            var request = new GraphQLRequest
            {
                Query = AuthorizeUserQuery,
                Variables = new { userId, resource, action }
            };
            return QueryAsync<bool>(request, token);
        });

    public Task<bool> IsValidServiceAsync(string? client, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = IsValidServiceQuery,
            Variables = new { client }
        };
        return QueryAsync<bool>(request, token);
    }

    public Task<bool> IsValidServiceAsync(string? client, string resource, string action, CancellationToken token)
    {
        var request = new GraphQLRequest
        {
            Query = IsValidServiceForResourceQuery,
            Variables = new { client, resource, action }
        };
        return QueryAsync<bool>(request, token);
    }

    public Task<bool> IsValidServiceAsync(string? client, string resource, string action, Guid? accountId, IReadOnlyCollection<string> scopes, IReadOnlyCollection<string> audiences, CancellationToken token)
        => GetOrCreateDecisionAsync(
            $"authz:svc:{client}:{resource}:{action}:{accountId}:{string.Join(',', scopes)}:{string.Join(',', audiences)}",
            () =>
            {
                var request = new GraphQLRequest
                {
                    Query = IsValidServiceForResourceFullQuery,
                    Variables = new { client, resource, action, accountId, scopes, audiences }
                };
                return QueryAsync<bool>(request, token);
            });
}
