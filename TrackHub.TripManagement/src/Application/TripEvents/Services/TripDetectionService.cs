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
using TrackHub.TripManagement.Application.Common;
using TrackHub.TripManagement.Application.TripEvents.Services.Interfaces;

namespace TrackHub.TripManagement.Application.TripEvents.Services;

/// <summary>
/// Trip-scoped arrival/departure/deviation detection over the pushed position feed.
/// <para>
/// Detection is an <b>assist, not the record of truth</b> (spec 11 §18.13): weak GPS, indoor docks
/// and devices that were off are field reality, so a manual override always wins and both land in
/// the same idempotent event log discriminated by <c>Source</c>.
/// </para>
/// <para>
/// Alert emission and job recording are best-effort and isolated — a Manager outage logs and never
/// fails position processing, because the Router batch that fed us must not flip to FAILED over a
/// downstream notification problem.
/// </para>
/// <para>
/// <b>All state that spans fixes is PERSISTED</b> (<c>TripStop.OutsideSinceAt</c>,
/// <c>Trip.ConsecutiveOutsideFixes</c>, <c>Trip.DeviationOpenedAt</c>). Router calls this with
/// exactly one position per transporter, so the debounce clock and the deviation run length must
/// survive the request — held in memory they were rebuilt from scratch every call, and neither the
/// 30 s departure window nor the three-fix deviation threshold could ever be reached.
/// <see cref="TripDetectionState"/> remains a within-batch cache in front of those columns.
/// </para>
/// </summary>
public sealed class TripDetectionService(
    ITripDetectionReader detectionReader,
    ITripWriter tripWriter,
    ITripStopWriter stopWriter,
    ITripEventWriter tripEventWriter,
    IAlertEmitter alertEmitter,
    ILogger<TripDetectionService> logger) : ITripDetectionService
{
    /// <summary>Same debounce as geofence exit: a stop is not "departed" until the vehicle has been outside for this long.</summary>
    private static readonly TimeSpan DepartureDebounce = TimeSpan.FromSeconds(30);

    /// <summary>Consecutive out-of-corridor fixes before an episode opens. Three, so one bad fix is not a deviation.</summary>
    private const int DeviationFixThreshold = 3;

    public async Task<TripProcessingResultVm> ProcessPositionsAsync(
        IEnumerable<TransporterPositionDto> positions,
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var ordered = positions
            .OrderBy(p => p.TransporterId)
            .ThenBy(p => p.DeviceDateTime)
            .ToList();

        if (ordered.Count == 0)
            return new TripProcessingResultVm(0, 0, 0, 0);

        var transporterIds = ordered.Select(p => p.TransporterId).Distinct().ToList();
        var openTrips = await detectionReader.GetOpenTripsAsync(accountId, transporterIds, cancellationToken);
        if (openTrips.Count == 0)
            return new TripProcessingResultVm(ordered.Count, 0, 0, 0);

        var states = openTrips.ToDictionary(t => t.TripId, t => new TripDetectionState(t));
        var tripsByTransporter = openTrips
            .GroupBy(t => t.TransporterId)
            .ToDictionary(g => g.Key, g => g.Select(t => t.TripId).ToList());

        var arrived = 0;
        var departed = 0;
        var deviations = 0;

        foreach (var position in ordered)
        {
            if (!tripsByTransporter.TryGetValue(position.TransporterId, out var tripIds))
                continue;

            foreach (var tripId in tripIds)
            {
                var state = states[tripId];

                // A rejected fix is out-of-order or a replay. Detection is skipped entirely for it
                // — see ITripWriter.UpdateTripProgressAsync: without this, a redelivered
                // out-of-corridor fix advances the deviation counter every time it arrives.
                if (!await UpdateProgressAsync(state, accountId, position, cancellationToken))
                    continue;

                arrived += await DetectArrivalsAsync(state, accountId, position, cancellationToken);
                departed += await DetectDeparturesAsync(state, accountId, position, cancellationToken);
                deviations += await DetectDeviationAsync(state, accountId, position, cancellationToken);
            }
        }

        return new TripProcessingResultVm(ordered.Count, arrived, departed, deviations);
    }

    // 1. Odometer and last-seen point. Distance accumulates from the previous fix, so a trip's
    //    actual distance is measured, never inferred from the plan.
    private async Task<bool> UpdateProgressAsync(
        TripDetectionState state, Guid accountId, TransporterPositionDto position, CancellationToken cancellationToken)
    {
        var added = 0d;
        if (state.LastLatitude is { } lastLat && state.LastLongitude is { } lastLng)
            added = GeoDistance.HaversineMeters(lastLat, lastLng, position.Latitude, position.Longitude);

        var accepted = await tripWriter.UpdateTripProgressAsync(
            state.TripId, accountId, position.Latitude, position.Longitude, position.DeviceDateTime, added, cancellationToken);

        if (!accepted)
            return false;

        state.LastLatitude = position.Latitude;
        state.LastLongitude = position.Longitude;

        return true;
    }

    // 2. Arrival. Containment is evaluated in the database against the snapshotted ArrivalGeom.
    //    Every Pending stop containing the point is considered, not just the lowest-sequence one,
    //    so an out-of-order arrival is RECORDED rather than lost — real routes get resequenced by
    //    traffic and a dispatcher would rather see the truth than a tidy fiction.
    private async Task<int> DetectArrivalsAsync(
        TripDetectionState state, Guid accountId, TransporterPositionDto position, CancellationToken cancellationToken)
    {
        var containing = await detectionReader.GetStopsContainingPointAsync(
            accountId, state.TripId, position.Latitude, position.Longitude, cancellationToken);

        state.ContainingStops = containing.ToHashSet();
        if (containing.Count == 0)
            return 0;

        var count = 0;
        foreach (var stop in state.Stops.Where(s => string.Equals(s.Status, TripStopStatuses.Pending, StringComparison.Ordinal)).ToList())
        {
            if (!state.ContainingStops.Contains(stop.TripStopId))
                continue;

            // Keyed WITHOUT a client event id: detection may see the same stop across many
            // batches, and exactly one arrival must ever be recorded (acceptance 13).
            var recorded = await stopWriter.RecordStopProgressAsync(
                state.TripId, stop.TripStopId, accountId, TripStopStatuses.Arrived, position.DeviceDateTime,
                position.Latitude, position.Longitude, TripEventSources.Detection,
                $"trip-arrive:{stop.TripStopId:N}", null, cancellationToken);

            if (!recorded)
                continue;

            state.MarkStopStatus(stop.TripStopId, TripStopStatuses.Arrived);

            // The writer clears the persisted debounce clock on arrival; keep the cache in step.
            state.OutsideSince.Remove(stop.TripStopId);
            count++;
            await EmitStopAlertAsync(state, accountId, stop, TripEventTypes.TripStopArrived, position, cancellationToken);
        }

        return count;
    }

    // 3. Departure. An Arrived stop whose position has been outside its arrival geometry for the
    //    debounce window. Without the debounce a single fix bouncing off the edge of the polygon
    //    would close a stop the vehicle is still sitting in.
    //
    //    The clock is PERSISTED (TripStop.OutsideSinceAt). Router delivers one fix per call, so the
    //    comparison below is only ever reachable from a LATER call — with a per-request clock the
    //    first outside fix stored the instant and the second call reset it, so no stop ever
    //    departed and an auto-tracked trip could only be completed with `force`.
    private async Task<int> DetectDeparturesAsync(
        TripDetectionState state, Guid accountId, TransporterPositionDto position, CancellationToken cancellationToken)
    {
        var count = 0;
        foreach (var stop in state.Stops.Where(s => string.Equals(s.Status, TripStopStatuses.Arrived, StringComparison.Ordinal)).ToList())
        {
            if (state.ContainingStops.Contains(stop.TripStopId))
            {
                // Back inside: the debounce restarts from zero, in the database as well as here.
                if (state.OutsideSince.Remove(stop.TripStopId))
                {
                    await stopWriter.SetStopOutsideSinceAsync(stop.TripStopId, accountId, null, cancellationToken);
                }

                continue;
            }

            if (!state.OutsideSince.TryGetValue(stop.TripStopId, out var outsideSince))
            {
                state.OutsideSince[stop.TripStopId] = position.DeviceDateTime;
                await stopWriter.SetStopOutsideSinceAsync(stop.TripStopId, accountId, position.DeviceDateTime, cancellationToken);
                continue;
            }

            if (position.DeviceDateTime - outsideSince < DepartureDebounce)
                continue;

            var recorded = await stopWriter.RecordStopProgressAsync(
                state.TripId, stop.TripStopId, accountId, TripStopStatuses.Departed, position.DeviceDateTime,
                position.Latitude, position.Longitude, TripEventSources.Detection,
                $"trip-depart:{stop.TripStopId:N}", null, cancellationToken);

            if (!recorded)
                continue;

            state.MarkStopStatus(stop.TripStopId, TripStopStatuses.Departed);

            // The writer already cleared the persisted clock as part of recording the departure.
            state.OutsideSince.Remove(stop.TripStopId);
            count++;
            await EmitStopAlertAsync(state, accountId, stop, TripEventTypes.TripStopDeparted, position, cancellationToken);
        }

        return count;
    }

    // 4. Deviation. Three consecutive fixes outside the corridor open an episode; re-entry clears
    //    it so a later departure can open a new one (acceptance 14).
    //
    //    The run length is PERSISTED (Trip.ConsecutiveOutsideFixes). Router delivers one fix per
    //    call, so an in-memory counter went 0 → 1 and was thrown away every batch and the threshold
    //    of three was unreachable — corridor deviation was completely inert.
    private async Task<int> DetectDeviationAsync(
        TripDetectionState state, Guid accountId, TransporterPositionDto position, CancellationToken cancellationToken)
    {
        if (!state.HasReadyRoutePlan || state.RoutePlanId is not { } routePlanId)
            return 0;

        bool? inside;
        try
        {
            inside = await detectionReader.IsInsideCorridorAsync(accountId, routePlanId, position.Latitude, position.Longitude, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Corridor check failed for trip {TripId}; skipping deviation detection for this fix", state.TripId);
            return 0;
        }

        if (inside is null)
        {
            // No corridor to test against. NOT a deviation: counting it would climb to the
            // three-fix threshold on a vehicle driving the route perfectly, and re-entry could
            // never clear it because there is nothing to re-enter. Leave the run length untouched.
            logger.LogWarning(
                "Route plan {RoutePlanId} on trip {TripId} has no corridor geometry; deviation detection is skipped for this fix",
                routePlanId, state.TripId);
            return 0;
        }

        if (inside.Value)
        {
            // Re-entry closes the episode — in the database, so a LATER departure opens a new one
            // with a new episode start and therefore a new idempotency key (acceptance 14).
            if (state.ConsecutiveOutside != 0 || state.DeviationOpenedAt is not null)
            {
                state.ConsecutiveOutside = 0;
                state.DeviationOpenedAt = null;
                await tripWriter.SetDeviationStateAsync(state.TripId, accountId, null, 0, cancellationToken);
            }

            return 0;
        }

        state.ConsecutiveOutside++;

        if (state.ConsecutiveOutside < DeviationFixThreshold || state.DeviationOpenedAt is not null)
        {
            // Still short of the threshold, or the episode is already open: only the run length
            // moves, and it MUST be persisted or the next call starts counting from zero again.
            await tripWriter.SetDeviationStateAsync(
                state.TripId, accountId, state.DeviationOpenedAt, state.ConsecutiveOutside, cancellationToken);
            return 0;
        }

        // The episode's identity is the instant it OPENS, which is what gets persisted as
        // DeviationOpenedAt — so the idempotency key below is stable across batches and restarts.
        var episodeStart = position.DeviceDateTime;
        try
        {
            await alertEmitter.EmitAsync(
                TripEventTypes.TripRouteDeviation,
                TripAlertSeverities.Warning,
                $"trip-deviation:{state.TripId:N}",
                new TripAlertDto(accountId, state.TripId, null, state.Code, state.TransporterId, state.DriverId, null,
                    position.DeviceDateTime, null, null, null, position.Latitude, position.Longitude),
                cancellationToken);
        }
        catch (Exception ex)
        {
            // The geofence dwell precedent: DeviationOpenedAt is not stamped, so the episode is
            // retried on the next cycle rather than being lost to a transient Manager failure. The
            // run length is still persisted — the vehicle really is outside, and losing the count
            // would restart the three-fix climb from zero on every failed emission.
            logger.LogError(ex, "Failed to emit TripRouteDeviation alert for trip {TripId}; it will be retried", state.TripId);
            await tripWriter.SetDeviationStateAsync(state.TripId, accountId, null, state.ConsecutiveOutside, cancellationToken);
            return 0;
        }

        // Stamped ONLY after a successful emission, and persisted: it is both the "an episode is
        // open" flag every later batch reads and the episode key's source.
        state.DeviationOpenedAt = episodeStart;
        await tripWriter.SetDeviationStateAsync(
            state.TripId, accountId, episodeStart, state.ConsecutiveOutside, cancellationToken);

        await tripEventWriter.AppendAsync(
            accountId, state.TripId, null, TripEventTypes.TripRouteDeviation, position.DeviceDateTime,
            TripEventSources.Detection, null,
            $"trip-deviation:{state.TripId:N}:{episodeStart.UtcTicks}", cancellationToken);

        return 1;
    }

    private async Task EmitStopAlertAsync(
        TripDetectionState state, Guid accountId, OpenTripStopVm stop, string eventType,
        TransporterPositionDto position, CancellationToken cancellationToken)
    {
        try
        {
            await alertEmitter.EmitAsync(
                eventType,
                TripAlertSeverities.Info,
                $"trip-{eventType.ToLowerInvariant()}:{stop.TripStopId:N}",
                new TripAlertDto(accountId, state.TripId, stop.TripStopId, state.Code, state.TransporterId, state.DriverId,
                    stop.Name, position.DeviceDateTime, null, stop.PlannedArrivalTo, null, position.Latitude, position.Longitude),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to emit {EventType} alert for stop {TripStopId}", eventType, stop.TripStopId);
        }
    }
}
