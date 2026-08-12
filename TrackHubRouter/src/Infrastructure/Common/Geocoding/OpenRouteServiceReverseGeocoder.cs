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

using System.Globalization;
using Common.Domain.Enums;
using TrackHub.Router.Domain.Interfaces.Geocoding;
using TrackHub.Router.Domain.Models;
using TrackHub.Router.Domain.Records;

namespace TrackHub.Router.Infrastructure.Common.Geocoding;

// Reverse geocoding against OpenRouteService (Pelias). Provider connection data comes from the
// persisted GeocodingProvider row — the ApiKey arrives already decrypted from
// ReverseGeocodingService — and ConfigurationJson supports a "language" extra.
// Shares the "Geocoding" named HttpClient with Nominatim: only one provider is active at a time.
public sealed class OpenRouteServiceReverseGeocoder(IHttpClientFactory httpClientFactory) : IReverseGeocoder
{
    public GeocodingProviderType Type => GeocodingProviderType.OpenRouteService;

    public async Task<AddressVm?> ResolveAsync(GeocodingProviderConnectionDto connection, double latitude, double longitude, CancellationToken cancellationToken)
    {
        // An ORS row without a key cannot resolve anything; treat it as "no address found"
        // rather than hammering the endpoint with a guaranteed 403.
        if (string.IsNullOrWhiteSpace(connection.ApiKey))
        {
            return null;
        }

        var baseUri = connection.EndpointUri.TrimEnd('/');
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        var requestUri = $"{baseUri}/geocode/reverse?api_key={Uri.EscapeDataString(connection.ApiKey)}&point.lat={lat}&point.lon={lon}&size=1";

        var language = ReadConfiguration(connection.ConfigurationJson);
        if (!string.IsNullOrWhiteSpace(language))
        {
            requestUri += $"&lang={Uri.EscapeDataString(language)}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var client = httpClientFactory.CreateClient(NominatimReverseGeocoder.HttpClientName);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(connection.TimeoutSeconds <= 0 ? 5 : connection.TimeoutSeconds));

        using var response = await client.SendAsync(request, timeout.Token);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: timeout.Token);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("features", out var features)
            || features.ValueKind != JsonValueKind.Array
            || features.GetArrayLength() == 0)
        {
            return null;
        }

        var first = features[0];
        if (first.ValueKind != JsonValueKind.Object
            || !first.TryGetProperty("properties", out var properties)
            || properties.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var label = GetFirst(properties, "label", "name");
        var city = GetFirst(properties, "locality", "localadmin", "county");
        var state = GetFirst(properties, "region", "macroregion");
        var country = GetFirst(properties, "country");

        return string.IsNullOrWhiteSpace(label) && country is null
            ? null
            : new AddressVm(label, city, state, country);
    }

    private static string? GetFirst(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }
        }
        return null;
    }

    private static string? ReadConfiguration(string? configurationJson)
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(configurationJson);
            return document.RootElement.ValueKind == JsonValueKind.Object
                ? GetFirst(document.RootElement, "language")
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
