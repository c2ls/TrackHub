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

using Common.Domain.Constants;
using Common.Domain.Enums;

namespace TrackHub.Manager.Application.GeocodingProviders.Commands.Create;

public sealed class CreateGeocodingProviderValidator : AbstractValidator<CreateGeocodingProviderCommand>
{
    public CreateGeocodingProviderValidator()
    {
        RuleFor(v => v.GeocodingProvider.Name)
            .NotEmpty()
            .MaximumLength(ColumnMetadata.DefaultNameLength);

        RuleFor(v => v.GeocodingProvider.EndpointUri)
            .NotEmpty()
            .MaximumLength(ColumnMetadata.DefaultDescriptionLength)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("EndpointUri must be an absolute URI.");

        RuleFor(v => v.GeocodingProvider.Type)
            .Must(type => Enum.IsDefined(typeof(GeocodingProviderType), (int)type))
            .WithMessage("Unknown geocoding provider type.");

        RuleFor(v => v.GeocodingProvider.RequestsPerSecond)
            .InclusiveBetween(1, 100);

        RuleFor(v => v.GeocodingProvider.TimeoutSeconds)
            .InclusiveBetween(1, 60);
    }
}
