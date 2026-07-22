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
using TrackHub.TripManagement.Application.Trips.Services.Interfaces;
using TrackHub.TripManagement.Domain.Exceptions;

namespace TrackHub.TripManagement.Application.Trips.Services;

/// <summary>
/// ETA refresh and schedule reminders.
/// <para>
/// ETA is <b>deterministic, not learned</b> (spec 11 §18.11): the latest position plus an ORS
/// distance/duration when both are available, the planned schedule otherwise, and the source is
/// recorded per stop so the UI can be honest about confidence instead of dressing a fallback up as
/// a live estimate.
/// </para>
/// </summary>
public sealed class TripEtaService(
    IAccountFeatureReader accountFeatureReader,
    ITripDetectionReader detectionReader,
    ITripStopWriter stopWriter,
    ITripEventWriter tripEventWriter,
    IRoutingProvider routingProvider,
    IAlertEmitter alertEmitter,
    ILogger<TripEtaService> logger) : ITripEtaService
{
    /// <summary>A position older than this is not evidence of where the vehicle is now.</summary>
    private static readonly TimeSpan PositionFreshness = TimeSpan.FromMinutes(15);

    /// <summary>
    /// How far BEFORE now a planned start still earns a reminder. The job runs every 15 minutes,
    /// so without a grace a trip whose start fell between two cycles (or during a restart) would
    /// never be reminded at all; with too generous a grace the first run after a deployment shouts
    /// about last year's abandoned trips. Two cadences is the smallest window that survives a
    /// missed cycle (spec 11 §10).
    /// </summary>
    private static readonly TimeSpan ScheduleReminderGrace = TimeSpan.FromMinutes(30);

    public async Task<int> RefreshEtasAsync(CancellationToken cancellationToken)
    {
        var refreshed = 0;
        var accountIds = await accountFeatureReader.GetEnabledAccountIdsAsync(FeatureKeys.TripManagement, cancellationToken);

        foreach (var accountId in accountIds)
        {
            var config = await accountFeatureReader.GetAccountConfigAsync(accountId, cancellationToken);
            var cutoff = DateTimeOffset.UtcNow - PositionFreshness;
            var candidates = await detectionReader.GetEtaCandidatesAsync(accountId, cutoff, cancellationToken);

            foreach (var candidate in candidates)
            {
                try
                {
                    refreshed += await RefreshCandidateAsync(accountId, candidate, config, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ETA refresh failed for trip {TripId}; it will be retried next cycle", candidate.TripId);
                }
            }
        }

        return refreshed;
    }

    private async Task<int> RefreshCandidateAsync(
        Guid accountId, EtaCandidateVm candidate, TripAccountConfigVm config, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        DateTimeOffset? etaAt;
        string etaSource;

        // A stale or missing position is NOT a reason to leave the previous Ors estimate standing:
        // that would keep presenting a hours-old guess as live-sourced. The freshness flag decides
        // which branch is honest (spec 11 §10, §18.11).
        if (candidate.HasFreshPosition
            && routingProvider.IsConfigured
            && candidate.LastLatitude is { } latitude
            && candidate.LastLongitude is { } longitude)
        {
            try
            {
                var summary = await routingProvider.GetSummaryAsync(
                    new CoordinateVm(latitude, longitude),
                    new CoordinateVm(candidate.NextStopLatitude, candidate.NextStopLongitude),
                    cancellationToken);

                // Anchored to NOW, not to the position's timestamp. The travel time still has to be
                // spent starting from this instant; anchoring it to a 14-minute-old fix made every
                // ETA up to 14 minutes optimistic and biased the TripDelayed threshold with it.
                etaAt = now.AddSeconds(summary.DurationSeconds);
                etaSource = EtaSources.Ors;

                if (candidate.LastPositionAt is { } positionAt)
                {
                    logger.LogDebug(
                        "Trip {TripId} ETA from a position {PositionAgeSeconds}s old plus {DurationSeconds}s of travel",
                        candidate.TripId,
                        (int)(now - positionAt).TotalSeconds,
                        summary.DurationSeconds);
                }
            }
            catch (RoutingUnavailableException ex)
            {
                logger.LogWarning(ex, "Routing unavailable for trip {TripId}; falling back to the planned schedule", candidate.TripId);
                (etaAt, etaSource) = FallBack(candidate);
            }
        }
        else
        {
            if (!candidate.HasFreshPosition)
            {
                logger.LogInformation(
                    "Trip {TripId} has no fresh position (last seen {LastPositionAt:o}); downgrading its next stop's ETA to {EtaSource}",
                    candidate.TripId,
                    candidate.LastPositionAt,
                    candidate.PlannedArrivalTo is null ? EtaSources.Unavailable : EtaSources.Planned);
            }

            (etaAt, etaSource) = FallBack(candidate);
        }

        // SVD-11: this job records a run only when it did work. A candidate whose stored ETA already
        // says exactly this - the steady state for a trip that has been dark for hours - must leave
        // no trace, or the on-work-only recorder degenerates into a per-cycle one.
        var changed = etaAt != candidate.CurrentEtaAt || !string.Equals(etaSource, candidate.CurrentEtaSource, StringComparison.Ordinal);
        if (changed)
            await stopWriter.UpdateStopEtaAsync(candidate.NextStopId, accountId, etaAt, etaSource, cancellationToken);

        var alerted = await RaiseDelayIfNeededAsync(accountId, candidate, etaAt, config, cancellationToken);
        return changed || alerted ? 1 : 0;
    }

    /// <summary>No live position or no reachable provider: the plan is the honest answer, labelled as such.</summary>
    private static (DateTimeOffset? EtaAt, string EtaSource) FallBack(EtaCandidateVm candidate)
        => candidate.PlannedArrivalTo is { } planned
            ? (planned, EtaSources.Planned)
            : (null, EtaSources.Unavailable);

    /// <summary>Returns true when an alert was actually emitted and stamped - that counts as work.</summary>
    private async Task<bool> RaiseDelayIfNeededAsync(
        Guid accountId, EtaCandidateVm candidate, DateTimeOffset? etaAt, TripAccountConfigVm config, CancellationToken cancellationToken)
    {
        if (candidate.DelayAlertedAt is not null
            || etaAt is not { } eta
            || candidate.PlannedArrivalTo is not { } plannedTo)
        {
            return false;
        }

        var threshold = plannedTo.AddMinutes(config.DelayThresholdMinutes);
        if (eta <= threshold)
            return false;

        var delayMinutes = (int)(eta - plannedTo).TotalMinutes;
        var alertedAt = DateTimeOffset.UtcNow;

        try
        {
            await alertEmitter.EmitAsync(
                TripEventTypes.TripDelayed,
                TripAlertSeverities.Warning,
                $"trip-delayed:{candidate.NextStopId:N}",
                new TripAlertDto(accountId, candidate.TripId, candidate.NextStopId, candidate.Code, candidate.TransporterId,
                    candidate.DriverId, candidate.NextStopName, alertedAt, eta, plannedTo, delayMinutes, null, null),
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Not stamped, so the next cycle retries: a Manager blip must not swallow a delay.
            logger.LogError(ex, "Failed to emit TripDelayed alert for stop {TripStopId}; it will be retried", candidate.NextStopId);
            return false;
        }

        // Stamped only after a successful emission — the one-shot marker (acceptance: once per stop).
        await stopWriter.MarkStopDelayAlertedAsync(candidate.NextStopId, accountId, alertedAt, cancellationToken);
        return true;
    }

    public async Task<int> RaiseStartRemindersAsync(CancellationToken cancellationToken)
    {
        var raised = 0;
        var accountIds = await accountFeatureReader.GetEnabledAccountIdsAsync(FeatureKeys.TripManagement, cancellationToken);

        foreach (var accountId in accountIds)
        {
            var config = await accountFeatureReader.GetAccountConfigAsync(accountId, cancellationToken);
            var now = DateTimeOffset.UtcNow;
            var dueBefore = now.AddMinutes(config.ScheduleLeadMinutes);
            var dueAfter = now - ScheduleReminderGrace;
            var trips = await detectionReader.GetTripsDueToStartAsync(accountId, dueAfter, dueBefore, cancellationToken);

            foreach (var trip in trips)
            {
                try
                {
                    // The event log is the once-only guard. NOTHING here changes trip status:
                    // auto-starting a trip nobody began would fabricate an ActualStartAt and a
                    // distance baseline out of thin air (spec 11 §10).
                    //
                    // Order matters: CHECK the key, EMIT, then APPEND. Appending first burned the
                    // key before the emission was known to have succeeded, so a transient Manager
                    // failure meant the reminder was never retried and the account got it zero
                    // times rather than once. Same rule the delay and deviation paths follow —
                    // the marker is written only after a successful emission.
                    var idempotencyKey = $"trip-start-due:{trip.TripId:N}";

                    if (await tripEventWriter.HasEventAsync(accountId, idempotencyKey, cancellationToken))
                        continue;

                    await alertEmitter.EmitAsync(
                        TripEventTypes.TripStartDue,
                        TripAlertSeverities.Info,
                        $"trip-startdue:{trip.TripId:N}",
                        new TripAlertDto(accountId, trip.TripId, null, trip.Code, trip.TransporterId, trip.DriverId, null,
                            trip.PlannedStartAt, null, null, null, null, null),
                        cancellationToken);

                    // The unique index is still the authority: two concurrent cycles both pass the
                    // check above, and the loser's append returns false rather than throwing.
                    if (!await tripEventWriter.AppendAsync(
                        accountId, trip.TripId, null, TripEventTypes.TripStartDue, DateTimeOffset.UtcNow,
                        TripEventSources.Job, null, idempotencyKey, cancellationToken))
                    {
                        continue;
                    }

                    raised++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Schedule reminder failed for trip {TripId}", trip.TripId);
                }
            }
        }

        return raised;
    }
}
