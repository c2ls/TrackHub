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

using NetTopologySuite.Geometries;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Readers;

/// <summary>
/// The detection working set. Unlike Geofencing - which must consider an account's whole zone
/// catalog - trip detection only ever looks at <c>InProgress</c> trips (spec 11 section 18.4).
/// Every containment test is evaluated in PostGIS against the snapshotted geometry, never in
/// application code.
/// </summary>
public sealed class TripDetectionReader(IApplicationDbContext context) : ITripDetectionReader
{
    /// <summary>
    /// Defensive per-cycle cap. The in-flight set is naturally small, but a pathological backlog
    /// must not materialize unbounded (the Geofencing dwell-candidate precedent).
    /// </summary>
    private const int MaxTripsPerCycle = 1000;

    public async Task<IReadOnlyCollection<OpenTripVm>> GetOpenTripsAsync(
        Guid accountId,
        IReadOnlyCollection<Guid> transporterIds,
        CancellationToken cancellationToken)
    {
        if (transporterIds.Count == 0)
        {
            return [];
        }

        var ids = transporterIds.ToList();

        var trips = await context.Trips
            .Where(t => t.AccountId == accountId
                && t.Status == TripStatuses.InProgress
                && ids.Contains(t.TransporterId))
            .OrderBy(t => t.PlannedStartAt)
            .Take(MaxTripsPerCycle)
            .ToListAsync(cancellationToken);

        if (trips.Count == 0)
        {
            return [];
        }

        var tripIds = trips.ConvertAll(t => t.TripId);

        // Only stops detection may still act on: Departed and Skipped are closed.
        var stops = await context.TripStops
            .Where(s => tripIds.Contains(s.TripId)
                && (s.Status == TripStopStatuses.Pending || s.Status == TripStopStatuses.Arrived))
            .OrderBy(s => s.TripId)
            .ThenBy(s => s.Sequence)
            .ToListAsync(cancellationToken);

        var readyPlanTripIds = await context.RoutePlans
            .Where(p => tripIds.Contains(p.TripId) && p.Status == RoutePlanStatuses.Ready)
            .Select(p => p.TripId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return [.. trips.Select(t => new OpenTripVm(
            t.TripId,
            t.AccountId,
            t.Code,
            t.TransporterId,
            t.DriverId,
            t.RoutePlanId,
            readyPlanTripIds.Contains(t.TripId),
            t.DeviationOpenedAt,
            t.ConsecutiveOutsideFixes,
            t.ActualDistanceMeters,
            t.LastPoint?.Y,
            t.LastPoint?.X,
            t.LastPositionAt,
            [.. stops.Where(s => s.TripId == t.TripId).Select(s => new OpenTripStopVm(
                s.TripStopId,
                s.Sequence,
                s.Name,
                s.Status,
                s.ActualArrivalAt,
                s.PlannedArrivalTo,
                s.DelayAlertedAt,
                s.OutsideSinceAt,
                s.Point.Y,
                s.Point.X))]))];
    }

    public async Task<IReadOnlyCollection<Guid>> GetStopsContainingPointAsync(
        Guid accountId,
        Guid tripId,
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var point = NewPoint(latitude, longitude);

        // ST_Contains against the SNAPSHOT taken at trip start, so a geofence edited mid-trip
        // cannot move a running trip's arrival geometry (spec 11 section 18.4).
        return await context.TripStops
            .Where(s => s.AccountId == accountId
                && s.TripId == tripId
                && s.ArrivalGeom != null
                && s.ArrivalGeom.Contains(point))
            .OrderBy(s => s.Sequence)
            .Select(s => s.TripStopId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool?> IsInsideCorridorAsync(Guid accountId, Guid routePlanId, double latitude, double longitude, CancellationToken cancellationToken)
    {
        var point = NewPoint(latitude, longitude);

        // Two queries, not one AnyAsync: a single boolean cannot separate "the point is outside the
        // corridor" from "there is no corridor to test", and the caller must treat those
        // differently or it invents deviations on a plan that has no geometry.
        var match = await context.RoutePlans
            .Where(p => p.RoutePlanId == routePlanId
                && p.AccountId == accountId
                && p.CorridorGeom != null)
            .Select(p => (bool?)p.CorridorGeom!.Contains(point))
            .FirstOrDefaultAsync(cancellationToken);

        return match;
    }

    public async Task<IReadOnlyCollection<EtaCandidateVm>> GetEtaCandidatesAsync(Guid accountId, DateTimeOffset positionFreshnessCutoff, CancellationToken cancellationToken)
    {
        // Deliberately NOT filtered by position freshness. A trip whose tracker went dark still
        // needs its ETA moved off a stale Ors value onto the planned schedule; excluding it here
        // made the fallback path in TripEtaService unreachable (spec 11 §10, §18.11). Freshness is
        // computed below from LastPositionAt and travels as a flag - no column stores it.
        var rows = await context.Trips
            .Where(t => t.AccountId == accountId
                && t.Status == TripStatuses.InProgress
                && context.RoutePlans.Any(p => p.TripId == t.TripId && p.Status == RoutePlanStatuses.Ready))
            .OrderBy(t => t.PlannedStartAt)
            .Take(MaxTripsPerCycle)
            .Select(t => new
            {
                t.TripId,
                t.AccountId,
                t.Code,
                t.TransporterId,
                t.DriverId,
                t.LastPoint,
                t.LastPositionAt,
                NextStop = context.TripStops
                    .Where(s => s.TripId == t.TripId && s.Status == TripStopStatuses.Pending)
                    .OrderBy(s => s.Sequence)
                    .Select(s => new
                    {
                        s.TripStopId,
                        s.Name,
                        s.Point,
                        s.PlannedArrivalTo,
                        s.DelayAlertedAt,
                        s.EtaAt,
                        s.EtaSource,
                    })
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        return [.. rows
            .Where(r => r.NextStop is not null)
            .Select(r => new EtaCandidateVm(
                r.TripId,
                r.AccountId,
                r.Code,
                r.TransporterId,
                r.DriverId,
                r.LastPoint?.Y,
                r.LastPoint?.X,
                r.LastPositionAt,
                r.LastPoint is not null && r.LastPositionAt is { } at && at >= positionFreshnessCutoff,
                r.NextStop!.TripStopId,
                r.NextStop!.Name,
                r.NextStop!.Point.Y,
                r.NextStop!.Point.X,
                r.NextStop!.PlannedArrivalTo,
                r.NextStop!.DelayAlertedAt,
                r.NextStop!.EtaAt,
                r.NextStop!.EtaSource))];
    }

    public async Task<IReadOnlyCollection<TripVm>> GetTripsDueToStartAsync(Guid accountId, DateTimeOffset dueAfter, DateTimeOffset dueBefore, CancellationToken cancellationToken)
    {
        // Both bounds matter. With only the upper one, the window is "any time before now + lead",
        // so the first cycle after a deployment reminds about every trip ever created and never
        // started. The idempotency guard makes that a one-off burst rather than a loop, but a
        // one-off burst of alerts for last year's trips is still noise (spec 11 §10).
        var trips = await context.Trips
            .Where(t => t.AccountId == accountId
                && t.Status == TripStatuses.Created
                && t.PlannedStartAt >= dueAfter
                && t.PlannedStartAt <= dueBefore)
            .OrderBy(t => t.PlannedStartAt)
            .Take(MaxTripsPerCycle)
            .ToListAsync(cancellationToken);

        if (trips.Count == 0)
        {
            return [];
        }

        var tripIds = trips.ConvertAll(t => t.TripId);
        var counts = await context.TripStops
            .Where(s => tripIds.Contains(s.TripId))
            .GroupBy(s => s.TripId)
            .Select(g => new { TripId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var byTrip = counts.ToDictionary(c => c.TripId, c => c.Count);
        return [.. trips.Select(t => TripMapper.ToVm(t, byTrip.TryGetValue(t.TripId, out var count) ? count : 0))];
    }

    private static Point NewPoint(double latitude, double longitude)
        => new(longitude, latitude) { SRID = TripGeometryDefaults.Srid };
}
