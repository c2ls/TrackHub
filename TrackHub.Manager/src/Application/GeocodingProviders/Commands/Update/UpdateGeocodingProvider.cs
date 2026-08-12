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
using Common.Domain.Extensions;
using Microsoft.Extensions.Configuration;

namespace TrackHub.Manager.Application.GeocodingProviders.Commands.Update;

[Authorize(Resource = Resources.GeocodingProviders, Action = Actions.Edit)]
[PlatformScoped("Platform geocoding-provider registry: one active provider serves every tenant, administered from the Administrator console; no tenant owns a row.")]
public readonly record struct UpdateGeocodingProviderCommand(Guid Id, UpdateGeocodingProviderDto GeocodingProvider) : IRequest;

public class UpdateGeocodingProviderCommandHandler(IGeocodingProviderWriter writer, IConfiguration configuration) : IRequestHandler<UpdateGeocodingProviderCommand>
{
    public async Task Handle(UpdateGeocodingProviderCommand request, CancellationToken cancellationToken)
    {
        var key = configuration["AppSettings:EncryptionKey"];
        Guard.Against.Null(key, message: "Credential key not found.");
        var salt = CryptographyExtensions.GenerateAesKey(256);
        await writer.UpdateGeocodingProviderAsync(request.Id, request.GeocodingProvider, salt, key, cancellationToken);
    }
}
