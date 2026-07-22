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

/// <summary>A planned route as returned by the routing provider, before persistence.</summary>
public readonly record struct RouteResultVm(
    IReadOnlyCollection<CoordinateVm> Geometry,
    double DistanceMeters,
    int DurationSeconds,
    IReadOnlyCollection<RouteLegVm> Legs);

public readonly record struct RouteLegVm(int Index, double DistanceMeters, int DurationSeconds);

/// <summary>Distance/duration only — the ETA path never asks for geometry it will not store.</summary>
public readonly record struct RouteSummaryVm(double DistanceMeters, int DurationSeconds);
