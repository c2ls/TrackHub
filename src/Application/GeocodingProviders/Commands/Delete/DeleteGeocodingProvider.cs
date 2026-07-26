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

namespace TrackHub.Manager.Application.GeocodingProviders.Commands.Delete;

[Authorize(Resource = Resources.GeocodingProviders, Action = Actions.Delete)]
[PlatformScoped("Platform geocoding-provider registry: one active provider serves every tenant, administered from the Administrator console; no tenant owns a row.")]
public readonly record struct DeleteGeocodingProviderCommand(Guid Id) : IRequest;

public class DeleteGeocodingProviderCommandHandler(IGeocodingProviderWriter writer) : IRequestHandler<DeleteGeocodingProviderCommand>
{
    public async Task Handle(DeleteGeocodingProviderCommand request, CancellationToken cancellationToken)
        => await writer.DeleteGeocodingProviderAsync(request.Id, cancellationToken);
}
