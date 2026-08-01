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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Rate limiting for ANONYMOUS HTTP endpoints.
/// <para>
/// The mediator's <c>RateLimitingBehavior</c> cannot cover these: it partitions on the
/// authenticated user or client and returns early when the partition key is empty, so an
/// unauthenticated endpoint routed through it is effectively unlimited. Anonymous endpoints
/// therefore need an ASP.NET Core limiter, and this is the single definition of it — every
/// anonymous surface on the platform (Manager's platform-status feed, TripManagement's public trip
/// links) gets the same partitioning and the same 429 semantics instead of each hand-rolling a
/// policy that then drifts.
/// </para>
/// <para>
/// Partitioned PER CLIENT IP, never a single global bucket: one shared budget across every caller
/// means the endpoint starts rejecting as soon as a handful of legitimate clients use it — it would
/// fail hardest during exactly the incident or rollout it exists to serve — and it hands any single
/// attacker a trivial denial of service against everybody else.
/// </para>
/// <para>
/// <b>Requires <c>UseForwardedHeaders</c> in the host pipeline, with <c>KnownProxies</c>/
/// <c>KnownIPNetworks</c> cleared.</b> Behind nginx every request otherwise appears to come from
/// the proxy's container IP, which silently collapses every partition into one bucket — the exact
/// failure this partitioning exists to avoid.
/// </para>
/// </summary>
public static class AnonymousRateLimiting
{
    /// <summary>
    /// Registers a named fixed-window policy partitioned by client IP, and sets the rejection status
    /// to 429. Call once per policy; the underlying <c>AddRateLimiter</c> is additive, so several
    /// anonymous endpoints in one host each register their own policy name and limits.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="policyName">Policy name, referenced from the endpoint's <c>RequireRateLimiting</c>.</param>
    /// <param name="permitLimit">Requests allowed per window, per client IP.</param>
    /// <param name="window">The fixed window length.</param>
    public static IServiceCollection AddAnonymousEndpointRateLimiter(
        this IServiceCollection services,
        string policyName,
        int permitLimit,
        TimeSpan window)
        => services.AddRateLimiter(options =>
        {
            // ASP.NET Core's default rejection status is 503, which tells a client the SERVER is
            // unhealthy and invites the retry storm the limiter exists to stop. 429 is the accurate
            // answer and the one CDNs, SDKs and monitoring already understand.
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(policyName, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = window,
                        QueueLimit = 0,
                    }));
        });
}
