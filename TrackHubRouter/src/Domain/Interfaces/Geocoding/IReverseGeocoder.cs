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

using Common.Domain.Enums;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Domain.Interfaces.Geocoding;

// One adapter per GeocodingProviderType, mirroring the operator-adapter pattern:
// a future ORS/Google integration is a new adapter plus a provider row, no call-site changes.
public interface IReverseGeocoder
{
    GeocodingProviderType Type { get; }
    Task<AddressVm?> ResolveAsync(GeocodingProviderConnectionDto connection, double latitude, double longitude, CancellationToken cancellationToken);
}
