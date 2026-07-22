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
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TrackHub.TripManagement.Domain.Constants;
using TrackHub.TripManagement.Domain.Exceptions;
using TrackHub.TripManagement.Domain.Models;
using TrackHub.TripManagement.Infrastructure.RoutingApi;

namespace Infrastructure.UnitTests;

/// <summary>
/// The routing adapter — the module's only external integration, and the one place a third party
/// can inject arbitrary failure into a trip command.
/// <para>
/// <b>Why this fixture exists:</b> <c>PlanTripRouteCommandHandler</c> catches ONLY
/// <see cref="RoutingUnavailableException"/>. Every other exception escapes to the GraphQL layer as
/// an unhandled 500 and the trip is left with no route plan at all instead of a <c>Failed</c> one,
/// which is precisely what acceptance 18 forbids. "Nothing else escapes the adapter" was therefore
/// the entire basis of that acceptance and was asserted nowhere: a raw <see cref="JsonException"/>
/// from a truncated body, a socket failure or a <see cref="TaskCanceledException"/> from the client
/// timeout would each have shipped as a 500.
/// </para>
/// <para>
/// The other half is the wire contract. ORS speaks <b>lon,lat</b> and everything above this adapter
/// speaks lat,lng; a flip is silent, survives every mock-based test, and puts the route in the
/// wrong hemisphere. Both directions of that flip are asserted here against a real payload.
/// </para>
/// </summary>
[TestFixture]
public class OpenRouteServiceRoutingProviderTests
{
    private static readonly IReadOnlyCollection<CoordinateVm> TwoWaypoints =
    [
        new CoordinateVm(4.60971, -74.08175),
        new CoordinateVm(4.70000, -74.10000),
    ];

    /// <summary>A realistic ORS geojson answer: two geometry vertices and two segments.</summary>
    private const string GeoJsonBody = """
    {
      "type": "FeatureCollection",
      "features": [
        {
          "geometry": {
            "type": "LineString",
            "coordinates": [[-74.08175, 4.60971], [-74.09000, 4.65000], [-74.10000, 4.70000]]
          },
          "properties": {
            "summary": { "distance": 15234.7, "duration": 1830.4 },
            "segments": [
              { "distance": 8000.5, "duration": 900.2 },
              { "distance": 7234.2, "duration": 930.2 }
            ]
          }
        }
      ]
    }
    """;

    // ----- The wire contract -------------------------------------------------------------------

    [Test]
    public async Task GetRoute_ParsesTheGeoJsonAndFlipsOrsLonLatBackToLatLng()
    {
        var stub = StubHandler.Returning(Ok(GeoJsonBody));
        var provider = Provider(stub);

        var route = await provider.GetRouteAsync(TwoWaypoints, CancellationToken.None);

        var geometry = route.Geometry.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(geometry, Has.Count.EqualTo(3));

            // ORS wrote [-74.08175, 4.60971]. Bogota is at latitude 4.6, NOT -74: a flip here puts
            // every planned route in the Southern Ocean and no mock-based test can see it.
            Assert.That(geometry[0].Latitude, Is.EqualTo(4.60971).Within(1e-9));
            Assert.That(geometry[0].Longitude, Is.EqualTo(-74.08175).Within(1e-9));
            Assert.That(geometry[2].Latitude, Is.EqualTo(4.70000).Within(1e-9));
            Assert.That(geometry[2].Longitude, Is.EqualTo(-74.10000).Within(1e-9));
        });
    }

    [Test]
    public async Task GetRoute_ReadsTheRouteSummaryAndRoundsTheDuration()
    {
        var provider = Provider(StubHandler.Returning(Ok(GeoJsonBody)));

        var route = await provider.GetRouteAsync(TwoWaypoints, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(route.DistanceMeters, Is.EqualTo(15234.7).Within(1e-6));
            Assert.That(route.DurationSeconds, Is.EqualTo(1830), "1830.4 s rounds to whole seconds");
        });
    }

    // The per-leg breakdown is what drives per-stop planned arrival times: collapsing it loses the
    // schedule for every intermediate stop while the total distance still looks right.
    [Test]
    public async Task GetRoute_BreaksTheSegmentsOutIntoIndexedLegs()
    {
        var provider = Provider(StubHandler.Returning(Ok(GeoJsonBody)));

        var route = await provider.GetRouteAsync(TwoWaypoints, CancellationToken.None);

        var legs = route.Legs.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(legs, Has.Count.EqualTo(2));
            Assert.That(legs[0].Index, Is.EqualTo(0));
            Assert.That(legs[0].DistanceMeters, Is.EqualTo(8000.5).Within(1e-6));
            Assert.That(legs[0].DurationSeconds, Is.EqualTo(900));
            Assert.That(legs[1].Index, Is.EqualTo(1), "leg order is the stop order; an unindexed leg cannot be matched to a stop");
            Assert.That(legs[1].DistanceMeters, Is.EqualTo(7234.2).Within(1e-6));
            Assert.That(legs[1].DurationSeconds, Is.EqualTo(930));
        });
    }

    // The outbound half of the same flip. ORS silently routes whatever coordinates it is given, so
    // a request written lat,lon comes back as a perfectly well-formed route to the wrong place.
    [Test]
    public async Task GetRoute_SendsCoordinatesToOrsInLonLatOrder()
    {
        var stub = StubHandler.Returning(Ok(GeoJsonBody));
        var provider = Provider(stub);

        await provider.GetRouteAsync(TwoWaypoints, CancellationToken.None);

        using var sent = JsonDocument.Parse(stub.LastRequestBody!);
        var first = sent.RootElement.GetProperty("coordinates")[0];
        Assert.Multiple(() =>
        {
            Assert.That(first[0].GetDouble(), Is.EqualTo(-74.08175).Within(1e-9), "ORS takes longitude first");
            Assert.That(first[1].GetDouble(), Is.EqualTo(4.60971).Within(1e-9));
        });
    }

    [Test]
    public async Task GetSummary_ReadsDistanceAndDurationWithoutAskingForGeometry()
    {
        var stub = StubHandler.Returning(Ok(GeoJsonBody));
        var provider = Provider(stub);

        var summary = await provider.GetSummaryAsync(
            new CoordinateVm(4.60971, -74.08175), new CoordinateVm(4.7, -74.1), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(summary.DistanceMeters, Is.EqualTo(15234.7).Within(1e-6));
            Assert.That(summary.DurationSeconds, Is.EqualTo(1830));
        });
    }

    // ----- Configuration -----------------------------------------------------------------------

    [Test]
    public void GetRoute_WithoutAnApiKey_FailsWithRoutingNotConfiguredBeforeAnyCall()
    {
        // A deployment that never set AppSettings:Routing:ApiKey must get a distinct, actionable
        // code — ROUTING_UNAVAILABLE would send an operator hunting a provider outage that is
        // really a missing environment variable.
        var stub = StubHandler.Returning(Ok(GeoJsonBody));
        var provider = Provider(stub, apiKey: null);

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingNotConfigured));
            Assert.That(stub.CallCount, Is.Zero, "an unconfigured provider must not put a request on the wire");
        });
    }

    [Test]
    public void GetRoute_WithoutABaseUrl_FailsWithRoutingNotConfigured()
    {
        var provider = Provider(StubHandler.Returning(Ok(GeoJsonBody)), baseUrl: null);

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingNotConfigured));
    }

    [Test]
    public void IsConfigured_IsFalseUntilBothTheBaseUrlAndTheKeyArePresent()
    {
        // TripEtaService reads this flag to decide whether to spend a call at all.
        Assert.Multiple(() =>
        {
            Assert.That(Provider(StubHandler.Returning(Ok(GeoJsonBody)), apiKey: null).IsConfigured, Is.False);
            Assert.That(Provider(StubHandler.Returning(Ok(GeoJsonBody)), baseUrl: null).IsConfigured, Is.False);
            Assert.That(Provider(StubHandler.Returning(Ok(GeoJsonBody))).IsConfigured, Is.True);
        });
    }

    // ----- Waypoint bounds ---------------------------------------------------------------------

    [Test]
    public void GetRoute_AboveMaxWaypoints_IsRejectedBeforeAnyCallGoesOut()
    {
        // The ceiling exists to stop a 300-stop trip from burning the vendor quota on a request ORS
        // would reject anyway; enforcing it after the call would defeat the point.
        var stub = StubHandler.Returning(Ok(GeoJsonBody));
        var provider = Provider(stub, maxWaypoints: 2);

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(() => provider.GetRouteAsync(
            [new CoordinateVm(4.6, -74.0), new CoordinateVm(4.7, -74.1), new CoordinateVm(4.8, -74.2)],
            CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
            Assert.That(stub.CallCount, Is.Zero, "the ceiling must be enforced before the request, not after");
        });
    }

    [Test]
    public void GetRoute_AtExactlyMaxWaypoints_IsAllowed()
    {
        // An off-by-one on the ceiling silently caps every fleet at MaxWaypoints - 1 stops.
        var stub = StubHandler.Returning(Ok(GeoJsonBody));
        var provider = Provider(stub, maxWaypoints: 2);

        Assert.DoesNotThrowAsync(() => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));
        Assert.That(stub.CallCount, Is.EqualTo(1));
    }

    [Test]
    public void GetRoute_WithASingleWaypoint_IsRejected()
    {
        var stub = StubHandler.Returning(Ok(GeoJsonBody));
        var provider = Provider(stub);

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync([new CoordinateVm(4.6, -74.0)], CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
            Assert.That(stub.CallCount, Is.Zero);
        });
    }

    // ----- Failure containment: acceptance 18 --------------------------------------------------

    [Test]
    public void GetRoute_OnAClientTimeout_SurfacesRoutingUnavailableRatherThanTaskCanceled()
    {
        // HttpClient.Timeout surfaces as TaskCanceledException with an UNCANCELLED token. That is
        // not an OperationCanceledException the caller asked for, so it must be translated — the
        // planning handler does not catch it and the trip would end up with no route plan at all.
        var stub = StubHandler.Throwing(() => new TaskCanceledException("The request timed out.", new TimeoutException()));
        var provider = Provider(stub);

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
    }

    [Test]
    public void GetRoute_OnASocketFailure_SurfacesRoutingUnavailable()
    {
        var stub = StubHandler.Throwing(() => new HttpRequestException("No such host is known."));
        var provider = Provider(stub);

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
    }

    [Test]
    public void GetRoute_OnAFourHundredResponse_SurfacesRoutingUnavailableAndDoesNotRetry()
    {
        // A 4xx is the adapter's own fault (bad profile, bad key, bad coordinates). Retrying it
        // spends the vendor quota three times over to get the same answer.
        var stub = StubHandler.Returning(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"error\":\"Invalid profile\"}", Encoding.UTF8, "application/json"),
        });
        var provider = Provider(stub);

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
            Assert.That(stub.CallCount, Is.EqualTo(1), "a 4xx is deterministic; retrying it only burns quota");
        });
    }

    [Test]
    public void GetRoute_OnAFourHundredOne_SurfacesRoutingUnavailableRatherThanLeakingTheKeyProblemAsAnUnhandledError()
    {
        var stub = StubHandler.Returning(new HttpResponseMessage(HttpStatusCode.Unauthorized));
        var provider = Provider(stub);

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
    }

    // ----- The bounded retry -------------------------------------------------------------------

    [Test]
    public async Task GetRoute_RetriesARateLimitAndSucceedsOnALaterAttempt()
    {
        // ORS rate-limits aggressively and a directions call has no side effect at the provider, so
        // a repeat is safe. Not retrying makes a single 429 fail a trip's whole route plan.
        var stub = StubHandler.ReturningInOrder(
            new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            Ok(GeoJsonBody));
        var provider = Provider(stub);

        var route = await provider.GetRouteAsync(TwoWaypoints, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(route.Geometry, Is.Not.Empty);
            Assert.That(stub.CallCount, Is.EqualTo(3));
        });
    }

    [Test]
    public void GetRoute_StopsRetryingAfterThreeAttempts()
    {
        // The bound is the point. An unbounded retry against a provider having a bad hour holds the
        // request (and its DB context) open indefinitely and multiplies the load on a service that
        // is already failing — the trip is far better served by a Failed route plan it can retry.
        var stub = StubHandler.Returning(() => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var provider = Provider(stub);

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
            Assert.That(stub.CallCount, Is.EqualTo(3), "three attempts, then give up — never a retry loop");
        });
    }

    [Test]
    public void GetRoute_DoesNotRetryATransportFailure()
    {
        // A thrown request is translated immediately: the retry budget is spent on responses the
        // provider actually produced, not on a resolver or socket problem that will not change.
        var stub = StubHandler.Throwing(() => new HttpRequestException("Connection refused."));
        var provider = Provider(stub);

        Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.That(stub.CallCount, Is.EqualTo(1));
    }

    // ----- Malformed and unusable payloads -----------------------------------------------------

    [Test]
    public void GetRoute_OnAMalformedBody_DoesNotLeakARawJsonException()
    {
        // A truncated body from a proxy is the realistic case. JsonException is not caught anywhere
        // above this adapter, so leaking it is an unhandled 500 on a trip command.
        var provider = Provider(StubHandler.Returning(Ok("{\"features\": [{\"geometry\":")));

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
    }

    [Test]
    public void GetRoute_OnAnEmptyBody_DoesNotLeakARawJsonException()
    {
        var provider = Provider(StubHandler.Returning(Ok(string.Empty)));

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
    }

    [Test]
    public void GetRoute_OnAResponseWithNoFeatures_SurfacesRoutingUnavailable()
    {
        // ORS answers 200 with an empty feature collection when it cannot route between the points
        // (an island, or a profile that cannot use the only road). Indexing features[0] on that
        // body is an IndexOutOfRangeException, which nothing above catches.
        var provider = Provider(StubHandler.Returning(Ok("{\"type\":\"FeatureCollection\",\"features\":[]}")));

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
    }

    [Test]
    public void GetRoute_OnAFeatureWithNoGeometry_SurfacesRoutingUnavailable()
    {
        // An empty geometry would be stored as a route plan with nothing to draw and no corridor,
        // which then makes IsInsideCorridorAsync answer null for every fix on that trip.
        var provider = Provider(StubHandler.Returning(Ok("""
            {"features":[{"properties":{"summary":{"distance":1,"duration":1}}}]}
            """)));

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
    }

    [Test]
    public void GetRoute_OnAFeatureWithNoSummary_SurfacesRoutingUnavailable()
    {
        // Silently defaulting distance/duration to zero would publish a route plan claiming a
        // 0 m / 0 s journey, and every ETA derived from it would say "arriving now".
        var provider = Provider(StubHandler.Returning(Ok("""
            {"features":[{"geometry":{"coordinates":[[-74.08,4.60],[-74.10,4.70]]},"properties":{}}]}
            """)));

        var ex = Assert.ThrowsAsync<RoutingUnavailableException>(
            () => provider.GetRouteAsync(TwoWaypoints, CancellationToken.None));

        Assert.That(ex!.ErrorCode, Is.EqualTo(TripErrorCodes.RoutingUnavailable));
    }

    [Test]
    public async Task GetRoute_OnAFeatureWithNoSegments_StillReturnsTheRouteWithNoLegs()
    {
        // A single-leg answer legitimately carries no segments array. That is a route without a
        // per-stop breakdown, not a failure — rejecting it would fail every two-point plan.
        var provider = Provider(StubHandler.Returning(Ok("""
            {"features":[{"geometry":{"coordinates":[[-74.08,4.60],[-74.10,4.70]]},
             "properties":{"summary":{"distance":1200,"duration":300}}}]}
            """)));

        var route = await provider.GetRouteAsync(TwoWaypoints, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(route.Legs, Is.Empty);
            Assert.That(route.DistanceMeters, Is.EqualTo(1200d));
        });
    }

    // ----- Fixture plumbing --------------------------------------------------------------------

    private static HttpResponseMessage Ok(string body)
        => new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private static OpenRouteServiceRoutingProvider Provider(
        StubHandler handler,
        string? baseUrl = "https://routing.test",
        string? apiKey = "test-key",
        int maxWaypoints = 50)
    {
        var factory = new StubHttpClientFactory(handler);
        var options = Options.Create(new RoutingOptions
        {
            BaseUrl = baseUrl,
            ApiKey = apiKey,
            MaxWaypoints = maxWaypoints,

            // The throttle gate is process-wide static state shared by every test in this fixture;
            // a high rate keeps it from serialising the suite behind a real wall-clock delay.
            RequestsPerSecond = 1000,
        });

        return new OpenRouteServiceRoutingProvider(factory, options, NullLogger<OpenRouteServiceRoutingProvider>.Instance);
    }

    private sealed class StubHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        // Deliberately leaves the handler undisposed: the fixture inspects it after the call.
        public HttpClient CreateClient(string name) => new(handler, disposeHandler: false);
    }

    /// <summary>
    /// Stands in for the network. Records what went out so the request contract can be asserted,
    /// and counts calls so the retry can be proven BOUNDED rather than merely present.
    /// </summary>
    private sealed class StubHandler : HttpMessageHandler
    {
        private Func<int, HttpResponseMessage>? respond;
        private Func<Exception>? throwWith;

        public int CallCount { get; private set; }

        public string? LastRequestBody { get; private set; }

        public static StubHandler Returning(HttpResponseMessage response)
            => new() { respond = _ => response };

        public static StubHandler Returning(Func<HttpResponseMessage> response)
            => new() { respond = _ => response() };

        public static StubHandler ReturningInOrder(params HttpResponseMessage[] responses)
            => new() { respond = attempt => responses[Math.Min(attempt, responses.Length) - 1] };

        public static StubHandler Throwing(Func<Exception> exception)
            => new() { throwWith = exception };

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            if (request.Content is not null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return throwWith is not null ? throw throwWith() : respond!(CallCount);
        }
    }
}
