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

namespace TrackHub.TripManagement.Application.Trips.Commands.Lifecycle;

/// <summary>
/// The one place a lifecycle transition is applied. Every lifecycle command funnels through here
/// so the transition matrix (<see cref="TripStatuses.CanTransition"/>) is consulted exactly once,
/// in one implementation: an illegal transition returns a validation error carrying
/// <see cref="TripErrorCodes.InvalidTransition"/> and changes nothing (acceptance 11).
/// </summary>
public static class TripLifecycleTransition
{
    /// <summary>
    /// Validates the transition, applies it, appends the timeline event, then emits the alert.
    /// Alert emission is best-effort and isolated — a Manager outage must never roll back a
    /// transition the operator already saw succeed.
    /// </summary>
    public static async Task<TripVm> ExecuteAsync(
        ITripReader reader,
        ITripWriter writer,
        ITripEventWriter tripEventWriter,
        IAlertEmitter alertEmitter,
        ILogger logger,
        Guid tripId,
        Guid accountId,
        Guid? scopeUserId,
        string toStatus,
        string eventType,
        string? alertSeverity,
        string? reason,
        bool force,
        string idempotencySuffix,
        CancellationToken cancellationToken)
    {
        // The scope travels with the id: all six lifecycle verbs run through here, so a
        // group-scoped dispatcher must not be able to start, complete, cancel or abort a trip that
        // belongs to another group in the same account.
        var trip = await reader.GetTripAsync(tripId, accountId, scopeUserId, cancellationToken);

        if (!TripStatuses.CanTransition(trip.Status, toStatus))
        {
            throw TripValidationFailure.Create(
                nameof(TripVm.Status),
                TripStatuses.IsTerminal(trip.Status) ? TripErrorCodes.TripAlreadyTerminal : TripErrorCodes.InvalidTransition);
        }

        await writer.TransitionTripAsync(tripId, accountId, toStatus, reason, force, cancellationToken);

        var occurredAt = DateTimeOffset.UtcNow;
        await tripEventWriter.AppendAsync(
            accountId,
            tripId,
            null,
            eventType,
            occurredAt,
            TripEventSources.Portal,
            reason is null ? null : $$"""{"reason":"{{reason}}","forced":{{(force ? "true" : "false")}}}""",
            idempotencySuffix,
            cancellationToken);

        if (alertSeverity is not null)
        {
            try
            {
                await alertEmitter.EmitAsync(
                    eventType,
                    alertSeverity,
                    $"trip-{eventType.ToLowerInvariant()}:{tripId:N}",
                    new TripAlertDto(accountId, tripId, null, trip.Code, trip.TransporterId, trip.DriverId, null, occurredAt, null, null, null, null, null),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to emit {EventType} alert for trip {TripId}", eventType, tripId);
            }
        }

        return trip;
    }
}
