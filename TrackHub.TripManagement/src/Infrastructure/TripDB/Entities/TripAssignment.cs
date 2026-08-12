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
/// Driver/transporter assignment history. At most ONE Active row per trip, enforced by a partial
/// unique index rather than by handler discipline (spec 11 section 6.1).
/// </summary>
public sealed class TripAssignment : BaseAuditableEntity
{
    public Guid TripAssignmentId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Guid TripId { get; set; }
    public Guid DriverId { get; set; }
    public Guid TransporterId { get; set; }
    public string Status { get; set; } = TripAssignmentStatuses.Active;
    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }

    public Trip? Trip { get; set; }
}
