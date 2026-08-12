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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TrackHub.Router.Domain.Exceptions;
using TrackHub.Router.Domain.Interfaces.Geocoding;
using TrackHub.Router.Domain.Interfaces.Manager;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Infrastructure.Common.Geocoding;

// Orchestrates reverse geocoding through the active GeocodingProvider row: fetches the
// provider from Manager (memory-cached briefly — config caching, not address caching),
// picks the adapter by type, and honors the provider's RequestsPerSecond throttle.
public sealed class ReverseGeocodingService(
    IGeocodingProviderReader providerReader,
    IEnumerable<IReverseGeocoder> geocoders,
    IConfiguration configuration,
    ILogger<ReverseGeocodingService> logger) : IReverseGeocodingService
{
    private const int DefaultEnrichmentBudget = 25;
    private static readonly TimeSpan ProviderCacheTtl = TimeSpan.FromSeconds(60);

    // Provider config cache and throttle state are process-wide (the service is scoped).
    private static readonly SemaphoreSlim CacheGate = new(1, 1);
    private static GeocodingProviderConnectionDto? _cachedConnection;
    private static string? _cachedConfigurationJson;
    private static DateTimeOffset _cacheExpiresAt = DateTimeOffset.MinValue;

    private static readonly SemaphoreSlim ThrottleGate = new(1, 1);
    private static DateTimeOffset _lastRequestAt = DateTimeOffset.MinValue;

    public async Task<AddressVm?> ResolveAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        var connection = await GetActiveConnectionAsync(cancellationToken)
            ?? throw new GeocodingUnavailableException("No active geocoding provider is configured.");

        var geocoder = geocoders.FirstOrDefault(g => (short)g.Type == connection.Type)
            ?? throw new GeocodingUnavailableException($"No geocoding adapter is registered for provider type {connection.Type}.");

        await ThrottleAsync(connection.RequestsPerSecond, cancellationToken);

        try
        {
            return await geocoder.ResolveAsync(connection, latitude, longitude, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Reverse geocoding failed for ({Latitude}, {Longitude}).", latitude, longitude);
            throw new GeocodingUnavailableException("The geocoding service is unavailable.");
        }
    }

    public async Task<AddressVm?> TryResolveAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        try
        {
            return await ResolveAsync(latitude, longitude, cancellationToken);
        }
        catch (GeocodingUnavailableException)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    public async Task<int> GetEnrichmentBudgetAsync(CancellationToken cancellationToken)
    {
        GeocodingProviderConnectionDto? connection;
        try
        {
            connection = await GetActiveConnectionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not resolve the active geocoding provider for enrichment.");
            return 0;
        }

        if (connection is null)
        {
            return 0;
        }

        var configurationJson = _cachedConfigurationJson;
        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            return DefaultEnrichmentBudget;
        }

        try
        {
            using var document = JsonDocument.Parse(configurationJson);
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("enrichmentBudget", out var budgetElement)
                && budgetElement.TryGetInt32(out var budget))
            {
                return Math.Max(0, budget);
            }
        }
        catch (JsonException)
        {
            // Malformed extras never disable enrichment; fall through to the default.
        }

        return DefaultEnrichmentBudget;
    }

    private async Task<GeocodingProviderConnectionDto?> GetActiveConnectionAsync(CancellationToken cancellationToken)
    {
        if (_cacheExpiresAt > DateTimeOffset.UtcNow)
        {
            return _cachedConnection;
        }

        await CacheGate.WaitAsync(cancellationToken);
        try
        {
            if (_cacheExpiresAt > DateTimeOffset.UtcNow)
            {
                return _cachedConnection;
            }

            var provider = await providerReader.GetActiveGeocodingProviderAsync(cancellationToken);
            if (provider is null || provider.Value.GeocodingProviderId == Guid.Empty)
            {
                _cachedConnection = null;
                _cachedConfigurationJson = null;
            }
            else
            {
                var vm = provider.Value;
                _cachedConnection = new GeocodingProviderConnectionDto(
                    vm.GeocodingProviderId,
                    vm.Type,
                    vm.EndpointUri,
                    DecryptApiKey(vm.ApiKey, vm.Salt),
                    vm.RequestsPerSecond,
                    vm.TimeoutSeconds,
                    vm.ConfigurationJson);
                _cachedConfigurationJson = vm.ConfigurationJson;
            }

            _cacheExpiresAt = DateTimeOffset.UtcNow.Add(ProviderCacheTtl);
            return _cachedConnection;
        }
        finally
        {
            CacheGate.Release();
        }
    }

    private string? DecryptApiKey(string? apiKey, string? salt)
    {
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(salt))
        {
            return null;
        }

        var key = configuration["AppSettings:EncryptionKey"];
        Guard.Against.Null(key, message: "Credential key not found.");
        return apiKey.DecryptData(key, Convert.FromBase64String(salt));
    }

    private static async Task ThrottleAsync(int requestsPerSecond, CancellationToken cancellationToken)
    {
        var minInterval = TimeSpan.FromSeconds(1d / Math.Max(1, requestsPerSecond));

        await ThrottleGate.WaitAsync(cancellationToken);
        try
        {
            var wait = _lastRequestAt + minInterval - DateTimeOffset.UtcNow;
            if (wait > TimeSpan.Zero)
            {
                await Task.Delay(wait, cancellationToken);
            }
            _lastRequestAt = DateTimeOffset.UtcNow;
        }
        finally
        {
            ThrottleGate.Release();
        }
    }
}
