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
using TrackHub.Router.Domain.Interfaces.Geocoding;
using TrackHub.Router.Domain.Interfaces.Geofence;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.DevicePositions.Events;

public sealed class PositionsRetrieved
{
    public readonly record struct Notification(
        IEnumerable<PositionVm> Positions,
        AccountSettingsVm Settings,
        OperatorVm Operator,
        DateTimeOffset StartedAt,
        string TriggerType,
        string CorrelationId,
        string? ProviderErrorCode = null,
        string? ProviderErrorMessage = null) : INotification
    {
        public class EventHandler(
            IPositionWriter positionWriter,
            IGeofenceWriter geofenceWriter,
            IOperatorSyncRunWriter syncRunWriter,
            IAlertEventWriter alertWriter,
            IReverseGeocodingService geocodingService,
            IPositionHistorySystemWriter historyWriter,
            ILogger<EventHandler> logger) : INotificationHandler<Notification>
        {
            public async Task Handle(Notification notification, CancellationToken cancellationToken)
            {
                var positions = notification.Positions?.ToArray() ?? [];
                var positionsRead = positions.Length;
                int positionsAccepted = 0;
                string? errorCode = notification.ProviderErrorCode;
                string? errorMessage = notification.ProviderErrorMessage;
                // A provider fetch that threw is a FAILED run, not a silent "0 fixes" success
                //The provider error is carried on the notification.
                var providerFailed = notification.ProviderErrorCode is not null;
                var result = providerFailed ? "FAILED" : "SUCCEEDED";

                var validPositionCandidates = positions
                    .Where(IsValidPosition)
                    .ToArray();
                var invalidPositionCount = positionsRead - validPositionCandidates.Length;

                // The per-transporter dedupe (freshest fix per transporter wins) applies ONLY to the
                // latest-position projection. The history append below receives every valid fix — a
                // transporter with two active devices records both — while the idempotency key
                // prevents duplicates.
                var validPositions = LatestPerTransporter(validPositionCandidates);
                positionsAccepted = validPositions.Length;
                if (invalidPositionCount > 0)
                {
                    result = validPositions.Length == 0 ? "FAILED" : "PARTIALLY_SUCCEEDED";
                    errorCode = "InvalidPosition";
                    errorMessage = $"{invalidPositionCount} position(s) rejected by validation.";
                }

                try
                {
                    if (validPositions.Length > 0)
                    {
                        // Phase 1 — freshness-critical: store the latest projection and run geofence
                        // detection with the addresses the provider already supplied. Reverse-
                        // geocoding (with its fleet-wide throttle) is deferred to phase 2 so it can
                        // never delay position storage or detection (router-audit A-10).
                        var writeSucceeded = await positionWriter.AddOrUpdatePositionAsync(validPositions, cancellationToken);
                        if (!writeSucceeded)
                        {
                            result = "FAILED";
                            positionsAccepted = 0;
                            errorCode = "PositionWriteRejected";
                            errorMessage = "Manager rejected the position upsert.";
                        }

                        if (writeSucceeded && notification.Settings.GeofencingEnabled)
                        {
                            // Best-effort, isolated from the sync-run classification: a Geofencing
                            // outage must not flip an already-stored position batch to FAILED nor
                            // raise a false position-sync-failed alert (router-audit A-09).
                            try
                            {
                                await geofenceWriter.ProcessPositionsAsync(validPositions, notification.Settings.AccountId, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                logger.LogWarning(ex, "Geofence processing failed for operator {OperatorId} (account {AccountId}); positions were stored.",
                                    notification.Operator.OperatorId, notification.Settings.AccountId);
                            }
                        }

                        // Phase 2 — best-effort enrichment off the freshness path: fill blank
                        // addresses, re-upsert the projection ONLY if enrichment resolved something
                        // (avoids a pointless second write), and append the enriched history.
                        if (writeSucceeded)
                        {
                            var (enrichedCandidates, anyResolved) = await EnrichAddressesAsync(validPositionCandidates, cancellationToken);

                            if (anyResolved)
                            {
                                try
                                {
                                    await positionWriter.AddOrUpdatePositionAsync(LatestPerTransporter(enrichedCandidates), cancellationToken);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogWarning(ex, "Address back-fill update failed for operator {OperatorId} (account {AccountId}); the fresh projection is already stored.",
                                        notification.Operator.OperatorId, notification.Settings.AccountId);
                                }
                            }

                            // Stored history receives ALL valid fixes (not the deduped latest set) so
                            // multi-device transporters keep every fix. Failures never fail the run.
                            if (notification.Settings.GpsPositionHistoryEnabled)
                            {
                                try
                                {
                                    await historyWriter.AppendRangeAsync(
                                        notification.Settings.AccountId,
                                        notification.Operator.OperatorId,
                                        enrichedCandidates,
                                        cancellationToken);
                                }
                                catch (Exception ex)
                                {
                                    logger.LogWarning(ex, "Position history append failed for operator {OperatorId} (account {AccountId}).",
                                        notification.Operator.OperatorId, notification.Settings.AccountId);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = "FAILED";
                    positionsAccepted = 0;
                    errorCode = ex.GetType().Name;
                    errorMessage = ex.Message;
                    logger.LogError(ex, "Position sync failed for operator {OperatorId} (account {AccountId}).",
                        notification.Operator.OperatorId, notification.Settings.AccountId);
                }

                try
                {
                    await syncRunWriter.RecordAsync(new OperatorSyncRunDto(
                        AccountId: notification.Settings.AccountId,
                        OperatorId: notification.Operator.OperatorId,
                        TriggerType: notification.TriggerType,
                        Result: result,
                        StartedAt: notification.StartedAt,
                        CompletedAt: DateTimeOffset.UtcNow,
                        DevicesSeen: 0,
                        DevicesAdded: 0,
                        DevicesUpdated: 0,
                        DevicesRemoved: 0,
                        DevicesIgnored: 0,
                        PositionsRead: positionsRead,
                        PositionsAccepted: positionsAccepted,
                        PositionsRejected: positionsRead - positionsAccepted,
                        ErrorCode: errorCode,
                        ErrorMessage: errorMessage,
                        CorrelationId: notification.CorrelationId), cancellationToken);

                    if (result == "FAILED")
                    {
                        await alertWriter.RecordAsync(new AlertEventDto(
                            AccountId: notification.Settings.AccountId,
                            EventType: "GpsOperatorPositionSyncFailed",
                            Severity: "Warning",
                            SourceModule: "TrackHub.Router.SyncWorker",
                            ResourceType: "Operator",
                            ResourceId: notification.Operator.OperatorId.ToString(),
                            Status: "Open",
                            PayloadJson: System.Text.Json.JsonSerializer.Serialize(new { errorCode, message = errorMessage ?? "Position sync failed" }),
                            DeduplicationKey: $"position-sync-failed:{notification.Operator.OperatorId}:{DateTimeOffset.UtcNow:yyyyMMddHH}"), cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to record sync telemetry for operator {OperatorId}.",
                        notification.Operator.OperatorId);
                }
            }

            // Freshest fix per transporter wins — the latest-position projection.
            private static PositionVm[] LatestPerTransporter(IEnumerable<PositionVm> candidates)
                => candidates
                    .GroupBy(p => p.TransporterId)
                    .Select(g => g
                        .OrderByDescending(p => p.DeviceDateTime)
                        .ThenByDescending(p => p.ServerDateTime ?? DateTimeOffset.MinValue)
                        .First())
                    .ToArray();

            // Returns the enriched positions plus whether any blank address was actually resolved
            // (so the caller only re-writes the projection when enrichment changed something).
            private async Task<(PositionVm[] Positions, bool AnyResolved)> EnrichAddressesAsync(PositionVm[] positions, CancellationToken cancellationToken)
            {
                var anyResolved = false;
                try
                {
                    var budget = await geocodingService.GetEnrichmentBudgetAsync(cancellationToken);
                    if (budget <= 0)
                    {
                        return (positions, false);
                    }

                    for (var i = 0; i < positions.Length && budget > 0; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(positions[i].Address))
                        {
                            continue;
                        }

                        budget--;
                        var address = await geocodingService.TryResolveAsync(positions[i].Latitude, positions[i].Longitude, cancellationToken);
                        if (address.HasValue && !string.IsNullOrWhiteSpace(address.Value.Address))
                        {
                            positions[i] = positions[i] with
                            {
                                Address = address.Value.Address,
                                City = address.Value.City,
                                State = address.Value.State,
                                Country = address.Value.Country
                            };
                            anyResolved = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Address enrichment failed; storing positions without addresses.");
                }

                return (positions, anyResolved);
            }

            private static bool IsValidPosition(PositionVm position)
                => position.TransporterId != Guid.Empty
                   && position.DeviceDateTime != default
                   && position.Latitude is >= -90d and <= 90d
                   && position.Longitude is >= -180d and <= 180d;
        }
    }
}
