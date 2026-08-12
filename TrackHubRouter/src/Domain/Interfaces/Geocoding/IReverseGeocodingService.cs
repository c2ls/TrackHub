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

using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Domain.Interfaces.Geocoding;

public interface IReverseGeocodingService
{
    /// <summary>
    /// Resolves a coordinate through the active provider. Throws
    /// <see cref="Exceptions.GeocodingUnavailableException"/> when no provider is active
    /// or the geocoder cannot be reached; returns null when no address was found.
    /// </summary>
    Task<AddressVm?> ResolveAsync(double latitude, double longitude, CancellationToken cancellationToken);

    /// <summary>
    /// Best-effort resolution for ingestion-time enrichment: never throws, returns null
    /// on any failure. Honors the provider's RequestsPerSecond throttle.
    /// </summary>
    Task<AddressVm?> TryResolveAsync(double latitude, double longitude, CancellationToken cancellationToken);

    /// <summary>
    /// Per-cycle enrichment budget from the active provider's ConfigurationJson
    /// ("enrichmentBudget", default 25; 0 disables). Returns 0 when no provider is active.
    /// </summary>
    Task<int> GetEnrichmentBudgetAsync(CancellationToken cancellationToken);
}
