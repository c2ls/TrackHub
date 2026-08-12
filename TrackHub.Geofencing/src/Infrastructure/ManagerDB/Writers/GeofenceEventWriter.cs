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

namespace TrackHub.Geofencing.Infrastructure.ManagerDB.Writers;

public sealed class GeofenceEventWriter(IApplicationDbContext context) : IGeofenceEventWriter
{
    public async Task<GeofenceEventVm?> CreateEntryEventAsync(
        GeofenceEventDto geofenceEvent,
        CancellationToken cancellationToken)
    {
        var geofence = await context.Geofences
            .FirstOrDefaultAsync(g => g.GeofenceId == geofenceEvent.GeofenceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Geofence with ID {geofenceEvent.GeofenceId} not found.");

        // Redelivery guard: identical device timestamps only occur when a batch is processed
        // again. The open-visit cache dedupes still-open visits; this catches the case where
        // the visit already COMPLETED (entry + exit in the redelivered batch) and would
        // otherwise be recorded twice.
        var duplicate = await context.GeofenceEvents.AnyAsync(e =>
            e.TransporterId == geofenceEvent.TransporterId
            && e.GeofenceId == geofenceEvent.GeofenceId
            && e.EventDateTime == geofenceEvent.EventDateTime, cancellationToken);
        if (duplicate)
            return null;

        var evt = new GeofenceEvent(
            geofenceEvent.TransporterId,
            geofenceEvent.GeofenceId,
            geofenceEvent.AccountId,
            geofenceEvent.EventDateTime,
            geofenceEvent.Latitude,
            geofenceEvent.Longitude);

        await context.GeofenceEvents.AddAsync(evt, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return CastVm(evt);
    }

    public async Task<GeofenceEventVm> UpdateExitEventAsync(
        Guid geofenceEventId,
        DateTimeOffset departureTimestamp,
        CancellationToken cancellationToken)
    {
        var evt = await context.GeofenceEvents.FindAsync([geofenceEventId], cancellationToken)
            ?? throw new KeyNotFoundException($"GeofenceEvent with ID {geofenceEventId} not found.");

        context.GeofenceEvents.Attach(evt);

        evt.DepartureTimestamp = departureTimestamp;
        await context.SaveChangesAsync(cancellationToken);

        return CastVm(evt);
    }

    public async Task StampDwellAlertedAsync(
        Guid geofenceEventId,
        DateTimeOffset alertedAt,
        CancellationToken cancellationToken)
    {
        var evt = await context.GeofenceEvents.FindAsync([geofenceEventId], cancellationToken)
            ?? throw new KeyNotFoundException($"GeofenceEvent with ID {geofenceEventId} not found.");

        context.GeofenceEvents.Attach(evt);

        evt.DwellAlertedAt = alertedAt;
        await context.SaveChangesAsync(cancellationToken);
    }

    private static GeofenceEventVm CastVm(GeofenceEvent evt)
        => new(evt.GeofenceEventId,
            evt.TransporterId,
            evt.GeofenceId,
            evt.EventDateTime,
            evt.DepartureTimestamp,
            evt.Latitude,
            evt.Longitude);
}
