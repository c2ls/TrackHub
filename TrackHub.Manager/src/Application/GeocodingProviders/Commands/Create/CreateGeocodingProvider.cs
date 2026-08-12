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

namespace TrackHub.Manager.Application.GeocodingProviders.Commands.Create;

[Authorize(Resource = Resources.GeocodingProviders, Action = Actions.Write)]
public readonly record struct CreateGeocodingProviderCommand(GeocodingProviderDto GeocodingProvider) : IRequest<GeocodingProviderVm>;

public class CreateGeocodingProviderCommandHandler(IGeocodingProviderWriter writer, IConfiguration configuration) : IRequestHandler<CreateGeocodingProviderCommand, GeocodingProviderVm>
{
    public async Task<GeocodingProviderVm> Handle(CreateGeocodingProviderCommand request, CancellationToken cancellationToken)
    {
        var key = configuration["AppSettings:EncryptionKey"];
        Guard.Against.Null(key, message: "Credential key not found.");
        var salt = CryptographyExtensions.GenerateAesKey(256);
        return await writer.CreateGeocodingProviderAsync(request.GeocodingProvider, salt, key, cancellationToken);
    }
}
