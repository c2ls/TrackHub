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
using Common.Domain.Constants;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Domain.Interfaces.Geocoding;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.Geocoding.Queries;

// Resolves a single coordinate through the geocoding abstraction. When the coordinate
// corresponds to a TrackHub-stored row (ids supplied), the resolved address is written back
// best-effort — but ONLY after confirming the caller may see the referenced transporter, so a
// foreign id cannot stamp an address onto another tenant's row (see CanCacheForCallerAsync).
[Authorize(Resource = Resources.Positions, Action = Actions.Read)]
[RateLimiting(PermitLimit = 10, WindowSeconds = 60)]
[AccountScopeEnforcedInHandler]
public readonly record struct ReverseGeocodeQuery(
    double Latitude,
    double Longitude,
    Guid? TransporterId = null,
    Guid? TransporterPositionHistoryId = null) : IRequest<AddressVm>;

public class ReverseGeocodeQueryHandler(
    IReverseGeocodingService geocodingService,
    IResolvedAddressSystemWriter addressWriter,
    IOperatorReader operatorReader,
    ILogger<ReverseGeocodeQueryHandler> logger) : IRequestHandler<ReverseGeocodeQuery, AddressVm>
{
    public async Task<AddressVm> Handle(ReverseGeocodeQuery request, CancellationToken cancellationToken)
    {
        var address = await geocodingService.ResolveAsync(request.Latitude, request.Longitude, cancellationToken);
        if (address is null)
        {
            return new AddressVm(null, null, null, null);
        }

        // The write-back stamps the resolved address onto a tenant-owned stored position row. Cache it
        // only when the caller may actually see the referenced transporter — otherwise a foreign
        // TransporterId would let one tenant write onto another tenant's row.
        if (!string.IsNullOrWhiteSpace(address.Value.Address)
            && await CanCacheForCallerAsync(request, cancellationToken))
        {
            try
            {
                await addressWriter.PersistResolvedAddressAsync(
                    request.TransporterPositionHistoryId,
                    request.TransporterId,
                    address.Value,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                // Best-effort persistence: the resolved address is still returned.
                logger.LogWarning(ex, "Failed to persist resolved address for transporter {TransporterId}.", request.TransporterId);
            }
        }

        return address.Value;
    }

    // Enforces caller access to the row the address would be cached onto. The caller-scoped operator
    // read goes through Manager under the caller's token, so a transporter outside the caller's
    // account/groups resolves to nothing and the write-back is skipped. A write-back keyed only by
    // TransporterPositionHistoryId is not caller-verifiable at this layer, so it is not cached.
    private async Task<bool> CanCacheForCallerAsync(ReverseGeocodeQuery request, CancellationToken cancellationToken)
    {
        if (request.TransporterId is not { } transporterId)
        {
            return false;
        }

        try
        {
            var scoped = await operatorReader.GetOperatorByTransporterAsync(transporterId, cancellationToken);
            return scoped.OperatorId != Guid.Empty;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Ownership resolve failed for transporter {TransporterId}; skipping address write-back.", transporterId);
            return false;
        }
    }
}

public sealed class ReverseGeocodeQueryValidator : AbstractValidator<ReverseGeocodeQuery>
{
    public ReverseGeocodeQueryValidator()
    {
        RuleFor(v => v.Latitude).InclusiveBetween(-90, 90);
        RuleFor(v => v.Longitude).InclusiveBetween(-180, 180);
    }
}
