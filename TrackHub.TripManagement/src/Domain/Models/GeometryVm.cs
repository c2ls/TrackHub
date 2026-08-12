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

namespace TrackHub.TripManagement.Domain.Models;

/// <summary>
/// A single WGS-84 coordinate. Geometry never crosses the API boundary as a NetTopologySuite
/// type — readers project it into these plain records (the Geofencing <c>CoordinateVm</c> pattern).
/// </summary>
public readonly record struct CoordinateVm(double Latitude, double Longitude);

/// <summary>An ordered vertex list — a planned route line or a corridor/arrival ring.</summary>
public readonly record struct GeometryLineVm(IReadOnlyCollection<CoordinateVm> Coordinates);

/// <summary>
/// A route-replay point stream. <paramref name="Truncated"/> is explicit: Telemetry's 10 000-point
/// cap must never silently shorten a route (spec 11 §7.5, acceptance 22).
/// </summary>
public readonly record struct RouteReplayVm(
    IReadOnlyCollection<RouteReplayPointVm> Points,
    bool Truncated);

public readonly record struct RouteReplayPointVm(
    double Latitude,
    double Longitude,
    DateTimeOffset DeviceTimestamp,
    double? Speed);
