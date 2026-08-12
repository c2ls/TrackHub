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
/// Dwell-threshold evaluation: scans open visits whose geofence defines a dwell
/// threshold and emits <c>GeofenceDwellExceeded</c> exactly once per visit. Triggered by elapsed
/// time, not by new positions, so a silent device still alerts. Emission failures leave the
/// visit unstamped and it is retried next cycle (Manager-side dedup makes that safe).
/// </summary>
public class DwellEvaluationService(
    IGeofenceEventReader geofenceEventReader,
    IGeofenceEventWriter geofenceEventWriter,
    IAlertEmitter alertEmitter,
    IBackgroundJobRunRecorder jobRunRecorder,
    ILogger<DwellEvaluationService> logger) : IDwellEvaluationService
{
    public const string JobKey = BackgroundJobKeys.GeofenceDwellEvaluation;

    public async Task<int> EvaluateDwellAsync(CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var candidates = await geofenceEventReader.GetDwellAlertCandidatesAsync(cancellationToken);
        var alerted = 0;

        foreach (var candidate in candidates)
        {
            var now = DateTimeOffset.UtcNow;
            if (candidate.EventDateTime.AddMinutes(candidate.DwellThresholdMinutes) > now)
                continue;

            try
            {
                var dwellSeconds = (long)(now - candidate.EventDateTime).TotalSeconds;
                await alertEmitter.EmitGeofenceDwellExceededAsync(new GeofenceAlertDto(
                    candidate.GeofenceEventId,
                    candidate.AccountId,
                    candidate.TransporterId,
                    candidate.GeofenceId,
                    candidate.GeofenceName,
                    candidate.GeofenceType,
                    candidate.EventDateTime,
                    null,
                    dwellSeconds,
                    candidate.Latitude,
                    candidate.Longitude), cancellationToken);

                await geofenceEventWriter.StampDwellAlertedAsync(candidate.GeofenceEventId, now, cancellationToken);
                alerted++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Dwell alert emission failed for event {GeofenceEventId}; it will be retried next cycle", candidate.GeofenceEventId);
            }
        }

        if (alerted > 0)
        {
            try
            {
                await jobRunRecorder.RecordAsync(
                    JobKey,
                    alerted.ToString(),
                    $"geofence-dwell:{startedAt:yyyyMMddHHmmssfff}",
                    "Succeeded",
                    startedAt,
                    DateTimeOffset.UtcNow,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to record background job run for {JobKey}", JobKey);
            }
        }

        return alerted;
    }
}
