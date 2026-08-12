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

using TrackHub.Geofencing.Domain.Records;

namespace TrackHub.Geofencing.Domain.Interfaces;

/// <summary>
/// Emits geofence alert events toward the Manager alert pipeline.
/// Emission is best-effort: callers must never fail position processing on emitter errors.
/// </summary>
public interface IAlertEmitter
{
    Task EmitGeofenceEnteredAsync(GeofenceAlertDto alert, CancellationToken cancellationToken);
    Task EmitGeofenceExitedAsync(GeofenceAlertDto alert, CancellationToken cancellationToken);
    Task EmitGeofenceDwellExceededAsync(GeofenceAlertDto alert, CancellationToken cancellationToken);
}
