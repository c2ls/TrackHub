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
/// Proof of delivery. Idempotent on the unique (TripStopId, ClientEventId) index so the spec 10
/// offline outbox can retry a submission safely (spec 11 section 6.1, acceptance 15).
/// </summary>
public sealed class ProofOfDelivery : BaseAuditableEntity
{
    public Guid ProofOfDeliveryId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Guid TripStopId { get; set; }
    public Guid? DeliveryId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverDocument { get; set; }
    public DateTimeOffset CapturedAt { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Notes { get; set; }
    public Guid ClientEventId { get; set; }

    public TripStop? TripStop { get; set; }
}
