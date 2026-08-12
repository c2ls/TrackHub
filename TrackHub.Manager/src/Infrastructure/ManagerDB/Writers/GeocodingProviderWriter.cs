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

using System.Text.Json;
using Common.Application.Interfaces;
using Common.Domain.Extensions;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Writers;

// Geocoding providers are platform infrastructure (no AccountId); audit events are
// recorded against the empty account id. ApiKey never appears in audit payloads.
public sealed class GeocodingProviderWriter(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IGeocodingProviderWriter
{
    public async Task<GeocodingProviderVm> CreateGeocodingProviderAsync(GeocodingProviderDto geocodingProviderDto, byte[] salt, string key, CancellationToken cancellationToken)
    {
        if (geocodingProviderDto.Active)
        {
            await DeactivateAllAsync(cancellationToken);
        }

        var provider = new GeocodingProvider(
            geocodingProviderDto.Name,
            geocodingProviderDto.Type,
            geocodingProviderDto.EndpointUri,
            geocodingProviderDto.ApiKey?.EncryptData(key, salt),
            geocodingProviderDto.ApiKey is null ? null : Convert.ToBase64String(salt),
            geocodingProviderDto.RequestsPerSecond,
            geocodingProviderDto.TimeoutSeconds,
            geocodingProviderDto.ConfigurationJson,
            geocodingProviderDto.Active);

        await Context.GeocodingProviders.AddAsync(provider, cancellationToken);

        AddAuditEvent(
            Guid.Empty,
            "Create",
            nameof(GeocodingProvider),
            provider.GeocodingProviderId.ToString(),
            null,
            SerializeForAudit(provider));

        await Context.SaveChangesAsync(cancellationToken);

        return new GeocodingProviderVm(
            provider.GeocodingProviderId,
            provider.Name,
            provider.Type,
            provider.EndpointUri,
            geocodingProviderDto.ApiKey,
            provider.RequestsPerSecond,
            provider.TimeoutSeconds,
            provider.ConfigurationJson,
            provider.Active);
    }

    public async Task UpdateGeocodingProviderAsync(Guid geocodingProviderId, UpdateGeocodingProviderDto geocodingProviderDto, byte[] salt, string key, CancellationToken cancellationToken)
    {
        var provider = await Context.GeocodingProviders.FindAsync([geocodingProviderId], cancellationToken)
            ?? throw new NotFoundException(nameof(GeocodingProvider), $"{geocodingProviderId}");

        Context.GeocodingProviders.Attach(provider);

        var oldValues = SerializeForAudit(provider);

        provider.Name = geocodingProviderDto.Name;
        provider.Type = geocodingProviderDto.Type;
        provider.EndpointUri = geocodingProviderDto.EndpointUri;
        provider.RequestsPerSecond = geocodingProviderDto.RequestsPerSecond;
        provider.TimeoutSeconds = geocodingProviderDto.TimeoutSeconds;
        provider.ConfigurationJson = geocodingProviderDto.ConfigurationJson;

        if (geocodingProviderDto.ApiKey is not null)
        {
            provider.ApiKey = geocodingProviderDto.ApiKey.EncryptData(key, salt);
            provider.Salt = Convert.ToBase64String(salt);
        }

        AddAuditEvent(
            Guid.Empty,
            "Update",
            nameof(GeocodingProvider),
            geocodingProviderId.ToString(),
            oldValues,
            SerializeForAudit(provider));

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteGeocodingProviderAsync(Guid geocodingProviderId, CancellationToken cancellationToken)
    {
        var provider = await Context.GeocodingProviders.FindAsync([geocodingProviderId], cancellationToken)
            ?? throw new NotFoundException(nameof(GeocodingProvider), $"{geocodingProviderId}");

        Context.GeocodingProviders.Attach(provider);
        Context.GeocodingProviders.Remove(provider);

        AddAuditEvent(
            Guid.Empty,
            "Delete",
            nameof(GeocodingProvider),
            geocodingProviderId.ToString(),
            SerializeForAudit(provider),
            null);

        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetActiveGeocodingProviderAsync(Guid geocodingProviderId, CancellationToken cancellationToken)
    {
        var provider = await Context.GeocodingProviders.FindAsync([geocodingProviderId], cancellationToken)
            ?? throw new NotFoundException(nameof(GeocodingProvider), $"{geocodingProviderId}");

        await DeactivateAllAsync(cancellationToken);

        Context.GeocodingProviders.Attach(provider);
        provider.Active = true;

        AddAuditEvent(
            Guid.Empty,
            "Activate",
            nameof(GeocodingProvider),
            geocodingProviderId.ToString(),
            null,
            SerializeForAudit(provider));

        await Context.SaveChangesAsync(cancellationToken);
    }

    private async Task DeactivateAllAsync(CancellationToken cancellationToken)
    {
        var activeProviders = await Context.GeocodingProviders
            .Where(p => p.Active)
            .ToListAsync(cancellationToken);

        foreach (var activeProvider in activeProviders)
        {
            Context.GeocodingProviders.Attach(activeProvider);
            activeProvider.Active = false;
        }
    }

    private static string SerializeForAudit(GeocodingProvider provider)
        => JsonSerializer.Serialize(new
        {
            provider.Name,
            provider.Type,
            provider.EndpointUri,
            provider.RequestsPerSecond,
            provider.TimeoutSeconds,
            provider.Active
        });
}
