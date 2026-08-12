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
/// External routing. The only implementation is OpenRouteService, but the seam exists so a
/// self-hosted instance or a future provider is a configuration change rather than a code change.
/// </summary>
public interface IRoutingProvider
{
    /// <summary>Provider name recorded on the resulting <c>RoutePlan</c>.</summary>
    string Name { get; }

    /// <summary>
    /// False when no API key / base URL is configured. Callers turn this into a
    /// <c>Failed</c> plan carrying <c>ROUTING_NOT_CONFIGURED</c> — absence of configuration is a
    /// deployment error, but it must still leave the trip fully usable (spec 11 §14).
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Routes through the ordered waypoints. Throws <see cref="Exceptions.RoutingUnavailableException"/>
    /// on provider failure; it is the caller's job to record that as a failed plan, never to
    /// propagate it to the trip command's caller.
    /// </summary>
    Task<RouteResultVm> GetRouteAsync(IReadOnlyCollection<CoordinateVm> waypoints, CancellationToken cancellationToken);

    /// <summary>
    /// Distance/duration from a live point to the next stop, for ETA refresh. Cheaper than a
    /// full route: no geometry is requested.
    /// </summary>
    Task<RouteSummaryVm> GetSummaryAsync(CoordinateVm from, CoordinateVm to, CancellationToken cancellationToken);
}
