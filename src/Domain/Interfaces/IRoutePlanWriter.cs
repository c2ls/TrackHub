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

namespace TrackHub.TripManagement.Domain.Interfaces;

/// <summary>
/// Route-plan persistence. A provider failure is stored as a <c>Failed</c> plan with an error
/// code — never surfaced as an exception to the trip command's caller, and never a reason the
/// trip cannot proceed (spec 11 §7.3, acceptance 18).
/// </summary>
public interface IRoutePlanWriter
{
    /// <summary>
    /// Stores a successful plan: geometry, the buffered corridor, distance/duration, per-leg
    /// breakdown and the toll estimate computed over it.
    /// </summary>
    Task<RoutePlanVm> SaveReadyPlanAsync(
        Guid tripId,
        Guid accountId,
        string provider,
        IReadOnlyCollection<CoordinateVm> geometry,
        int corridorMeters,
        double plannedDistanceMeters,
        int plannedDurationSeconds,
        string? waypointsJson,
        string? legsJson,
        TollEstimateVm tollEstimate,
        CancellationToken cancellationToken);

    Task<RoutePlanVm> SaveFailedPlanAsync(
        Guid tripId,
        Guid accountId,
        string provider,
        int corridorMeters,
        string errorCode,
        string errorMessage,
        CancellationToken cancellationToken);
}
