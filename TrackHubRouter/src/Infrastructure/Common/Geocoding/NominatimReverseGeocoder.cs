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

// Reverse geocoding against a Nominatim instance (the platform's internal instance by
// default). Provider connection data comes from the persisted GeocodingProvider row;
// ConfigurationJson supports "userAgent", "email", and "language" extras.
public sealed class NominatimReverseGeocoder(IHttpClientFactory httpClientFactory) : IReverseGeocoder
{
    public const string HttpClientName = "Geocoding";

    public GeocodingProviderType Type => GeocodingProviderType.Nominatim;

    public async Task<AddressVm?> ResolveAsync(GeocodingProviderConnectionDto connection, double latitude, double longitude, CancellationToken cancellationToken)
    {
        var baseUri = connection.EndpointUri.TrimEnd('/');
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        var requestUri = $"{baseUri}/reverse?format=jsonv2&lat={lat}&lon={lon}&addressdetails=1";

        var (userAgent, email, language) = ReadConfiguration(connection.ConfigurationJson);
        if (!string.IsNullOrWhiteSpace(email))
        {
            requestUri += $"&email={Uri.EscapeDataString(email)}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.TryAddWithoutValidation("User-Agent", string.IsNullOrWhiteSpace(userAgent) ? "TrackHub" : userAgent);
        if (!string.IsNullOrWhiteSpace(language))
        {
            request.Headers.TryAddWithoutValidation("Accept-Language", language);
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(connection.TimeoutSeconds <= 0 ? 5 : connection.TimeoutSeconds));

        using var response = await client.SendAsync(request, timeout.Token);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: timeout.Token);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object || root.TryGetProperty("error", out _))
        {
            return null;
        }

        var displayName = root.TryGetProperty("display_name", out var displayNameElement)
            ? displayNameElement.GetString()
            : null;

        string? city = null, state = null, country = null;
        if (root.TryGetProperty("address", out var address) && address.ValueKind == JsonValueKind.Object)
        {
            city = GetFirst(address, "city", "town", "village", "municipality", "hamlet");
            state = GetFirst(address, "state", "region", "province");
            country = GetFirst(address, "country");
        }

        return string.IsNullOrWhiteSpace(displayName) && country is null
            ? null
            : new AddressVm(displayName, city, state, country);
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

    private static (string? UserAgent, string? Email, string? Language) ReadConfiguration(string? configurationJson)
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            return (null, null, null);
        }

        try
        {
            using var document = JsonDocument.Parse(configurationJson);
            var root = document.RootElement;
            return (GetFirst(root, "userAgent"), GetFirst(root, "email"), GetFirst(root, "language"));
        }
        catch (JsonException)
        {
            return (null, null, null);
        }
    }
}
