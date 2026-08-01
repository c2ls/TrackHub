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

using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Resilience level for an inter-service GraphQL HttpClient. GraphQL always travels as POST,
/// so HTTP-method-based idempotency detection cannot distinguish queries from mutations:
/// clients that carry mutations must not retry.
/// </summary>
public enum GraphQLClientResilience
{
    /// <summary>No resilience pipeline (bare named client with timeout).</summary>
    None,

    /// <summary>Timeout + circuit breaker, retries disabled. Safe default for clients that carry mutations.</summary>
    NoRetry,

    /// <summary>Full standard pipeline including retries. Only for clients that exclusively run queries.</summary>
    WithRetry
}

/// <summary>
/// Single registration point for inter-service GraphQL HttpClients
/// (consumed through <c>IGraphQLClientFactory.CreateClient</c>). Guarantees every client gets
/// the same timeout, Authorization/x-correlation-id propagation, and an explicit resilience choice.
/// </summary>
public static class GraphQLClientServiceCollectionExtensions
{
    // Marker so header-propagation options are configured once per host even when several
    // Infrastructure projects register clients.
    private sealed class TrackHubHeaderPropagationConfigured { }

    /// <summary>
    /// Registers the named HttpClient for a user-token (header-propagating) GraphQL client.
    /// </summary>
    public static IHttpClientBuilder AddGraphQLClient(
        this IServiceCollection services,
        string name,
        bool propagateHeaders = true,
        GraphQLClientResilience resilience = GraphQLClientResilience.NoRetry,
        int timeoutSeconds = 30)
    {
        if (propagateHeaders)
        {
            services.AddTrackHubHeaderPropagation();
        }

        // Only load-bearing on the None path. AddStandardResilienceHandler replaces this with
        // InfiniteTimeSpan so the pipeline is the single source of truth for timeouts — which is
        // exactly why the pipeline has to be configured (ApplyTimeout) rather than left at its
        // 10 s-per-attempt defaults.
        var builder = services.AddHttpClient(name,
            client => client.Timeout = TimeSpan.FromSeconds(timeoutSeconds));

        if (propagateHeaders)
        {
            builder.AddHeaderPropagation();
        }

        switch (resilience)
        {
            case GraphQLClientResilience.NoRetry:
                builder.AddStandardResilienceHandler(options =>
                {
                    options.Retry.ShouldHandle = static _ => PredicateResult.False();
                    ApplyTimeout(options, timeoutSeconds);
                });
                break;
            case GraphQLClientResilience.WithRetry:
                builder.AddStandardResilienceHandler(options => ApplyTimeout(options, timeoutSeconds));
                break;
        }

        return builder;
    }

    /// <summary>
    /// Binds the resilience pipeline's timeouts to the caller's <paramref name="timeoutSeconds"/>.
    /// <para>
    /// WITHOUT this, <c>AddStandardResilienceHandler</c> keeps its defaults — a 10 s PER-ATTEMPT
    /// timeout and a 30 s total — and those fire long before <c>HttpClient.Timeout</c> ever does,
    /// because the pipeline sits inside it. A client registered with `timeoutSeconds: 120` was
    /// therefore still cut off at 10 s, and the declared timeout was silently inert. That is what
    /// killed Manager's sync dispatch to the Router (`Router-standard/Standard-AttemptTimeout`)
    /// whenever the Router's app pool was cold, even though the provider answered in under a second.
    /// </para>
    /// <para>
    /// The library validates these against each other, so they move together: the total must exceed
    /// a single attempt, and the breaker's sampling window must cover at least two attempts.
    /// </para>
    /// </summary>
    private static void ApplyTimeout(HttpStandardResilienceOptions options, int timeoutSeconds)
    {
        var attempt = TimeSpan.FromSeconds(timeoutSeconds);
        options.AttemptTimeout.Timeout = attempt;
        options.TotalRequestTimeout.Timeout = TotalFor(timeoutSeconds);
        options.CircuitBreaker.SamplingDuration = attempt * 2;
    }

    /// <summary>
    /// `timeoutSeconds` is the budget for ONE attempt. The total leaves room for the retry path's
    /// extra attempts so a single knob configures both; the breaker's sampling window must cover at
    /// least two attempts, which the library validates.
    /// </summary>
    private static TimeSpan TotalFor(int timeoutSeconds) => TimeSpan.FromSeconds(timeoutSeconds * 3);

    /// <summary>
    /// Registers the '{name}AsService' twin used by <c>IGraphQLClientFactory.CreateClient(name, asService: true)</c>:
    /// no user-token propagation; the factory attaches the host's client-credentials identity.
    /// </summary>
    public static IHttpClientBuilder AddGraphQLServiceClient(
        this IServiceCollection services,
        string name,
        GraphQLClientResilience resilience = GraphQLClientResilience.NoRetry,
        int timeoutSeconds = 30)
        => services.AddGraphQLClient($"{name}AsService", propagateHeaders: false, resilience, timeoutSeconds);

    /// <summary>
    /// Configures which inbound headers are propagated to outbound clients (Authorization and
    /// x-correlation-id). Idempotent across the multiple Infrastructure registrations of a host.
    /// </summary>
    public static IServiceCollection AddTrackHubHeaderPropagation(this IServiceCollection services)
    {
        if (!services.Any(d => d.ServiceType == typeof(TrackHubHeaderPropagationConfigured)))
        {
            services.AddSingleton<TrackHubHeaderPropagationConfigured>();
            services.AddHeaderPropagation(options =>
            {
                options.Headers.Add("Authorization");
                options.Headers.Add("x-correlation-id");
            });
        }

        return services;
    }
}
