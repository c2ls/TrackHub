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

using System.Net;
using System.Text;
using TrackHub.Router.Domain.Records;
using TrackHub.Router.Infrastructure.Common.Geocoding;

namespace TrackHub.Router.Infrastructure.Tests;

/// <summary>
/// Response-mapping tests for the OpenRouteService (Pelias) reverse-geocoding adapter.
/// The adapter deliberately does NOT swallow transport failures — ReverseGeocodingService
/// wraps them into GeocodingUnavailableException — so only mapping and the null contract
/// are asserted here.
/// </summary>
[TestFixture]
public class OpenRouteServiceReverseGeocoderTests
{
    private const string OrsPayload = """
    {
      "type": "FeatureCollection",
      "features": [
        {
          "type": "Feature",
          "geometry": { "type": "Point", "coordinates": [-74.0721, 4.7110] },
          "properties": {
            "label": "Carrera 7 #71-21, Bogota, Colombia",
            "name": "Carrera 7 #71-21",
            "locality": "Bogota",
            "localadmin": "Chapinero",
            "region": "Bogota D.C.",
            "country": "Colombia"
          }
        }
      ]
    }
    """;

    private static GeocodingProviderConnectionDto Connection(string? apiKey = "ors-test-key", string? configurationJson = null)
        => new(
            GeocodingProviderId: Guid.NewGuid(),
            Type: (short)GeocodingProviderType.OpenRouteService,
            EndpointUri: "https://api.openrouteservice.org/",
            ApiKey: apiKey,
            RequestsPerSecond: 1,
            TimeoutSeconds: 5,
            ConfigurationJson: configurationJson);

    private static (OpenRouteServiceReverseGeocoder Geocoder, StubHandler Handler) CreateGeocoder(string payload, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new StubHandler(payload, status);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(handler, disposeHandler: false));
        return (new OpenRouteServiceReverseGeocoder(factory.Object), handler);
    }

    [Test]
    public void Type_IsOpenRouteService()
    {
        var (geocoder, _) = CreateGeocoder(OrsPayload);
        Assert.That(geocoder.Type, Is.EqualTo(GeocodingProviderType.OpenRouteService));
    }

    [Test]
    public async Task ResolveAsync_MapsPeliasPropertiesToAddressVm()
    {
        var (geocoder, handler) = CreateGeocoder(OrsPayload);

        var address = await geocoder.ResolveAsync(Connection(), 4.7110, -74.0721, CancellationToken.None);

        Assert.That(address, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(address!.Value.Address, Is.EqualTo("Carrera 7 #71-21, Bogota, Colombia"));
            // locality wins over localadmin for the city slot.
            Assert.That(address.Value.City, Is.EqualTo("Bogota"));
            Assert.That(address.Value.State, Is.EqualTo("Bogota D.C."));
            Assert.That(address.Value.Country, Is.EqualTo("Colombia"));
        });

        // The request targets ORS /geocode/reverse with invariant-culture coordinates and the key.
        Assert.That(handler.LastRequestUri, Is.Not.Null);
        var uri = handler.LastRequestUri!.ToString();
        Assert.Multiple(() =>
        {
            Assert.That(uri, Does.StartWith("https://api.openrouteservice.org/geocode/reverse?"));
            Assert.That(uri, Does.Contain("api_key=ors-test-key"));
            Assert.That(uri, Does.Contain("point.lat=4.711"));
            Assert.That(uri, Does.Contain("point.lon=-74.0721"));
            Assert.That(uri, Does.Contain("size=1"));
        });
    }

    [Test]
    public async Task ResolveAsync_FallsBackToLocaladmin_WhenLocalityMissing()
    {
        const string payload = """
        {
          "features": [
            { "properties": { "label": "Somewhere", "localadmin": "Chapinero", "country": "Colombia" } }
          ]
        }
        """;
        var (geocoder, _) = CreateGeocoder(payload);

        var address = await geocoder.ResolveAsync(Connection(), 1, 1, CancellationToken.None);

        Assert.That(address, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(address!.Value.City, Is.EqualTo("Chapinero"));
            Assert.That(address.Value.State, Is.Null);
        });
    }

    [Test]
    public async Task ResolveAsync_AppendsLanguage_WhenConfigured()
    {
        var (geocoder, handler) = CreateGeocoder(OrsPayload);

        await geocoder.ResolveAsync(Connection(configurationJson: """{ "language": "es" }"""), 1, 1, CancellationToken.None);

        Assert.That(handler.LastRequestUri!.ToString(), Does.Contain("lang=es"));
    }

    [Test]
    public async Task ResolveAsync_ReturnsNull_WhenFeaturesEmpty()
    {
        var (geocoder, _) = CreateGeocoder("""{ "type": "FeatureCollection", "features": [] }""");

        var address = await geocoder.ResolveAsync(Connection(), 1, 1, CancellationToken.None);

        Assert.That(address, Is.Null);
    }

    [Test]
    public async Task ResolveAsync_ReturnsNull_WhenLabelAndCountryAbsent()
    {
        const string payload = """{ "features": [ { "properties": { "locality": "Bogota" } } ] }""";
        var (geocoder, _) = CreateGeocoder(payload);

        var address = await geocoder.ResolveAsync(Connection(), 1, 1, CancellationToken.None);

        Assert.That(address, Is.Null);
    }

    [Test]
    public async Task ResolveAsync_ReturnsNullWithoutCallingOrs_WhenApiKeyMissing()
    {
        var (geocoder, handler) = CreateGeocoder(OrsPayload);

        var address = await geocoder.ResolveAsync(Connection(apiKey: "  "), 1, 1, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(address, Is.Null);
            Assert.That(handler.CallCount, Is.Zero);
        });
    }

    private sealed class StubHandler(string payload, HttpStatusCode status) : HttpMessageHandler
    {
        public Uri? LastRequestUri { get; private set; }
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            LastRequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            });
        }
    }
}
