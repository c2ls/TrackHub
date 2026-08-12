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

public interface IGeofenceReader
{
    Task<GeofenceVm> GetGeofenceAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a server-side page of the account's geofences with optional type/active/name filters.
    /// </summary>
    Task<GeofencesPageVm> GetGeofencesPageAsync(
        Guid accountId,
        int skip,
        int take,
        short? type,
        bool? active,
        string? search,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets alert opt-in metadata (name, type, entry/exit flags) for the account's active geofences.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, GeofenceAlertInfoVm>> GetActiveGeofenceAlertInfoAsync(
        Guid accountId,
        CancellationToken cancellationToken);


    /// <summary>
    /// Gets the IDs of all active geofences that contain the specified point.
    /// Uses spatial indexing for efficient lookup.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetGeofenceIdsContainingPointAsync(
        Guid accountId,
        double latitude,
        double longitude,
        CancellationToken cancellationToken);
}

