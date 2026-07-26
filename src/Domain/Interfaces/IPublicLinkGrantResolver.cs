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

namespace TrackHub.Manager.Domain.Interfaces;

/// <summary>
/// The single implementation of anonymous public-link resolution (spec 02 §7.4, spec 11 §7.8/§18.10).
/// <para>
/// It is deliberately NOT an <c>IPublicLinkGrantWriter</c> member: the caller has no principal to
/// scope by, so it must not inherit the account-scoped write checks. Access counting, the
/// <c>LastAccessedAt</c> stamp and the <c>PublicLinkAccessed</c> audit event happen here and
/// nowhere else — both the anonymous REST endpoint and the ServiceClient-only
/// <c>resolvePublicLinkGrant</c> mutation funnel through this one method.
/// </para>
/// </summary>
public interface IPublicLinkGrantResolver
{
    /// <summary>
    /// Matches a grant by id + account + resource + SHA-256 token hash, enforces revocation, scope
    /// and expiry, and — only on success — increments the access count and writes exactly one
    /// <c>PublicLinkAccessed</c> audit row.
    /// </summary>
    Task<PublicLinkGrantResolutionResult> ResolvePublicLinkGrantAsync(
        Guid publicLinkGrantId,
        Guid accountId,
        string resourceType,
        string resourceId,
        string scope,
        string token,
        CancellationToken cancellationToken);
}
