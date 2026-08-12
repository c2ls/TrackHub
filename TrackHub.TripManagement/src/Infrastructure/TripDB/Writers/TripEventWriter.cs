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

namespace TrackHub.TripManagement.Infrastructure.TripDB.Writers;

/// <summary>
/// The single idempotent event log. Manual overrides and automatic detections both land here,
/// discriminated by <c>Source</c> (spec 11 section 18.13).
/// </summary>
public sealed class TripEventWriter(IApplicationDbContext context) : ITripEventWriter
{
    public async Task<bool> AppendAsync(
        Guid accountId,
        Guid tripId,
        Guid? tripStopId,
        string eventType,
        DateTimeOffset occurredAt,
        string source,
        string? payloadJson,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var entity = new TripEvent
        {
            AccountId = accountId,
            TripId = tripId,
            TripStopId = tripStopId,
            EventType = eventType,
            OccurredAt = occurredAt,
            Source = source,
            PayloadJson = payloadJson,
            IdempotencyKey = idempotencyKey,
        };

        await context.TripEvents.AddAsync(entity, cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (UniqueViolation.Matches(exception, "ux_trip_events_idempotencykey"))
        {
            // A retry, not an error. The caller skips the side effects (alert emission, status
            // change) rather than treating the duplicate as a failure (acceptance 15).
            context.TripEvents.Entry(entity).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<bool> HasEventsAsync(Guid tripId, Guid accountId, CancellationToken cancellationToken)
        => await context.TripEvents.AnyAsync(
            e => e.TripId == tripId
                && e.AccountId == accountId
                // Job-sourced events do NOT count as execution history. The guard exists so a trip
                // that produced stops, POD or documents can never be deleted out from under them —
                // and a schedule reminder produces none of that. Counting it meant the
                // trip-schedule-reminder job silently made every mistaken trip undeletable an hour
                // before its planned start, leaving Cancel as the only exit and parking a bogus row
                // on the dispatch board and in reports (acceptance 16).
                && e.Source != TripEventSources.Job,
            cancellationToken);

    public async Task<bool> HasEventAsync(Guid accountId, string idempotencyKey, CancellationToken cancellationToken)
        => await context.TripEvents.AnyAsync(
            e => e.AccountId == accountId && e.IdempotencyKey == idempotencyKey, cancellationToken);
}
