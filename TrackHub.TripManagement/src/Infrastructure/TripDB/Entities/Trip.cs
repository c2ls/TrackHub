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
using NetTopologySuite.Geometries;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Entities;

/// <summary>
/// The dispatch aggregate root (spec 11 section 6.1, 18.3). A dispatch code is a field on this
/// entity, not a parent aggregate.
/// </summary>
public sealed class Trip : BaseAuditableEntity
{
    public Guid TripId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = TripStatuses.Created;
    public Guid TransporterId { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? RoutePlanId { get; set; }
    public Guid? ServiceOrderId { get; set; }
    public string? ExternalReference { get; set; }
    public string? CustomerName { get; set; }
    public string OriginName { get; set; } = string.Empty;
    public Point OriginPoint { get; set; } = default!;
    public DateTimeOffset PlannedStartAt { get; set; }
    public DateTimeOffset? PlannedEndAt { get; set; }
    public DateTimeOffset? ActualStartAt { get; set; }
    public DateTimeOffset? ActualEndAt { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? LastPositionAt { get; set; }
    public Point? LastPoint { get; set; }
    public double ActualDistanceMeters { get; set; }
    public string? TollVehicleClass { get; set; }

    /// <summary>
    /// Stamped only after a TripRouteDeviation alert was successfully emitted, so a failed
    /// emission is retried on the next cycle rather than silently swallowed (spec 11 section 7.4).
    /// </summary>
    public DateTimeOffset? DeviationOpenedAt { get; set; }

    /// <summary>
    /// Run length of consecutive out-of-corridor fixes, PERSISTED because Router pushes exactly one
    /// position per transporter per call: an in-memory counter rebuilt per request could never reach
    /// the three-fix threshold, which is why corridor deviation never fired (spec 11 section 7.4).
    /// Reset to 0 by any fix inside the corridor.
    /// </summary>
    public int ConsecutiveOutsideFixes { get; set; }
    public string? CancellationReason { get; set; }
}
