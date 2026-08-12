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

namespace TrackHub.Manager.Application.GeocodingProviders.Queries.GetActive;

// Service-client surface for the Router geocoding adapter: the ApiKey stays encrypted
// and travels with its Salt, mirroring the operator-credential consumption pattern.
[Authorize(Resource = Resources.GeocodingProviders, Action = Actions.Read, PrincipalTypes = "ServiceClient")]
[PlatformScoped("Platform geocoding-provider registry: one active provider serves every tenant, consumed by the Router geocoding adapter under its global service identity. No tenant owns a row.")]
public readonly record struct GetActiveGeocodingProviderQuery() : IRequest<GeocodingProviderTokenVm?>;

public class GetActiveGeocodingProviderQueryHandler(IGeocodingProviderReader reader) : IRequestHandler<GetActiveGeocodingProviderQuery, GeocodingProviderTokenVm?>
{
    public async Task<GeocodingProviderTokenVm?> Handle(GetActiveGeocodingProviderQuery request, CancellationToken cancellationToken)
        => await reader.GetActiveGeocodingProviderAsync(cancellationToken);
}
