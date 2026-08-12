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

namespace TrackHub.Manager.Application.GeocodingProviders.Queries.Get;

// The VM carries the decrypted ApiKey for the SuperAdministrator editor form only
// (authorized-CRUD exception); the resource seeding restricts this surface to
// the Administrator role.
[Authorize(Resource = Resources.GeocodingProviders, Action = Actions.Read)]
public readonly record struct GetGeocodingProvidersQuery() : IRequest<IReadOnlyCollection<GeocodingProviderVm>>;

public class GetGeocodingProvidersQueryHandler(IGeocodingProviderReader reader, IConfiguration configuration) : IRequestHandler<GetGeocodingProvidersQuery, IReadOnlyCollection<GeocodingProviderVm>>
{
    public async Task<IReadOnlyCollection<GeocodingProviderVm>> Handle(GetGeocodingProvidersQuery request, CancellationToken cancellationToken)
    {
        var key = configuration["AppSettings:EncryptionKey"];
        Guard.Against.Null(key, message: "Credential key not found.");
        return await reader.GetGeocodingProvidersAsync(key, cancellationToken);
    }
}
