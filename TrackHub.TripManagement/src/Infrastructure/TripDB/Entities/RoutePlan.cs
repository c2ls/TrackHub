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
/// A planned route plus the toll estimate computed over it. A provider failure is stored as a
/// Failed plan carrying an error code - geometry is therefore nullable, because a failed plan has
/// none and the trip must stay fully usable regardless (spec 11 section 7.3, acceptance 18).
/// </summary>
public sealed class RoutePlan : BaseAuditableEntity
{
    public Guid RoutePlanId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; }
    public Guid TripId { get; set; }
    public string Provider { get; set; } = RoutePlanProviders.OpenRouteService;
    public LineString? Geom { get; set; }
    public Polygon? CorridorGeom { get; set; }
    public int CorridorMeters { get; set; } = TripGeometryDefaults.CorridorMeters;
    public double PlannedDistanceMeters { get; set; }
    public int PlannedDurationSeconds { get; set; }
    public string? WaypointsJson { get; set; }
    public string? LegsJson { get; set; }
    public DateTimeOffset ComputedAt { get; set; }
    public string Status { get; set; } = RoutePlanStatuses.Ready;
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? TollVehicleClass { get; set; }
    public decimal? EstimatedTollAmount { get; set; }
    public string? TollCurrency { get; set; }
    public string? TollStationsJson { get; set; }
    public string TollStatus { get; set; } = TollStatuses.NotComputed;

    public Trip? Trip { get; set; }
}
