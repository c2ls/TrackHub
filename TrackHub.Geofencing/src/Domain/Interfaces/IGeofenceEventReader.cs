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

namespace TrackHub.Geofencing.Domain.Interfaces;

/// <summary>
/// Interface for reading geofence event data.
/// </summary>
public interface IGeofenceEventReader
{
    /// <summary>
    /// Gets open (no departure) geofence events for a transporter within the account.
    /// </summary>
    Task<IReadOnlyCollection<GeofenceEventVm>> GetOpenEventsForTransporterAsync(Guid transporterId, Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a server-side page of geofence events filtered by account, user visibility, date range,
    /// optional transporter/geofence, and open-visit-only flag.
    /// </summary>
    Task<GeofenceEventsPageVm> GetGeofenceEventsAsync(
        Guid accountId,
        Guid userId,
        DateTimeOffset fromDate,
        DateTimeOffset toDate,
        Guid? transporterId,
        Guid? geofenceId,
        bool openOnly,
        int skip,
        int take,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets open visits eligible for a dwell alert: geofence defines a threshold, the account has
    /// geofencing enabled and is not suspended, and the visit has not been dwell-alerted yet.
    /// The elapsed-time check against the threshold is the caller's responsibility.
    /// </summary>
    Task<IReadOnlyCollection<DwellAlertCandidateVm>> GetDwellAlertCandidatesAsync(
        CancellationToken cancellationToken);
}
