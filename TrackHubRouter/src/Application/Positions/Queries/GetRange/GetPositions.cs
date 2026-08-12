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
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using Microsoft.Extensions.Configuration;
using TrackHub.Router.Domain.Enumerators;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.Positions.Queries.GetRange;

[Authorize(Resource = Resources.Positions, Action = Actions.Read)]
[RateLimiting(PermitLimit = 3, WindowSeconds = 60)]
public readonly record struct GetPositionsRecordQuery(Guid TransporterId, DateTimeOffset From, DateTimeOffset To, PositionSourceType Source = PositionSourceType.Provider) : IRequest<IEnumerable<PositionVm>>;

public class GetPositionsRecordQueryHandler(
        IConfiguration configuration,
        IOperatorReader operatorReader,
        IPositionRegistry positionRegistry,
        IDeviceTransporterReader deviceReader,
        Application.Gating.IAccountModeResolver modeResolver,
        IPositionHistoryReader positionHistoryReader,
        IGroupVisibilityReader groupVisibilityReader,
        ICurrentPrincipal principal)
        : PositionBaseHandler, IRequestHandler<GetPositionsRecordQuery, IEnumerable<PositionVm>>
{
    private string? EncryptionKey { get; } = configuration["AppSettings:EncryptionKey"];

    /// <summary>
    /// Retrieves the operator, and device positions asynchronously.
    /// PROVIDER (default) reads the GPS operator API on demand; STORED reads
    /// TrackHub-stored history through Manager and requires gps.positionHistory.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the collection of PositionVm</returns>
    public async Task<IEnumerable<PositionVm>> Handle(GetPositionsRecordQuery request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(EncryptionKey, message: "Credential key not found.");
        var @operator = await operatorReader.GetOperatorByTransporterAsync(request.TransporterId, cancellationToken);
        var device = await deviceReader.GetDevicesTransporterAsync(request.TransporterId, cancellationToken);
        await EnsureTransporterVisibilityAsync(groupVisibilityReader, principal, @operator.AccountId, request.TransporterId, cancellationToken);

        if (request.Source == PositionSourceType.Stored)
        {
            if (!await modeResolver.IsPositionHistoryEnabledAsync(@operator.AccountId, cancellationToken))
            {
                throw new FeatureDisabledException(FeatureKeys.GpsPositionHistory, @operator.AccountId);
            }

            var stored = await positionHistoryReader.GetPositionHistoryRangeAsync(@operator.AccountId, request.TransporterId, request.From, request.To, cancellationToken);
            return stored.Select(p => p with { DeviceName = device.Name, TransporterType = device.TransporterType });
        }

        return await GetDevicePositionAsync(
            positionRegistry,
            EncryptionKey,
            @operator,
            request.From,
            request.To,
            device,
            cancellationToken);

    }

}
