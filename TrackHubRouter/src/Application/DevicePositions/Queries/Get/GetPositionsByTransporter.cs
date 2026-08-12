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
using Common.Application.Attributes;
using Common.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Domain.Extensions;

namespace TrackHub.Router.Application.DevicePositions.Queries.Get;

[Authorize(Resource = Resources.Positions, Action = Actions.Read)]
public readonly record struct GetPositionByTransporterQuery(Guid TransporterId) : IRequest<PositionVm>;

public class GetPositionByTransporterQueryHandler(
        IConfiguration configuration,
        Application.Gating.IAccountModeResolver modeResolver,
        IOperatorReader operatorReader,
        IPositionRegistry positionRegistry,
        IDeviceTransporterReader deviceReader,
        ITransporterPositionReader transporterPositionReader,
        IPositionSystemWriter positionSystemWriter,
        ILogger<GetPositionByTransporterQueryHandler> logger)
        : IRequestHandler<GetPositionByTransporterQuery, PositionVm>
{
    private string? EncryptionKey { get; } = configuration["AppSettings:EncryptionKey"];

    /// <summary>
    /// Get the position of a device by its transporter id.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<PositionVm> Handle(GetPositionByTransporterQuery request, CancellationToken cancellationToken)
    {
        var @operator = await operatorReader.GetOperatorByTransporterAsync(request.TransporterId, cancellationToken);
        // Mode split through the single resolver: integration enabled -> serve the
        // stored projection; disabled -> read the provider on demand.
        var integrationEnabled = await modeResolver.IsIntegrationEnabledAsync(@operator.AccountId, cancellationToken);
        if (!@operator.Enabled || integrationEnabled)
        {
            return await GetFallbackPositionAsync(@operator.OperatorId, request.TransporterId, cancellationToken);
        }

        return await TryGetDevicePositionAsync(@operator, request.TransporterId, cancellationToken);
    }

    /// <summary>
    /// Get the position of a device by its transporter id.
    /// </summary>
    /// <param name="encryptionKey"></param>
    /// <param name="operator"></param>
    /// <param name="device"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<PositionVm> TryGetDevicePositionAsync(
        OperatorVm @operator,
        Guid transporterId,
        CancellationToken cancellationToken)
    {
        try
        {
            Guard.Against.Null(EncryptionKey, message: "Credential key not found.");
            var device = await deviceReader.GetDevicesTransporterAsync(transporterId, cancellationToken);
            var reader = positionRegistry.GetReader((ProtocolType)@operator.ProtocolTypeId);
            if (@operator.Credential is not null)
            {
                await reader.Init(@operator.Credential.Value.Decrypt(EncryptionKey), cancellationToken);
                var position = await reader.GetDevicePositionAsync(device, cancellationToken);
                if (position.TransporterId != Guid.Empty)
                {
                    // On-demand mode: keep the latest-position projection current with the
                    // provider read, using the Router's service identity (best effort).
                    await PersistLatestPositionAsync(position, cancellationToken);
                    return position;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error retrieving position for transporter {TransporterId}", transporterId);
        }

        return await GetFallbackPositionAsync(@operator.OperatorId, transporterId, cancellationToken);
    }

    private async Task PersistLatestPositionAsync(PositionVm position, CancellationToken cancellationToken)
    {
        if (position.DeviceDateTime == default
            || position.Latitude is < -90d or > 90d
            || position.Longitude is < -180d or > 180d)
        {
            return;
        }

        try
        {
            await positionSystemWriter.AddOrUpdatePositionAsync([position], cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Failed to persist on-demand position for transporter {TransporterId}; the read is unaffected.", position.TransporterId);
        }
    }

    private async Task<PositionVm> GetFallbackPositionAsync(
        Guid operatorId,
        Guid transporterId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await transporterPositionReader.GetTransporterPositionAsync(operatorId, transporterId, cancellationToken)
                ?? default;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error retrieving fallback position for transporter {TransporterId}", transporterId);
            return default;
        }
    }

}
