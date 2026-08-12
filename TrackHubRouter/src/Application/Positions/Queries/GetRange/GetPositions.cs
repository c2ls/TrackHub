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
using TrackHub.Router.Domain.Interfaces;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.Positions.Queries.GetRange;

[Authorize(Resource = Resources.Positions, Action = Actions.Read)]
[RateLimiting(PermitLimit = 3, WindowSeconds = 60)]
[AccountScopeEnforcedInHandler]
public readonly record struct GetPositionsRecordQuery(Guid TransporterId, DateTimeOffset From, DateTimeOffset To, PositionSourceType Source = PositionSourceType.Provider) : IRequest<IEnumerable<PositionVm>>;

public class GetPositionsRecordQueryHandler(
        IConfiguration configuration,
        IOperatorReader operatorReader,
        IOperatorSystemReader operatorSystemReader,
        IProviderCapabilityCatalog capabilityCatalog,
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
        // Resolved under the caller's identity, so Manager applies their account scope.
        var scoped = await operatorReader.GetOperatorByTransporterAsync(request.TransporterId, cancellationToken);
        var device = await deviceReader.GetDevicesTransporterAsync(request.TransporterId, cancellationToken);
        await EnsureTransporterVisibilityAsync(groupVisibilityReader, principal, scoped.AccountId, request.TransporterId, cancellationToken);

        if (request.Source == PositionSourceType.Stored)
        {
            if (!await modeResolver.IsPositionHistoryEnabledAsync(scoped.AccountId, cancellationToken))
            {
                throw new FeatureDisabledException(FeatureKeys.GpsPositionHistory, scoped.AccountId);
            }

            var stored = await positionHistoryReader.GetPositionHistoryRangeAsync(scoped.AccountId, request.TransporterId, request.From, request.To, cancellationToken);
            return stored.Select(p => p with { DeviceName = device.Name, TransporterType = device.TransporterType });
        }

        // PROVIDER replay needs the decrypted credential, read with the Router's service identity
        // by operator id: the service principal carries no account scope, so only the
        // [AllowCrossAccount] by-id operator read admits it (the by-transporter read is
        // caller-scoped and fail-closed denies account-less principals).
        var @operator = await operatorSystemReader.GetOperatorAsync(scoped.OperatorId, cancellationToken);

        return await GetDevicePositionAsync(
            capabilityCatalog,
            positionRegistry,
            EncryptionKey,
            @operator,
            request.From,
            request.To,
            device,
            cancellationToken);

    }

}

public sealed class GetPositionsRecordQueryValidator : AbstractValidator<GetPositionsRecordQuery>
{
    private const int MaxRangeDays = 31;

    public GetPositionsRecordQueryValidator()
    {
        RuleFor(v => v.TransporterId)
            .NotEmpty();

        RuleFor(v => v)
            .Must(v => v.From < v.To)
            .WithMessage("From must be earlier than To.");

        RuleFor(v => v)
            .Must(v => (v.To - v.From) <= TimeSpan.FromDays(MaxRangeDays))
            .WithMessage($"The requested range exceeds the maximum of {MaxRangeDays} days.");
    }
}
