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
/// Interface for writing geofence event data.
/// </summary>
public interface IGeofenceEventWriter
{
    /// <summary>
    /// Creates an entry event when a transporter enters a geofence. Returns null when a visit
    /// with the same transporter/geofence/entry timestamp already exists (redelivered batch) —
    /// the caller must skip counting, tracking, and alert emission for it.
    /// </summary>
    Task<GeofenceEventVm?> CreateEntryEventAsync(
        GeofenceEventDto geofenceEvent,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates an event with departure information when a transporter exits a geofence.
    /// </summary>
    Task<GeofenceEventVm> UpdateExitEventAsync(
        Guid geofenceEventId,
        DateTimeOffset departureTimestamp,
        CancellationToken cancellationToken);

    /// <summary>
    /// Stamps the one-time dwell alert emission for a visit.
    /// </summary>
    Task StampDwellAlertedAsync(
        Guid geofenceEventId,
        DateTimeOffset alertedAt,
        CancellationToken cancellationToken);
}
