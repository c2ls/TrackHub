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

using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

/// <summary>
/// The one and only public-link resolution implementation (spec 11 §7.8/§18.10, closing spec 02 §7.4).
/// <para>
/// Previously this logic existed twice and diverged: the anonymous REST endpoint hashed, matched,
/// counted access and wrote a <c>PublicLinkAccessed</c> audit row inline against the DbContext, while
/// <c>PublicLinkGrantWriter.RecordPublicLinkAccessAsync</c> counted access a SECOND time and wrote a
/// DIFFERENT action name (<c>RecordPublicLinkAccess</c>). Both are collapsed here.
/// </para>
/// <para>
/// Deliberately does NOT derive from <see cref="AccountScopedDataAccess"/>: the anonymous caller has
/// no principal, so there is no account scope to enforce and no <c>ICurrentPrincipal</c> to read. The
/// token hash IS the authorization — a caller must present a grant id, its account, its resource and
/// the pre-image of the stored SHA-256 hash before anything is returned or written.
/// </para>
/// </summary>
public sealed class PublicLinkGrantResolver(IApplicationDbContext context) : IPublicLinkGrantResolver
{
    /// <summary>
    /// Audit actor type for a public-link resolution. This is the audit contract the anonymous REST
    /// endpoint has always written and downstream audit consumers match on — do not "normalize" it to
    /// the calling principal's type just because a ServiceClient may now be the transport.
    /// </summary>
    private const string PublicLinkActorType = "PublicLink";

    /// <summary>Audit action name for a successful resolution. Part of the audit contract.</summary>
    private const string PublicLinkAccessedAction = "PublicLinkAccessed";

    public async Task<PublicLinkGrantResolutionResult> ResolvePublicLinkGrantAsync(
        Guid publicLinkGrantId,
        Guid accountId,
        string resourceType,
        string resourceId,
        string scope,
        string token,
        CancellationToken cancellationToken)
    {
        var tokenHash = PublicLinkTokenHasher.Hash(token);
        var grant = await context.PublicLinkGrants
            .FirstOrDefaultAsync(x =>
                x.PublicLinkGrantId == publicLinkGrantId
                && x.AccountId == accountId
                && x.ResourceType == resourceType
                && x.ResourceId == resourceId
                && x.SubjectTokenIdHash == tokenHash,
                cancellationToken);

        // No match, revoked, or the requested scope was never granted are all a flat NotFound:
        // distinguishing them would let a caller probe which grants exist.
        if (grant == null || grant.RevokedAt.HasValue || !HasScope(grant.Scopes, scope))
        {
            return new PublicLinkGrantResolutionResult(PublicLinkResolution.NotFound, null);
        }

        if (grant.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return new PublicLinkGrantResolutionResult(PublicLinkResolution.Expired, null);
        }

        // ApplicationDbContext is configured NoTracking, so the grant comes back untracked and must be
        // attached for these writes to reach the same SaveChanges as the audit row below.
        context.PublicLinkGrants.Attach(grant);
        grant.AccessCount++;
        grant.LastAccessedAt = DateTimeOffset.UtcNow;
        context.AuditEvents.Add(new AuditEvent(
            grant.AccountId,
            PublicLinkActorType,
            grant.PublicLinkGrantId.ToString(),
            PublicLinkAccessedAction,
            "PublicLinkGrant",
            grant.PublicLinkGrantId.ToString(),
            "Succeeded",
            null,
            $$"""{"resourceType":"{{grant.ResourceType}}","resourceId":"{{grant.ResourceId}}","scope":"{{scope}}","accessCount":{{grant.AccessCount}}}""",
            null,
            null,
            null,
            null));
        await context.SaveChangesAsync(cancellationToken);

        return new PublicLinkGrantResolutionResult(PublicLinkResolution.Found, ToVm(grant));
    }

    private static PublicLinkGrantVm ToVm(PublicLinkGrant x)
        => new(x.PublicLinkGrantId, x.AccountId, x.ResourceType, x.ResourceId, x.Scopes, x.Purpose, x.ExpiresAt, x.RevokedAt, x.RevokedBy, x.CreatedByPrincipalId, x.AccessCount, x.LastAccessedAt, x.LastModified, null);

    // Scopes are stored space- or comma-separated; comparison is case-insensitive.
    private static bool HasScope(string scopes, string requestedScope)
        => scopes
            .Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(scope => string.Equals(scope, requestedScope, StringComparison.OrdinalIgnoreCase));
}
