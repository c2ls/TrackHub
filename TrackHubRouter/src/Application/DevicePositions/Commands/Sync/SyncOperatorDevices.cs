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

using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.DevicePositions.Commands.Sync;

public readonly record struct SyncOperatorDevicesCommand(
    OperatorVm Operator,
    string TriggerType,
    string? CorrelationId = null,
    bool ResetDeviceCatalog = false,
    bool AutoAssignNewDevices = true) : IRequest<bool>;

public class SyncOperatorDevicesCommandHandler(
    IConfiguration configuration,
    IDeviceRegistry deviceRegistry,
    IDeviceSyncWriter deviceSyncWriter,
    IOperatorSyncRunWriter syncRunWriter,
    IOperatorHealthCheckSystemWriter healthWriter,
    IAlertEventWriter alertWriter,
    IOperatorSyncLock syncLock,
    IDeviceCatalogCache deviceCatalogCache,
    ILogger<SyncOperatorDevicesCommandHandler> logger) : IRequestHandler<SyncOperatorDevicesCommand, bool>
{
    private string? EncryptionKey { get; } = configuration["AppSettings:EncryptionKey"];

    public async Task<bool> Handle(SyncOperatorDevicesCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(EncryptionKey, message: "Credential key not found.");
        if (request.Operator.Credential is null)
        {
            return false;
        }

        // Serialize per operator: a manual "sync now" and the background sync loop (or two manual
        // triggers) must not run concurrently for the same operator, or the ResetDeviceCatalog
        // wipe/rebuild races.
        using var operatorGate = await syncLock.AcquireAsync(request.Operator.OperatorId, cancellationToken);

        var startedAt = DateTimeOffset.UtcNow;
        var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString();
        var result = "SUCCEEDED";
        string? errorCode = null;
        string? errorMessage = null;
        var devices = Array.Empty<DeviceVm>();
        var counts = default(DeviceSyncCountsVm);
        var providerReached = false;
        DateTimeOffset? providerCompletedAt = null;

        try
        {
            var reader = deviceRegistry.GetReader((ProtocolType)request.Operator.ProtocolTypeId);
            await reader.Init(request.Operator.Credential.Value.Decrypt(EncryptionKey), cancellationToken);
            devices = (await reader.GetDevicesAsync(cancellationToken))?.ToArray() ?? [];
            providerReached = true;
            providerCompletedAt = DateTimeOffset.UtcNow;
            if (request.ResetDeviceCatalog)
            {
                await deviceSyncWriter.ResetAsync(request.Operator.AccountId, request.Operator.OperatorId, cancellationToken);
            }

            var dtos = devices.Select(d => new SynchronizedDeviceDto(
                AccountId: request.Operator.AccountId,
                OperatorId: request.Operator.OperatorId,
                Serial: d.Serial,
                Name: d.Name,
                Identifier: d.Identifier,
                ProviderDisplayName: d.ProviderDisplayName ?? d.Name,
                DeviceTypeId: d.DeviceTypeId,
                Description: null,
                ProviderMetadataHash: d.ProviderMetadataHash,
                ProviderStatus: d.ProviderStatus));

            // Manager returns the counts and no longer records the run.
            counts = await deviceSyncWriter.SynchronizeAsync(
                request.Operator.AccountId,
                request.Operator.OperatorId,
                dtos,
                correlationId,
                request.TriggerType,
                request.AutoAssignNewDevices,
                cancellationToken);

            // The catalog may have changed (devices added/removed) — drop the cached copy so the
            // position loop picks up the new set immediately (router-audit A-12).
            deviceCatalogCache.Invalidate(request.Operator.OperatorId);
        }
        catch (Exception ex)
        {
            result = "FAILED";
            errorCode = ex.GetType().Name;
            errorMessage = ex.Message;
            logger.LogError(ex, "Device sync failed for operator {OperatorId} (account {AccountId}).",
                request.Operator.OperatorId, request.Operator.AccountId);
        }

        // Single sync-run writer: the Router records exactly one run per attempt, with
        // identical field completeness for success (Manager's counts) and failure (counts default to
        // zero, DevicesSeen reflects what the provider returned before the failure). Best-effort:
        // telemetry failures never fail the sync itself.
        var succeeded = result == "SUCCEEDED";
        try
        {
            await syncRunWriter.RecordAsync(new OperatorSyncRunDto(
                AccountId: request.Operator.AccountId,
                OperatorId: request.Operator.OperatorId,
                TriggerType: request.TriggerType,
                Result: result,
                StartedAt: startedAt,
                CompletedAt: DateTimeOffset.UtcNow,
                DevicesSeen: succeeded ? counts.DevicesSeen : devices.Length,
                DevicesAdded: counts.DevicesAdded,
                DevicesUpdated: counts.DevicesUpdated,
                DevicesRemoved: counts.DevicesRemoved,
                DevicesIgnored: counts.DevicesIgnored,
                PositionsRead: 0,
                PositionsAccepted: 0,
                PositionsRejected: 0,
                ErrorCode: errorCode,
                ErrorMessage: errorMessage,
                CorrelationId: correlationId), cancellationToken);

            // Every sync attempt IS a connectivity observation: reaching the provider proves the
            // operator is up, failing to reach it proves it is not. Recording it here keeps the
            // derived Health status meaningful for accounts that only sync manually (no
            // background worker / gps.integration required). Uses the Router's service identity —
            // recordOperatorHealth is ServiceClient-only.
            var checkCompletedAt = providerCompletedAt ?? DateTimeOffset.UtcNow;
            await healthWriter.RecordAsync(new OperatorHealthCheckDto(
                AccountId: request.Operator.AccountId,
                OperatorId: request.Operator.OperatorId,
                // Telemetry's OperatorHealthCheckType enum, GraphQL form (like the worker's "PING").
                CheckType: "DEVICE_SYNC",
                Status: providerReached ? "HEALTHY" : "OFFLINE",
                LatencyMs: (int)(checkCompletedAt - startedAt).TotalMilliseconds,
                StartedAt: startedAt,
                CompletedAt: checkCompletedAt,
                ErrorCode: providerReached ? null : errorCode,
                ErrorMessage: providerReached ? null : errorMessage,
                RetryCount: 0,
                CorrelationId: correlationId), cancellationToken);

            if (!succeeded)
            {
                await alertWriter.RecordAsync(new AlertEventDto(
                    AccountId: request.Operator.AccountId,
                    EventType: "GpsOperatorDeviceSyncFailed",
                    Severity: "Warning",
                    SourceModule: "TrackHub.Router.SyncWorker",
                    ResourceType: "Operator",
                    ResourceId: request.Operator.OperatorId.ToString(),
                    Status: "Open",
                    PayloadJson: System.Text.Json.JsonSerializer.Serialize(new { errorCode, message = errorMessage ?? "Device sync failed" }),
                    DeduplicationKey: $"device-sync-failed:{request.Operator.OperatorId}:{DateTimeOffset.UtcNow:yyyyMMddHH}"), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to record device-sync telemetry for operator {OperatorId}.",
                request.Operator.OperatorId);
        }

        return succeeded;
    }
}
