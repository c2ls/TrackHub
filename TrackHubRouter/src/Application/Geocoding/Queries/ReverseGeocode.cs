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
// corresponds to a TrackHub-stored row (ids supplied), the resolved address is written
// back best-effort with the Router's own service identity — never the user token.
[Authorize(Resource = Resources.Positions, Action = Actions.Read)]
[RateLimiting(PermitLimit = 10, WindowSeconds = 60)]
public readonly record struct ReverseGeocodeQuery(
    double Latitude,
    double Longitude,
    Guid? TransporterId = null,
    Guid? TransporterPositionHistoryId = null) : IRequest<AddressVm>;

public class ReverseGeocodeQueryHandler(
    IReverseGeocodingService geocodingService,
    IResolvedAddressSystemWriter addressWriter,
    ILogger<ReverseGeocodeQueryHandler> logger) : IRequestHandler<ReverseGeocodeQuery, AddressVm>
{
    public async Task<AddressVm> Handle(ReverseGeocodeQuery request, CancellationToken cancellationToken)
    {
        var address = await geocodingService.ResolveAsync(request.Latitude, request.Longitude, cancellationToken);
        if (address is null)
        {
            return new AddressVm(null, null, null, null);
        }

        if ((request.TransporterId.HasValue || request.TransporterPositionHistoryId.HasValue)
            && !string.IsNullOrWhiteSpace(address.Value.Address))
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
}

public sealed class ReverseGeocodeQueryValidator : AbstractValidator<ReverseGeocodeQuery>
{
    public ReverseGeocodeQueryValidator()
    {
        RuleFor(v => v.Latitude).InclusiveBetween(-90, 90);
        RuleFor(v => v.Longitude).InclusiveBetween(-180, 180);
    }
}
