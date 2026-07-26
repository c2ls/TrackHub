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

using Common.Web.Infrastructure;
using TrackHub.Manager.Domain.Enums;
using TrackHub.Manager.Domain.Interfaces;

namespace TrackHub.Manager.Web.Endpoints;

/// <summary>
/// Anonymous public-link resolution. The HTTP contract is fixed: same route, same query parameters,
/// same 400/404/410/200 semantics and the same JSON body shape.
/// <para>
/// This endpoint calls <see cref="IPublicLinkGrantResolver"/> DIRECTLY rather than sending
/// <c>ResolvePublicLinkGrantCommand</c> through the mediator. That is the point of extracting the
/// logic behind an interface instead of behind the command alone: the request is anonymous, so there
/// is no principal for <c>AuthorizationBehavior</c> (or <c>AccountStatusBehavior</c>) to evaluate.
/// Routing it through the pipeline would mean either widening the command's <c>PrincipalTypes</c> to
/// admit unauthenticated callers — which would also expose it to real unauthenticated GraphQL
/// traffic — or bolting a bypass onto the pipeline itself. Calling the shared service keeps the
/// anonymity local to this one endpoint while still guaranteeing one implementation of hashing,
/// scope checking, access counting and the `PublicLinkAccessed` audit event.
/// </para>
/// </summary>
public sealed class PublicLinks : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGet("~/public-links/{publicLinkGrantId:guid}", GetPublicLink);
    }

    public static async Task<IResult> GetPublicLink(
        Guid publicLinkGrantId,
        Guid accountId,
        string resourceType,
        string resourceId,
        string scope,
        string token,
        IPublicLinkGrantResolver resolver,
        CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty
            || string.IsNullOrWhiteSpace(resourceType)
            || string.IsNullOrWhiteSpace(resourceId)
            || string.IsNullOrWhiteSpace(scope)
            || string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest();
        }

        var result = await resolver.ResolvePublicLinkGrantAsync(
            publicLinkGrantId, accountId, resourceType, resourceId, scope, token, cancellationToken);

        if (result.Resolution == PublicLinkResolution.Expired)
        {
            return Results.StatusCode(StatusCodes.Status410Gone);
        }

        // NotFound covers "no such grant", "revoked" and "scope not granted" alike — non-disclosure.
        if (result.Resolution != PublicLinkResolution.Found || result.Grant is not { } grant)
        {
            return Results.NotFound();
        }

        // Body shape is the pre-existing contract; the grant's Token is never included here.
        return Results.Ok(new
        {
            grant.PublicLinkGrantId,
            grant.AccountId,
            grant.ResourceType,
            grant.ResourceId,
            grant.Scopes,
            grant.Purpose,
            grant.ExpiresAt,
            grant.AccessCount,
            grant.LastAccessedAt
        });
    }
}
