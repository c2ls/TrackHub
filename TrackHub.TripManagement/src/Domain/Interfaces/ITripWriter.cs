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

namespace TrackHub.TripManagement.Domain.Interfaces;

/// <summary>Write side of the trip aggregate: CRUD plus the lifecycle transitions.</summary>
public interface ITripWriter
{
    Task<TripVm> CreateTripAsync(TripDto trip, Guid accountId, CancellationToken cancellationToken);

    Task UpdateTripAsync(Guid tripId, TripDto trip, Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    /// Permitted only for a <c>Created</c> trip with no <c>TripEvent</c> rows; anything else is a
    /// conflict and the caller is told to cancel instead (spec 11 §5, acceptance 16).
    /// </summary>
    Task DeleteTripAsync(Guid tripId, Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    /// Applies a lifecycle transition after validating it against the matrix, stamping
    /// <c>ActualStartAt</c>/<c>ActualEndAt</c> and — on Start — snapshotting each stop's
    /// <c>ArrivalGeom</c> so a mid-flight geofence edit cannot move a running trip's geometry.
    /// </summary>
    Task TransitionTripAsync(Guid tripId, Guid accountId, string toStatus, string? reason, bool force, CancellationToken cancellationToken);

    Task<TripAssignmentVm> AssignTripAsync(Guid tripId, Guid accountId, Guid driverId, Guid? transporterId, CancellationToken cancellationToken);

    /// <summary>
    /// Records the running odometer and last-seen point from the position feed.
    /// <para>
    /// Returns <c>false</c> when the fix was REJECTED as out-of-order or replayed (its timestamp is
    /// not newer than the trip's <c>LastPositionAt</c>), and the caller must then skip detection for
    /// that fix too. Detection has no independent staleness guard: arrival is covered by its
    /// idempotency key, but the deviation run length is a plain counter that a redelivered fix would
    /// advance three times into a false episode (spec 11 §7.4).
    /// </para>
    /// </summary>
    Task<bool> UpdateTripProgressAsync(Guid tripId, Guid accountId, double latitude, double longitude, DateTimeOffset positionAt, double addedDistanceMeters, CancellationToken cancellationToken);

    /// <summary>
    /// Persists the corridor-deviation detection state.
    /// <para>
    /// Both values MUST survive across position batches and process restarts: Router pushes exactly
    /// one position per transporter per call, so a run length held only in memory is discarded
    /// before it can ever reach the three-fix threshold (spec 11 section 7.4).
    /// </para>
    /// <para>
    /// <paramref name="deviationOpenedAt"/> is stamped only AFTER the alert was successfully emitted
    /// (the geofence dwell precedent) and is cleared on re-entry so a later departure opens a NEW
    /// episode. Because it is persisted, it is also the episode's identity: the
    /// <c>TripEvent</c> idempotency key derives from it, so one episode mints exactly one key.
    /// </para>
    /// </summary>
    Task SetDeviationStateAsync(Guid tripId, Guid accountId, DateTimeOffset? deviationOpenedAt, int consecutiveOutsideFixes, CancellationToken cancellationToken);
}
