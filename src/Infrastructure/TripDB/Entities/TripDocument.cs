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
/// Link row to a Manager-owned Document. No storage surface is introduced here: trip documents
/// are created through the existing spec 04 upload with OwnerEntityType = "Transporter"
/// (spec 11 section 11, 18.6).
/// <para>
/// <b>Deliberately <see cref="BaseEntity"/>, not BaseAuditableEntity</b> (spec 11 section 6.1):
/// the row is a system-written association whose audit trail belongs to the document itself and
/// to the POD/trip event that produced it - the same AW-02 deviation reasoning as
/// <see cref="TripEvent"/>.
/// </para>
/// </summary>
public sealed class TripDocument : BaseEntity
{
    public Guid TripDocumentId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Guid TripId { get; set; }
    public Guid? TripStopId { get; set; }
    public Guid? ProofOfDeliveryId { get; set; }

    /// <summary>Identifier of the Manager-owned document; never a local blob.</summary>
    public Guid DocumentId { get; set; }
    public string Kind { get; set; } = TripDocumentKinds.Other;

    public Trip? Trip { get; set; }
}
