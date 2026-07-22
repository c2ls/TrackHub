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

using Common.Infrastructure;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Entities;

/// <summary>
/// The local half of a public share. The grant itself lives in Manager; this row records exactly
/// which field groups the public snapshot may expose, because public projection is
/// field-configured server-side and never filtered by the client (spec 11 section 5,
/// acceptance 23).
/// </summary>
public sealed class TripShare : BaseAuditableEntity
{
    public Guid TripShareId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Guid TripId { get; set; }
    public Guid PublicLinkGrantId { get; set; }
    public bool IncludeDriverName { get; set; }
    public bool IncludeVehicle { get; set; }
    public bool IncludeLivePosition { get; set; }
    public bool IncludeStopDetail { get; set; }
    public bool IncludePodSummary { get; set; }

    /// <summary>
    /// Gates the planned route geometry. Spec §7.8 lists the route as exposed "per field flags" but
    /// §6.1's field list omits the flag; §7.8 is the disclosure contract, so the flag exists here.
    /// Defaults to <c>false</c> — a disclosure flag fails closed, and a share created with every
    /// box unticked must not hand out the trip's full planned route.
    /// </summary>
    public bool IncludeRoute { get; set; }
    public string CreatedByPrincipalId { get; set; } = string.Empty;

    /// <summary>
    /// Mirror of the Manager grant expiry. Not in the spec 6.1 field list, but required by the
    /// frozen ITripShareWriter.CreateShareAsync / TripShareVm.ExpiresAt contract - the share list
    /// must render expiry without a cross-service call per row.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public Trip? Trip { get; set; }
}
