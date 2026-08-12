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

namespace TrackHub.Geofencing.Infrastructure.Entities;

/// <summary>
/// Entity representing a geofence entry or exit event.
/// </summary>
public class GeofenceEvent(
    Guid transporterId,
    Guid geofenceId,
    Guid accountId,
    DateTimeOffset eventDateTime,
    double latitude,
    double longitude)
{
    public Guid GeofenceEventId { get; private set; } = Guid.NewGuid();
    public Guid TransporterId { get; set; } = transporterId;
    public Guid GeofenceId { get; set; } = geofenceId;
    public Guid AccountId { get; set; } = accountId;
    public DateTimeOffset EventDateTime { get; set; } = eventDateTime;
    /// <summary>
    /// Exit timestamp (null if still inside geofence).
    /// </summary>
    public DateTimeOffset? DepartureTimestamp { get; set; }
    /// <summary>
    /// Entry location.
    /// </summary>
    public double Latitude { get; set; } = latitude;
    public double Longitude { get; set; } = longitude;
    /// <summary>
    /// One-time dwell alert emission stamp for this visit (null = not alerted).
    /// </summary>
    public DateTimeOffset? DwellAlertedAt { get; set; }
    // Navigation properties
    public Geofence? Geofence { get; set; }
}
