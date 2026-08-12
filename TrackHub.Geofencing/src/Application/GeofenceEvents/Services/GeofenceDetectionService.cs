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

using Microsoft.Extensions.Logging;
using TrackHub.Geofencing.Application.GeofenceEvents.Services.Interfaces;

namespace TrackHub.Geofencing.Application.GeofenceEvents.Services;

/// <summary>
/// Service that processes transporter positions to detect geofence entry/exit events.
/// </summary>
public class GeofenceDetectionService(
    IGeofenceReader geofenceReader,
    IGeofenceEventReader geofenceEventReader,
    IGeofenceEventWriter geofenceEventWriter,
    IAlertEmitter alertEmitter,
    ILogger<GeofenceDetectionService> logger) : IGeofenceDetectionService
{
    private static readonly TimeSpan MinEventInterval = TimeSpan.FromSeconds(30);

    public async Task<GeofenceProcessingResultVm> ProcessPositionsAsync(
        IEnumerable<TransporterPositionDto> positions,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var positionsList = positions
            .OrderBy(p => p.TransporterId)
            .ThenBy(p => p.DeviceDateTime)
            .ToList();

        if (positionsList.Count == 0)
            return new GeofenceProcessingResultVm(0, 0, 0);

        // Pre-load open events for all transporters
        var transporterIds = positionsList.Select(p => p.TransporterId).Distinct();
        var openEventsByTransporter = new Dictionary<Guid, Dictionary<Guid, GeofenceEventVm>>();

        foreach (var transporterId in transporterIds)
        {
            var openEvents = await geofenceEventReader.GetOpenEventsForTransporterAsync(transporterId, accountId, cancellationToken);
            openEventsByTransporter[transporterId] = openEvents.ToDictionary(e => e.GeofenceId);
        }

        // Process positions and accumulate results
        var eventsCreated = 0;
        var eventsUpdated = 0;
        var entries = new List<GeofenceEventVm>();
        var exits = new List<GeofenceEventVm>();

        foreach (var transporterPositions in positionsList.GroupBy(p => p.TransporterId))
        {
            var transporterId = transporterPositions.Key;
            var openEvents = openEventsByTransporter[transporterId];

            foreach (var position in transporterPositions)
            {
                var (created, updated) = await ProcessPositionAsync(
                    position, accountId, openEvents, entries, exits, cancellationToken);

                eventsCreated += created;
                eventsUpdated += updated;
            }
        }

        // Post-commit, best-effort alert emission: failures are logged and never
        // fail position processing; Manager-side AlertEvent dedup makes retries safe.
        if (entries.Count > 0 || exits.Count > 0)
            await EmitAlertsAsync(accountId, entries, exits, cancellationToken);

        return new GeofenceProcessingResultVm(
            positionsList.Count,
            eventsCreated,
            eventsUpdated);
    }

    private async Task<(int Created, int Updated)> ProcessPositionAsync(
        TransporterPositionDto position,
        Guid accountId,
        Dictionary<Guid, GeofenceEventVm> openEvents,
        List<GeofenceEventVm> entries,
        List<GeofenceEventVm> exits,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var updated = 0;

        // Use spatial query to find geofences containing this point (uses PostGIS ST_Contains)
        var containingGeofenceIds = await geofenceReader.GetGeofenceIdsContainingPointAsync(
            accountId, position.Latitude, position.Longitude, cancellationToken);
        var containingGeofences = containingGeofenceIds.ToHashSet();

        // Process entries (in geofence but no open event)
        foreach (var geofenceId in containingGeofences)
        {
            if (openEvents.ContainsKey(geofenceId))
                continue;

            // Create entry event; null = the visit already exists (redelivered batch) and
            // must not be counted, tracked, or re-alerted.
            var newEvent = await geofenceEventWriter.CreateEntryEventAsync(
                new GeofenceEventDto(
                    position.TransporterId,
                    geofenceId,
                    accountId,
                    position.DeviceDateTime,
                    position.Latitude,
                    position.Longitude),
                cancellationToken);
            if (newEvent is not { } createdEvent)
                continue;

            created++;
            entries.Add(createdEvent);
            openEvents[geofenceId] = createdEvent;
        }

        // Process exits (has open event but not in geofence)
        var geofencesToClose = openEvents.Keys
            .Where(id => !containingGeofences.Contains(id))
            .ToList();

        foreach (var geofenceId in geofencesToClose)
        {
            var openEvent = openEvents[geofenceId];

            // Debounce check
            if ((position.DeviceDateTime - openEvent.Timestamp) < MinEventInterval)
                continue;

            var closedEvent = await geofenceEventWriter.UpdateExitEventAsync(
                openEvent.GeofenceEventId,
                position.DeviceDateTime,
                cancellationToken);

            updated++;
            exits.Add(closedEvent);
            openEvents.Remove(geofenceId);
        }

        return (created, updated);
    }

    private async Task EmitAlertsAsync(
        Guid accountId,
        IReadOnlyCollection<GeofenceEventVm> entries,
        IReadOnlyCollection<GeofenceEventVm> exits,
        CancellationToken cancellationToken)
    {
        IReadOnlyDictionary<Guid, GeofenceAlertInfoVm> alertInfo;
        try
        {
            alertInfo = await geofenceReader.GetActiveGeofenceAlertInfoAsync(accountId, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load geofence alert metadata for account {AccountId}; skipping alert emission for this batch", accountId);
            return;
        }

        foreach (var entry in entries)
        {
            if (!alertInfo.TryGetValue(entry.GeofenceId, out var info) || !info.AlertOnEntry)
                continue;

            try
            {
                await alertEmitter.EmitGeofenceEnteredAsync(ToAlertDto(accountId, entry, info, dwellSeconds: null), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to emit GeofenceEntered alert for event {GeofenceEventId}", entry.GeofenceEventId);
            }
        }

        foreach (var exit in exits)
        {
            if (!alertInfo.TryGetValue(exit.GeofenceId, out var info) || !info.AlertOnExit)
                continue;

            try
            {
                var dwellSeconds = exit.DepartureTimestamp is null
                    ? (long?)null
                    : (long)(exit.DepartureTimestamp.Value - exit.Timestamp).TotalSeconds;
                await alertEmitter.EmitGeofenceExitedAsync(ToAlertDto(accountId, exit, info, dwellSeconds), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to emit GeofenceExited alert for event {GeofenceEventId}", exit.GeofenceEventId);
            }
        }
    }

    private static GeofenceAlertDto ToAlertDto(
        Guid accountId,
        GeofenceEventVm evt,
        GeofenceAlertInfoVm info,
        long? dwellSeconds)
        => new(evt.GeofenceEventId,
            accountId,
            evt.TransporterId,
            evt.GeofenceId,
            info.Name,
            info.Type,
            evt.Timestamp,
            evt.DepartureTimestamp,
            dwellSeconds,
            evt.Latitude,
            evt.Longitude);
}
