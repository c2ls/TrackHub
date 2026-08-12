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

namespace TrackHub.TripManagement.Domain.Interfaces;

/// <summary>
/// Public-link lifecycle, delegated to Manager. A parallel link mechanism is forbidden — Manager
/// owns token hashing, access counting and the <c>PublicLinkAccessed</c> audit event, in exactly
/// one implementation shared with its own anonymous REST endpoint (spec 11 §18.10).
/// </summary>
public interface IPublicLinkGrantClient
{
    /// <summary>
    /// Creates the grant and returns the plaintext token — available exactly once, at creation.
    /// Manager's <c>[RequireFeature(public-links)]</c> still applies, so an account without the
    /// key gets <c>FEATURE_DISABLED</c> from Manager rather than a silently degraded share.
    /// </summary>
    Task<PublicLinkGrantResultVm> CreateAsync(
        Guid accountId,
        string resourceType,
        string resourceId,
        string scopes,
        string purpose,
        DateTimeOffset expiresAt,
        string createdByPrincipalId,
        CancellationToken cancellationToken);

    Task RevokeAsync(Guid publicLinkGrantId, string revokedBy, CancellationToken cancellationToken);

    /// <summary>
    /// Resolves an anonymous token through Manager's shared resolution command, which counts the
    /// access and writes the audit event. Returns the trichotomy the endpoint maps to
    /// 200 / 404 / 410 — never an exception, because 404 and 410 are normal answers.
    /// </summary>
    Task<PublicLinkResolutionVm> ResolveAsync(
        Guid publicLinkGrantId,
        Guid accountId,
        string resourceType,
        string resourceId,
        string scope,
        string token,
        CancellationToken cancellationToken);
}

/// <summary>Grant creation result. <paramref name="Token"/> is never re-readable afterwards.</summary>
public readonly record struct PublicLinkGrantResultVm(Guid PublicLinkGrantId, string? Token, DateTimeOffset ExpiresAt);

public readonly record struct PublicLinkResolutionVm(PublicTripResolution Resolution, Guid? PublicLinkGrantId, string? ResourceId);
