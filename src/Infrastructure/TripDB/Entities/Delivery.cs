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

/// <summary>A consignment dropped at a stop. A delivery never moves between stops.</summary>
public sealed class Delivery : BaseAuditableEntity
{
    public Guid DeliveryId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Guid TripStopId { get; set; }
    public string? Reference { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? BranchName { get; set; }
    public string? ProductsSummary { get; set; }
    public string Status { get; set; } = DeliveryStatuses.Pending;
    public string? Observations { get; set; }
    public int SequenceIndex { get; set; }

    public TripStop? TripStop { get; set; }
}
