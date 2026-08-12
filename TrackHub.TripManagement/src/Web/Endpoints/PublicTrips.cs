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

using TrackHub.TripManagement.Application.PublicTrips.Services.Interfaces;

namespace TrackHub.TripManagement.Web.Endpoints;

/// <summary>
/// Anonymous customer tracking.
/// <para>
/// It <b>bypasses the mediator deliberately</b>: <c>AuthorizationBehavior</c> and
/// <c>AccountStatusBehavior</c> both resolve a principal, and a public-link subject is not one.
/// Running this through the pipeline would either fail closed on every request or require
/// weakening two behaviours that exist to protect every other surface — so the resolver is
/// injected directly instead (spec 11 §7.8).
/// </para>
/// <para>
/// Rate-limited per client IP, which is why the service also runs <c>UseForwardedHeaders</c>:
/// behind nginx every request would otherwise appear to come from the proxy and collapse the per-IP
/// partition into one shared bucket.
/// </para>
/// <para>
/// <b>Deliberately NOT output-cached.</b> This endpoint originally carried the spec-28 announcements
/// precedent's 30 s <c>CacheOutput</c>, but that precedent does not transfer: announcements are a
/// genuinely public, non-revocable, subject-less payload, so serving one 30 s stale costs nothing.
/// A public tracking link is the opposite — a revocable, expiring, per-subject credential. Caching
/// it broke acceptance 24 twice over: the cache key (grantId + account + resource + token) had no
/// invalidation on revoke, so a revoked link kept returning 200 for up to 30 s; and because a cache
/// hit never reaches Manager's resolver, it incremented no access count and wrote no
/// <c>PublicLinkAccessed</c> audit event, while acceptance 24 requires that of <i>every</i>
/// successful resolution. Evicting by tag on revoke would have fixed only the first half — the
/// audit and access-count guarantee is unsatisfiable by any response cache, because the guarantee
/// is precisely that the request reaches the resolver. Per-IP rate limiting is the abuse control
/// here; do not reintroduce <c>CacheOutput</c> on this route.
/// </para>
/// </summary>
public sealed class PublicTrips : EndpointGroupBase
{
    /// <summary>Rate-limit policy name; the policy itself is defined in Program.cs.</summary>
    public const string RateLimitPolicy = "public-trip-tracking";

    public override void Map(WebApplication app)
        => app.MapGet("~/public/trips/{publicLinkGrantId:guid}", GetPublicTrip)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicy);

    // GET ~/public/trips/{publicLinkGrantId}?accountId&resourceId&token
    // 200 Found / 404 NotFound (missing, revoked, wrong scope, or feature disabled) / 410 Expired.
    public static async Task<IResult> GetPublicTrip(
        Guid publicLinkGrantId,
        Guid accountId,
        string? resourceId,
        string? token,
        IPublicTripResolver resolver,
        CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty || string.IsNullOrWhiteSpace(resourceId) || string.IsNullOrWhiteSpace(token))
            return Results.BadRequest();

        var result = await resolver.ResolveAsync(publicLinkGrantId, accountId, resourceId, token, cancellationToken);

        return result.Resolution switch
        {
            PublicTripResolution.Found when result.Trip is { } trip => Results.Ok(trip),
            PublicTripResolution.Expired => Results.StatusCode(StatusCodes.Status410Gone),
            _ => Results.NotFound(),
        };
    }
}
