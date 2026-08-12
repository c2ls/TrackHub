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

using Ardalis.GuardClauses;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Common.Application.Interfaces;
using System.Text.Json;

namespace Common.Infrastructure;

// The `GraphQLClientFactory` class is responsible for creating instances of `IGraphQLClient`.
public sealed class GraphQLClientFactory(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IGraphQLClientFactory
{
    // Refresh the cached client-credentials token this long before its actual expiry so a
    // token attached at client creation cannot expire mid-request.
    private static readonly TimeSpan TokenExpiryMargin = TimeSpan.FromSeconds(60);

    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiration;

    /// <summary>
    /// Creates a new instance of `IGraphQLClient` with the specified name.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <returns>An instance of `IGraphQLClient`.</returns>
    public IGraphQLClient CreateClient(string name) => CreateClient(name, asService: false);

    /// <summary>
    /// Creates a new instance of `IGraphQLClient` with the specified name. When `asService` is true,
    /// the client authenticates with the host's client-credentials identity even if the host normally
    /// propagates user tokens; a dedicated `{name}AsService` HttpClient is used so user-token header
    /// propagation configured on the named client does not apply.
    /// </summary>
    /// <param name="name">The name of the client.</param>
    /// <param name="asService">True to authenticate with client credentials.</param>
    /// <returns>An instance of `IGraphQLClient`.</returns>
    public IGraphQLClient CreateClient(string name, bool asService)
    {
        // Create an instance of `HttpClient` using the `IHttpClientFactory`.
        var httpClient = httpClientFactory.CreateClient(asService ? $"{name}AsService" : name);

        // Get the URL for the GraphQL service from the configuration.
        var url = configuration.GetValue<string>($"AppSettings:GraphQL{name}Service");

        // Ensure that the URL is not null.
        Guard.Against.Null(url, message: $"Setting 'GraphQL{name}Service' not found.");

        // Get the value of the `IsService` setting from the configuration.
        var isService = configuration.GetValue<bool?>($"AuthorityServer:IsService");
        if (asService || (isService.HasValue && isService.Value))
        {
            // Retrieve an access token using client credentials. Blocking here is accepted:
            // the token is cached process-wide, so this reaches the network at most once per
            // token lifetime, and client classes create their GraphQL client in constructors.
            var token = GetClientCredentialsTokenAsync().GetAwaiter().GetResult();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        // Create an instance of `GraphQLHttpClientOptions` and set the endpoint URL.
        var options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(url)
        };

        // Create an instance of `SystemTextJsonSerializer` for JSON serialization.
        var jsonSerializer = new SystemTextJsonSerializer();

        // Create and return a new instance of `GraphQLHttpClient`.
        return new GraphQLHttpClient(options, jsonSerializer, httpClient);
    }

    /// <summary>
    /// Retrieves an access token using client credentials.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="clientSecret">The client secret.</param>
    /// <param name="tokenUrl">The URL to retrieve the token from.</param>
    /// <returns>The access token.</returns>
    public async Task<string?> GetClientCredentialsTokenAsync()
    {
        if (_cachedToken != null && DateTimeOffset.UtcNow < _tokenExpiration)
        {
            return _cachedToken;
        }

        // The factory is a singleton: serialize refreshes so concurrent client creations
        // trigger a single token request instead of a stampede.
        await _tokenLock.WaitAsync();
        try
        {
            if (_cachedToken != null && DateTimeOffset.UtcNow < _tokenExpiration)
            {
                return _cachedToken;
            }

            var clientId = configuration.GetValue<string>($"AuthorityServer:ClientId");
            var clientSecret = configuration.GetValue<string>($"AuthorityServer:ClientSecret");
            var tokenUrl = configuration.GetValue<string>($"AuthorityServer:Authority");
            Guard.Against.Null(clientId, message: $"Setting 'ClientId' not found.");
            Guard.Against.Null(clientSecret, message: $"Setting 'ClientSecret' not found.");

            using var httpClient = httpClientFactory.CreateClient();
            var formData = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret }
            };
            var scope = configuration.GetValue<string>("AuthorityServer:Scope");
            if (!string.IsNullOrEmpty(scope))
            {
                formData.Add("scope", scope);
            }
            var request = new HttpRequestMessage(HttpMethod.Post, $"{tokenUrl}/token")
            {
                Content = new FormUrlEncodedContent(formData)
            };

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);
            var token = tokenResponse.GetProperty("access_token").GetString();
            var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();

            var lifetime = TimeSpan.FromSeconds(expiresIn);
            var margin = lifetime > TokenExpiryMargin + TokenExpiryMargin ? TokenExpiryMargin : TimeSpan.Zero;

            _cachedToken = token;
            _tokenExpiration = DateTimeOffset.UtcNow.Add(lifetime - margin);

            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }
}
