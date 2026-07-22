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

public interface ITripShareWriter
{
    /// <summary>
    /// Stores the local half of a share. The grant itself lives in Manager; this row records
    /// exactly which field groups the snapshot may expose, because public projection is
    /// field-configured server-side and never filtered by the client (spec 11 §5).
    /// </summary>
    Task<TripShareVm> CreateShareAsync(
        Guid tripId,
        Guid accountId,
        Guid publicLinkGrantId,
        TripShareFieldFlagsDto fieldFlags,
        string createdByPrincipalId,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Stamps <c>RevokedAt</c> locally and returns the <b>Manager <c>PublicLinkGrantId</c></b> the
    /// share is backed by — not the local <c>TripShareId</c>. The caller feeds the returned value
    /// straight into Manager's revoke, so returning the wrong id would leave the public link live
    /// after a successful-looking revoke (acceptance 24). Idempotent: re-revoking keeps the first
    /// revocation instant.
    /// </summary>
    Task<Guid> RevokeShareAsync(Guid tripShareId, Guid accountId, CancellationToken cancellationToken);
}
