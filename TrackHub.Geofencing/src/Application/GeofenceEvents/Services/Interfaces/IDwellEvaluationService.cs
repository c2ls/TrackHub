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

namespace TrackHub.Geofencing.Application.GeofenceEvents.Services.Interfaces;

/// <summary>
/// Evaluates open visits against per-geofence dwell thresholds and emits one-time dwell alerts.
/// </summary>
public interface IDwellEvaluationService
{
    /// <summary>
    /// Runs one evaluation cycle and returns the number of dwell alerts emitted.
    /// </summary>
    Task<int> EvaluateDwellAsync(CancellationToken cancellationToken);
}
