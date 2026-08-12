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

using Common.Domain.Extensions;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

public sealed class GeocodingProviderReader(IApplicationDbContext context) : IGeocodingProviderReader
{
    // ApiKey is decrypted here because this surface is restricted to the
    // SuperAdministrator CRUD editor (authorized-CRUD exception).
    public async Task<IReadOnlyCollection<GeocodingProviderVm>> GetGeocodingProvidersAsync(string key, CancellationToken cancellationToken)
    {
        var providers = await context.GeocodingProviders
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return [.. providers.Select(p => new GeocodingProviderVm(
            p.GeocodingProviderId,
            p.Name,
            p.Type,
            p.EndpointUri,
            Decrypt(p.ApiKey, p.Salt, key),
            p.RequestsPerSecond,
            p.TimeoutSeconds,
            p.ConfigurationJson,
            p.Active))];
    }

    // The Router service client receives the encrypted ApiKey plus its Salt and
    // decrypts with the shared encryption key (operator-credential pattern).
    public async Task<GeocodingProviderTokenVm?> GetActiveGeocodingProviderAsync(CancellationToken cancellationToken)
        => await context.GeocodingProviders
            .Where(p => p.Active)
            .Select(p => (GeocodingProviderTokenVm?)new GeocodingProviderTokenVm(
                p.GeocodingProviderId,
                p.Name,
                p.Type,
                p.EndpointUri,
                p.ApiKey,
                p.Salt,
                p.RequestsPerSecond,
                p.TimeoutSeconds,
                p.ConfigurationJson))
            .FirstOrDefaultAsync(cancellationToken);

    private static string? Decrypt(string? encrypted, string? salt, string key)
        => string.IsNullOrEmpty(encrypted) || string.IsNullOrEmpty(salt)
            ? null
            : encrypted.DecryptData(key, Convert.FromBase64String(salt));
}
