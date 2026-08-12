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

using Common.Application.Attributes;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Domain.Enumerators;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Domain.Extensions;
using TrackHub.Router.Application.DevicePositions.Events;

namespace TrackHub.Router.Application.DevicePositions.Commands.Sync;

// In-process only (no [Authorize]): the SyncWorker's position loop dispatches it. The account rides
// inside OperatorVm (and again inside AccountSettingsVm — the resolver takes the ordinally-first
// depth-1 path, Operator, and both name the same account).
[AllowCrossAccount("SyncWorker position loop: one global syncworker_client identity enumerates every account's operators and fetches their positions, so the OperatorVm's AccountId is by definition not the worker's own (it has none).")]
public readonly record struct GetPositionsByOperatorCommand(
    OperatorVm Operator,
    AccountSettingsVm Settings,
    string TriggerType = "AUTOMATIC",
    string? CorrelationId = null) : IRequest<bool>;

public class GetPositionsByOperatorCommandHandler(
        IPublisher publisher,
        IConfiguration configuration,
        IPositionRegistry positionRegistry,
        IDeviceTransporterReader deviceReader,
        IDeviceCatalogCache deviceCatalogCache,
        IProviderCapabilityCatalog capabilityCatalog,
        ILogger<GetPositionsByOperatorCommandHandler> logger)
        : IRequestHandler<GetPositionsByOperatorCommand, bool>
{
    private string? EncryptionKey { get; } = configuration["AppSettings:EncryptionKey"];

    /// <summary>
    /// Retrieves the device positions asynchronously
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the collection of PositionVm</returns>
    public async Task<bool> Handle(GetPositionsByOperatorCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(EncryptionKey, message: "Credential key not found.");

        // A provider that declares no RealTimePositions is a DECLARED capability gap, not a
        // failure — polling it would record a FAILED run every cycle for a limitation of the
        // provider's API. The position loop skips it quietly.
        var protocol = (ProtocolType)request.Operator.ProtocolTypeId;
        if (!capabilityCatalog.Supports(protocol, ProviderCapability.RealTimePositions))
        {
            logger.LogDebug(
                "Position sync skipped for operator {OperatorId} (account {AccountId}): provider {Protocol} declares no real-time positions.",
                request.Operator.OperatorId, request.Operator.AccountId, protocol);
            return false;
        }

        if (request.Operator.Credential is not null)
        {
            var startedAt = DateTimeOffset.UtcNow;
            var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString();
            var reader = positionRegistry.GetReader((ProtocolType)request.Operator.ProtocolTypeId);
            await reader.Init(request.Operator.Credential.Value.Decrypt(EncryptionKey), cancellationToken);
            // The device→transporter catalog changes rarely; serve it from a short-TTL cache so the
            // 10-second position loop does not re-fetch it from Manager every cycle per operator
            // (router-audit A-12). The device-sync loop invalidates it on catalog changes.
            var devices = await deviceCatalogCache.GetOrLoadAsync(
                request.Settings.AccountId,
                request.Operator.OperatorId,
                ct => deviceReader.GetDeviceTransporterAsync(request.Settings.AccountId, request.Operator.OperatorId, ct),
                cancellationToken);
            var (positions, errorCode, errorMessage) = await TryGetPositionsAsync(reader, request.Operator, devices, cancellationToken);
            await publisher.Publish(
                new PositionsRetrieved.Notification(
                    positions,
                    request.Settings,
                    request.Operator,
                    startedAt,
                    request.TriggerType,
                    correlationId,
                    errorCode,
                    errorMessage),
                cancellationToken);
        }
        return true;
    }

    // A provider position-fetch failure must NOT masquerade as a successful sync with zero fixes
    // the error is logged and returned so PositionsRetrieved records the run
    // as FAILED with the error code and raises the sync-failed alert. Returning empty here (rather
    // than throwing) keeps the failure isolated to this operator — the notification still fires so
    // the run is recorded, and cancellation propagates.
    private async Task<(IEnumerable<PositionVm> Positions, string? ErrorCode, string? ErrorMessage)> TryGetPositionsAsync(
        IPositionReader reader,
        OperatorVm @operator,
        IEnumerable<DeviceTransporterVm> devices,
        CancellationToken cancellationToken)
    {
        try
        {
            var positions = await reader.GetDevicePositionAsync(devices, cancellationToken);
            return (positions, null, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Provider position fetch failed for operator {OperatorId} (account {AccountId}).",
                @operator.OperatorId, @operator.AccountId);
            return ([], ex.GetType().Name, ex.Message);
        }
    }

}
