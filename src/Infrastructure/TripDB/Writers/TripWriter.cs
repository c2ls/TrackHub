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

using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.TripManagement.Infrastructure.TripDB.Events;
using TrackHub.TripManagement.Infrastructure.TripDB.Readers;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Writers;

/// <summary>Write side of the trip aggregate: CRUD plus the lifecycle transitions.</summary>
public sealed class TripWriter(IApplicationDbContext context, IUser user) : ITripWriter
{
    public async Task<TripVm> CreateTripAsync(TripDto trip, Guid accountId, CancellationToken cancellationToken)
    {
        await GuardUniqueCodeAsync(accountId, trip.Code, null, cancellationToken);
        await GuardUniqueExternalReferenceAsync(accountId, trip.ExternalReference, null, cancellationToken);

        var entity = new Trip
        {
            AccountId = accountId,
            Code = trip.Code,
            Status = TripStatuses.Created,
            TransporterId = trip.TransporterId,
            DriverId = trip.DriverId,
            ServiceOrderId = trip.ServiceOrderId,
            ExternalReference = trip.ExternalReference,
            CustomerName = trip.CustomerName,
            OriginName = trip.OriginName,
            OriginPoint = TripGeometryFactory.Point(trip.OriginLatitude, trip.OriginLongitude),
            PlannedStartAt = trip.PlannedStartAt,
            PlannedEndAt = trip.PlannedEndAt,
            Notes = trip.Notes,
            TollVehicleClass = trip.TollVehicleClass,
        };

        entity.AddDomainEvent(new TripDomainEvent(TripEventTypes.TripCreated, accountId, entity.TripId));
        await context.Trips.AddAsync(entity, cancellationToken);
        AddAuditEvent(accountId, "CreateTrip", entity.TripId);
        await context.SaveChangesAsync(cancellationToken);

        return TripMapper.ToVm(entity, 0);
    }

    public async Task UpdateTripAsync(Guid tripId, TripDto trip, Guid accountId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(tripId, accountId, cancellationToken);

        // A terminal trip is history: it is cancelled or completed and must not be re-edited.
        if (TripStatuses.IsTerminal(entity.Status))
        {
            throw ConflictException.WithCode(TripErrorCodes.TripAlreadyTerminal);
        }

        await GuardUniqueCodeAsync(accountId, trip.Code, tripId, cancellationToken);
        await GuardUniqueExternalReferenceAsync(accountId, trip.ExternalReference, tripId, cancellationToken);

        context.Trips.Attach(entity);

        entity.Code = trip.Code;
        entity.TransporterId = trip.TransporterId;
        entity.DriverId = trip.DriverId;
        entity.ServiceOrderId = trip.ServiceOrderId;
        entity.ExternalReference = trip.ExternalReference;
        entity.CustomerName = trip.CustomerName;
        entity.OriginName = trip.OriginName;
        entity.OriginPoint = TripGeometryFactory.Point(trip.OriginLatitude, trip.OriginLongitude);
        entity.PlannedStartAt = trip.PlannedStartAt;
        entity.PlannedEndAt = trip.PlannedEndAt;
        entity.Notes = trip.Notes;
        entity.TollVehicleClass = trip.TollVehicleClass;

        entity.AddDomainEvent(new TripDomainEvent(TripEventTypes.TripUpdated, accountId, entity.TripId));
        AddAuditEvent(accountId, "UpdateTrip", entity.TripId);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTripAsync(Guid tripId, Guid accountId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(tripId, accountId, cancellationToken);

        // Trip history is permanent (spec 11 section 5, acceptance 16): only a Created trip that
        // never produced an event may be deleted. Anything else must be cancelled instead, so
        // stops, events, POD and documents are never orphaned.
        //
        // Job-sourced events are excluded for the same reason as in TripEventWriter.HasEventsAsync:
        // a trip-schedule-reminder orphans nothing, so counting it would block deletion of a trip
        // that never actually ran.
        var hasHistory = !string.Equals(entity.Status, TripStatuses.Created, StringComparison.Ordinal)
            || await context.TripEvents.AnyAsync(
                e => e.TripId == tripId && e.Source != TripEventSources.Job, cancellationToken);

        if (hasHistory)
        {
            throw ConflictException.WithCode(TripErrorCodes.TripHasHistory);
        }

        context.Trips.Attach(entity);
        AddAuditEvent(accountId, "DeleteTrip", entity.TripId);
        context.Trips.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task TransitionTripAsync(Guid tripId, Guid accountId, string toStatus, string? reason, bool force, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(tripId, accountId, cancellationToken);

        if (!TripStatuses.CanTransition(entity.Status, toStatus))
        {
            throw ConflictException.WithCode(TripErrorCodes.InvalidTransition);
        }

        var startingNow = string.Equals(toStatus, TripStatuses.InProgress, StringComparison.Ordinal)
            && string.Equals(entity.Status, TripStatuses.Created, StringComparison.Ordinal);

        if (string.Equals(toStatus, TripStatuses.Completed, StringComparison.Ordinal) && !force)
        {
            var openStops = await context.TripStops.AnyAsync(
                s => s.TripId == tripId
                    && s.Status != TripStopStatuses.Departed
                    && s.Status != TripStopStatuses.Skipped,
                cancellationToken);

            if (openStops)
            {
                throw ConflictException.WithCode(TripErrorCodes.StopsNotComplete);
            }
        }

        context.Trips.Attach(entity);

        if (startingNow)
        {
            await SnapshotArrivalGeometryAsync(entity, cancellationToken);
            entity.ActualStartAt ??= DateTimeOffset.UtcNow;
        }

        if (TripStatuses.IsTerminal(toStatus))
        {
            entity.ActualEndAt ??= DateTimeOffset.UtcNow;
        }

        // The reason is recorded on the audit row and the timeline event for EVERY transition, but
        // it only becomes a CANCELLATION reason when the trip is actually cancelled or aborted.
        // Forced completion passes a reason too, and stamping it here labelled a completed trip
        // with a cancellation it never had.
        if (!string.IsNullOrWhiteSpace(reason)
            && (string.Equals(toStatus, TripStatuses.Cancelled, StringComparison.Ordinal)
                || string.Equals(toStatus, TripStatuses.Aborted, StringComparison.Ordinal)))
        {
            entity.CancellationReason = reason;
        }

        entity.Status = toStatus;
        entity.AddDomainEvent(new TripDomainEvent(EventTypeFor(toStatus, startingNow), accountId, entity.TripId));
        AddAuditEvent(accountId, $"TransitionTrip:{toStatus}", entity.TripId, reason);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TripAssignmentVm> AssignTripAsync(Guid tripId, Guid accountId, Guid driverId, Guid? transporterId, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(tripId, accountId, cancellationToken);

        if (TripStatuses.IsTerminal(entity.Status))
        {
            throw ConflictException.WithCode(TripErrorCodes.TripAlreadyTerminal);
        }

        var now = DateTimeOffset.UtcNow;

        // Exactly one Active assignment per trip: the prior one is ENDED, never deleted, so the
        // handover history survives.
        var current = await context.TripAssignments
            .Where(a => a.TripId == tripId && a.Status == TripAssignmentStatuses.Active)
            .ToListAsync(cancellationToken);

        foreach (var previous in current)
        {
            context.TripAssignments.Attach(previous);
            previous.Status = TripAssignmentStatuses.Ended;
            previous.EndedAt = now;
        }

        var assignment = new TripAssignment
        {
            AccountId = accountId,
            TripId = tripId,
            DriverId = driverId,
            TransporterId = transporterId ?? entity.TransporterId,
            Status = TripAssignmentStatuses.Active,
            AssignedAt = now,
        };

        context.Trips.Attach(entity);
        entity.DriverId = driverId;
        if (transporterId is { } newTransporterId)
        {
            entity.TransporterId = newTransporterId;
        }

        assignment.AddDomainEvent(new TripDomainEvent(
            TripEventTypes.TripAssigned, accountId, tripId, assignment.TripAssignmentId));

        await context.TripAssignments.AddAsync(assignment, cancellationToken);
        AddAuditEvent(accountId, "AssignTrip", tripId);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (UniqueViolation.Matches(exception, "ux_trip_assignments_active_per_trip"))
        {
            // ux_trip_assignments_active_per_trip: a concurrent assign already won.
            throw ConflictException.WithCode(TripErrorCodes.DriverNotAssignable);
        }

        return TripMapper.ToVm(assignment);
    }

    public async Task<bool> UpdateTripProgressAsync(
        Guid tripId,
        Guid accountId,
        double latitude,
        double longitude,
        DateTimeOffset positionAt,
        double addedDistanceMeters,
        CancellationToken cancellationToken)
    {
        var entity = await context.Trips
            .FirstOrDefaultAsync(t => t.TripId == tripId && t.AccountId == accountId, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        // Out-of-order or replayed fixes never rewind the odometer or the last-seen point.
        //
        // The rejection is REPORTED to the caller rather than swallowed. Detection must skip the
        // same fixes this guard skips: arrival is protected by its idempotency key, but the
        // deviation run length is a plain counter, so one genuinely out-of-corridor fix redelivered
        // three times (a client retry, or the WithRetry policy after a timeout that had already
        // committed) would reach the threshold and open a false episode with a real alert.
        if (entity.LastPositionAt is { } last && positionAt <= last)
        {
            return false;
        }

        context.Trips.Attach(entity);
        entity.LastPoint = TripGeometryFactory.Point(latitude, longitude);
        entity.LastPositionAt = positionAt;
        entity.ActualDistanceMeters += Math.Max(addedDistanceMeters, 0d);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task SetDeviationStateAsync(
        Guid tripId,
        Guid accountId,
        DateTimeOffset? deviationOpenedAt,
        int consecutiveOutsideFixes,
        CancellationToken cancellationToken)
    {
        var entity = await context.Trips
            .FirstOrDefaultAsync(t => t.TripId == tripId && t.AccountId == accountId, cancellationToken);

        if (entity is null)
        {
            return;
        }

        // Nothing to write when the state is already where the caller wants it - detection touches
        // this on every out-of-corridor fix, and an UPDATE per fix per trip is pure noise.
        if (entity.DeviationOpenedAt == deviationOpenedAt && entity.ConsecutiveOutsideFixes == consecutiveOutsideFixes)
        {
            return;
        }

        context.Trips.Attach(entity);

        // Deliberately a plain assignment, not `??=`: an episode must be able to CLOSE on re-entry
        // so a later departure opens a new one (acceptance 14). The one-shot guarantee lives in the
        // caller, which stamps only after a successful emission and only while nothing is open.
        entity.DeviationOpenedAt = deviationOpenedAt;
        entity.ConsecutiveOutsideFixes = consecutiveOutsideFixes;

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Freezes each stop's arrival geometry at the moment the trip starts (spec 11 section 18.4):
    /// the linked geofence's polygon when <c>GeofenceId</c> is set, otherwise the stop point
    /// buffered by its radius. Reading the geofence ONCE, here, is exactly what makes a running
    /// trip immune to a geofence being edited mid-execution.
    /// </summary>
    private async Task SnapshotArrivalGeometryAsync(Trip trip, CancellationToken cancellationToken)
    {
        var stops = await context.TripStops
            .Where(s => s.TripId == trip.TripId && s.AccountId == trip.AccountId)
            .ToListAsync(cancellationToken);

        if (stops.Count == 0)
        {
            return;
        }

        var geofenceIds = stops.Where(s => s.GeofenceId.HasValue)
            .Select(s => s.GeofenceId!.Value)
            .Distinct()
            .ToList();

        var geofences = geofenceIds.Count == 0
            ? []
            : await context.Geofences
                .Where(g => geofenceIds.Contains(g.GeofenceId) && g.AccountId == trip.AccountId)
                .ToDictionaryAsync(g => g.GeofenceId, g => g.Geom, cancellationToken);

        foreach (var stop in stops)
        {
            context.TripStops.Attach(stop);

            stop.ArrivalGeom = stop.GeofenceId is { } geofenceId && geofences.TryGetValue(geofenceId, out var geom)
                ? geom
                : TripGeometryFactory.Buffer(stop.Point, stop.ArrivalRadiusMeters);
        }
    }

    private static string EventTypeFor(string toStatus, bool startingNow) => toStatus switch
    {
        TripStatuses.InProgress => startingNow ? TripEventTypes.TripStarted : TripEventTypes.TripResumed,
        TripStatuses.Paused => TripEventTypes.TripPaused,
        TripStatuses.Completed => TripEventTypes.TripCompleted,
        TripStatuses.Cancelled => TripEventTypes.TripCancelled,
        TripStatuses.Aborted => TripEventTypes.TripAborted,
        _ => TripEventTypes.TripUpdated,
    };

    private async Task<Trip> FindAsync(Guid tripId, Guid accountId, CancellationToken cancellationToken)
        => await context.Trips.FirstOrDefaultAsync(t => t.TripId == tripId && t.AccountId == accountId, cancellationToken)
            ?? throw new NotFoundException($"{tripId}", nameof(Trip));

    private async Task GuardUniqueCodeAsync(Guid accountId, string code, Guid? excludeTripId, CancellationToken cancellationToken)
    {
        var duplicate = await context.Trips.AnyAsync(
            t => t.AccountId == accountId && t.Code == code && (excludeTripId == null || t.TripId != excludeTripId),
            cancellationToken);

        if (duplicate)
        {
            throw ConflictException.WithCode(TripErrorCodes.DuplicateTripCode);
        }
    }

    private async Task GuardUniqueExternalReferenceAsync(Guid accountId, string? externalReference, Guid? excludeTripId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(externalReference))
        {
            return;
        }

        var duplicate = await context.Trips.AnyAsync(
            t => t.AccountId == accountId
                && t.ExternalReference == externalReference
                && (excludeTripId == null || t.TripId != excludeTripId),
            cancellationToken);

        if (duplicate)
        {
            throw ConflictException.WithCode(TripErrorCodes.DuplicateExternalReference);
        }
    }

    private void AddAuditEvent(Guid accountId, string action, Guid tripId, string? reason = null)
        => context.AuditEvents.Add(new AuditEvent(
            accountId,
            user.PrincipalType.ToString(),
            user.UserId?.ToString() ?? user.ClientId ?? user.SubjectId ?? string.Empty,
            action,
            TripSharing.ResourceType,
            tripId.ToString(),
            "Success",
            null,
            null,
            reason,
            null,
            null,
            user.CorrelationId));
}
