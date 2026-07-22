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

namespace TrackHub.TripManagement.Application.PublicTrips.Services.Interfaces;

/// <summary>
/// Resolves an anonymous public tracking link.
/// <para>
/// This is a SERVICE rather than a mediator request on purpose: the anonymous endpoint must bypass
/// the pipeline entirely, because <c>AuthorizationBehavior</c> and <c>AccountStatusBehavior</c>
/// both assume a principal and there is none here (spec 11 §7.8). The endpoint resolves this
/// interface directly.
/// </para>
/// <para>
/// There is deliberately NO mediator request wrapping this resolver. A <c>ResolvePublicTripQuery</c>
/// existed, wired to nothing, carrying no <c>[Authorize]</c> — one <c>partial Query</c> method away
/// from becoming an unauthenticated GraphQL surface that returns another account's trip to anyone
/// who can guess a grant id. It was deleted rather than annotated: an unreachable request with the
/// right attributes is still a request someone can reach later, whereas a type that does not exist
/// cannot be exposed. Public trip data has exactly one path out of this service — the anonymous
/// REST endpoint, which resolves the grant through Manager first.
/// </para>
/// </summary>
public interface IPublicTripResolver
{
    Task<PublicTripResultVm> ResolveAsync(
        Guid publicLinkGrantId,
        Guid accountId,
        string resourceId,
        string token,
        CancellationToken cancellationToken);
}
