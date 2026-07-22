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
/// The single idempotent trip event log. Manual overrides and automatic detections both land
/// here, discriminated by <see cref="Source"/> (spec 11 section 18.13).
/// <para>
/// <b>Deliberately <see cref="BaseEntity"/>, not BaseAuditableEntity</b> - spec 11 section 6.1 /
/// 18.14. This is the accepted AW-02 deviation already established for GeofenceEvent: the row is
/// system-generated and never user-edited, <see cref="OccurredAt"/> IS its creation record, and a
/// Created/CreatedBy/LastModified quartet on a high-volume append-only log would duplicate that
/// timestamp while claiming an actor that, for detection rows, does not exist. Do not "fix" this
/// by promoting it to auditable.
/// </para>
/// </summary>
public sealed class TripEvent : BaseEntity
{
    public Guid TripEventId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Guid TripId { get; set; }
    public Guid? TripStopId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public string Source { get; set; } = TripEventSources.Portal;
    public string? PayloadJson { get; set; }

    /// <summary>
    /// Unique. Idempotency is enforced by the database index, never by caller discipline - a
    /// retried submission hits the constraint and is reported as a duplicate (acceptance 15).
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    public Trip? Trip { get; set; }
}
