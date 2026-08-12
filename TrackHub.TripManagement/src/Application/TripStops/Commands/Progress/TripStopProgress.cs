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

namespace TrackHub.TripManagement.Application.TripStops.Commands.Progress;

/// <summary>
/// The shared body of the three dispatcher-side progress overrides (arrive, depart, skip).
/// <para>
/// <b>Idempotency is server-side by design, not by caller discipline.</b> Every call builds
/// <c>TripEvent.IdempotencyKey = trip-{verb}:{tripStopId:N}:{clientEventId:N}</c>; a duplicate
/// submission returns success and writes no second row (acceptance 15). This is precisely what
/// makes spec 10's offline outbox safe to layer on later without reopening these handlers — the
/// server never assumes a client will refrain from retrying.
/// </para>
/// <para>
/// Manual override always beats automatic detection (spec 11 §18.13): both land in the same event
/// log, discriminated only by <c>Source</c>. Timestamps once written are never overwritten.
/// </para>
/// </summary>
public static class TripStopProgress
{
    public static async Task<bool> ExecuteAsync(
        ITripReader reader,
        ITripStopWriter stopWriter,
        IAlertEmitter alertEmitter,
        ILogger logger,
        Guid tripId,
        Guid tripStopId,
        Guid accountId,
        Guid? scopeUserId,
        string toStatus,
        string eventType,
        string? alertSeverity,
        DateTimeOffset occurredAt,
        double? latitude,
        double? longitude,
        string idempotencyKey,
        string? reason,
        CancellationToken cancellationToken)
    {
        var trip = await reader.GetTripAsync(tripId, accountId, scopeUserId, cancellationToken);

        if (!string.Equals(trip.Status, TripStatuses.InProgress, StringComparison.Ordinal))
            throw TripValidationFailure.Create(nameof(TripVm.Status), TripErrorCodes.TripNotActive);

        // The trip id travels to the writer so the stop is resolved by (stop, account, TRIP): the
        // active-trip check above and the row actually written must concern the SAME trip.
        var recorded = await stopWriter.RecordStopProgressAsync(
            tripId,
            tripStopId,
            accountId,
            toStatus,
            occurredAt,
            latitude,
            longitude,
            TripEventSources.Portal,
            idempotencyKey,
            reason,
            cancellationToken);

        // A duplicate is a success with no side effects: no second row and no second alert.
        //
        // `recorded` is the WRITER's contract ("did this insert"), which the detection service needs
        // so it can count arrivals and skip a second alert. It is NOT this mutation's contract.
        // Returning it verbatim answered `false` to a duplicate, which a client cannot tell from a
        // failure — spec 10's offline outbox would keep the event queued and retry it forever.
        // Acceptance 15 says a duplicate submission RETURNS SUCCESS, so the caller can drop it.
        if (!recorded)
            return true;

        if (alertSeverity is null)
            return true;

        try
        {
            await alertEmitter.EmitAsync(
                eventType,
                alertSeverity,
                $"trip-{eventType.ToLowerInvariant()}:{tripStopId:N}",
                new TripAlertDto(accountId, tripId, tripStopId, trip.Code, trip.TransporterId, trip.DriverId, null, occurredAt, null, null, null, latitude, longitude),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to emit {EventType} alert for stop {TripStopId}", eventType, tripStopId);
        }

        // Reached only when the event WAS recorded, so this is unconditionally a success.
        return true;
    }
}
