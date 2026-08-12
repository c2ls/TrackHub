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

/// <summary>
/// The single idempotent event log. Manual overrides and automatic detections both land here,
/// discriminated by <c>Source</c> — field reality (weak GPS, indoor docks, devices off) means
/// detection is an assist, not the record of truth (spec 11 §18.13).
/// </summary>
public interface ITripEventWriter
{
    /// <summary>
    /// Appends an event unless <paramref name="idempotencyKey"/> already exists.
    /// Returns false when the event was a duplicate, so callers can skip the side effects
    /// (alert emission, status change) without treating the retry as an error.
    /// </summary>
    Task<bool> AppendAsync(
        Guid accountId,
        Guid tripId,
        Guid? tripStopId,
        string eventType,
        DateTimeOffset occurredAt,
        string source,
        string? payloadJson,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<bool> HasEventsAsync(Guid tripId, Guid accountId, CancellationToken cancellationToken);

    /// <summary>
    /// True when <paramref name="idempotencyKey"/> has already been appended.
    /// <para>
    /// For the once-only side effects that must be emitted BEFORE the event is written. Appending
    /// first burns the key even when the emission then fails, so the alert is never retried and the
    /// event is raised zero times instead of once — the ordering the delay and deviation paths
    /// already get right by stamping their marker only after a successful emission.
    /// </para>
    /// </summary>
    Task<bool> HasEventAsync(Guid accountId, string idempotencyKey, CancellationToken cancellationToken);
}
