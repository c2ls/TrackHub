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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TrackHub.TripManagement.Infrastructure.RoutingApi;

/// <summary>
/// OpenRouteService directions over a named <see cref="HttpClient"/>:
/// <c>POST {BaseUrl}/v2/directions/{profile}/geojson</c>. ORS speaks <b>lon,lat</b> order on the
/// wire and this adapter is the only place that knows it — everything above it uses
/// <see cref="CoordinateVm"/> (lat,lng).
/// <para>
/// Every failure path leaves as a <see cref="RoutingUnavailableException"/> carrying
/// <c>ROUTING_UNAVAILABLE</c> or <c>ROUTING_NOT_CONFIGURED</c>; nothing else escapes, because the
/// planning handler turns that into a <c>Failed</c> route plan and the trip stays fully usable
/// (spec 11 §7.3, §14, acceptance 18).
/// </para>
/// </summary>
public sealed class OpenRouteServiceRoutingProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<RoutingOptions> options,
    ILogger<OpenRouteServiceRoutingProvider> logger) : IRoutingProvider
{
    public const string HttpClientName = "OpenRouteServiceRouting";

    private const int MaxAttempts = 3;
    private static readonly TimeSpan InitialBackoff = TimeSpan.FromMilliseconds(400);

    // Throttle state is process-wide: the provider itself is registered Scoped, but the vendor
    // rate limit is per deployment (the Router ReverseGeocodingService pattern).
    //
    // Two lanes, because interactive route planning and the ETA job are not equal callers. A single
    // queue meant a dispatcher's PlanTripRoute waited behind everything the background job had already
    // enqueued — unbounded, since the job walks every account and up to MaxTripsPerCycle trips in each.
    // Each lane now queues separately; only the pacing clock, which is what the vendor limit actually
    // constrains, is shared, so an interactive caller waits for at most one in-flight background
    // request rather than for the whole background backlog.
    private static readonly SemaphoreSlim InteractiveGate = new(1, 1);
    private static readonly SemaphoreSlim BackgroundGate = new(1, 1);
    private static readonly SemaphoreSlim PacingGate = new(1, 1);
    private static DateTimeOffset _lastRequestAt = DateTimeOffset.MinValue;

    // Background requests stand aside while any interactive request is queueing, and the deferral is
    // capped so continuous interactive traffic degrades ETA refresh rather than starving it outright.
    private static int _interactiveQueued;
    private static readonly TimeSpan InteractiveYieldInterval = TimeSpan.FromMilliseconds(50);

    private static readonly Lock BudgetLock = new();
    private static DateTimeOffset _budgetWindowStartedAt = DateTimeOffset.MinValue;
    private static int _budgetUsed;

    private enum RequestLane
    {
        Interactive,
        Background
    }

    public string Name => options.Value.Provider;

    public bool IsConfigured
        => !string.IsNullOrWhiteSpace(options.Value.BaseUrl) && !string.IsNullOrWhiteSpace(options.Value.ApiKey);

    public async Task<RouteResultVm> GetRouteAsync(IReadOnlyCollection<CoordinateVm> waypoints, CancellationToken cancellationToken)
    {
        var routing = EnsureConfigured();

        if (waypoints is null || waypoints.Count < 2)
        {
            throw new RoutingUnavailableException(
                TripErrorCodes.RoutingUnavailable,
                "A route needs at least two waypoints.");
        }

        if (waypoints.Count > routing.MaxWaypoints)
        {
            throw new RoutingUnavailableException(
                TripErrorCodes.RoutingUnavailable,
                $"The route has {waypoints.Count} waypoints, above the configured maximum of {routing.MaxWaypoints}.");
        }

        using var document = await SendAsync(routing, waypoints, RequestLane.Interactive, cancellationToken);
        var feature = GetFirstFeature(document);

        var geometry = ReadGeometry(feature);
        var (distance, duration) = ReadSummary(feature);
        var legs = ReadLegs(feature);

        return new RouteResultVm(geometry, distance, duration, legs);
    }

    public async Task<RouteSummaryVm> GetSummaryAsync(CoordinateVm from, CoordinateVm to, CancellationToken cancellationToken)
    {
        var routing = EnsureConfigured();

        using var document = await SendAsync(routing, [from, to], RequestLane.Background, cancellationToken);
        var feature = GetFirstFeature(document);

        // Deliberately no geometry: the ETA path never asks for what it will not store.
        var (distance, duration) = ReadSummary(feature);
        return new RouteSummaryVm(distance, duration);
    }

    private RoutingOptions EnsureConfigured()
    {
        var routing = options.Value;
        return IsConfigured
            ? routing
            : throw new RoutingUnavailableException(
                TripErrorCodes.RoutingNotConfigured,
                "OpenRouteService is not configured (AppSettings:Routing:BaseUrl/ApiKey).");
    }

    private async Task<JsonDocument> SendAsync(
        RoutingOptions routing,
        IReadOnlyCollection<CoordinateVm> waypoints,
        RequestLane lane,
        CancellationToken cancellationToken)
    {
        var url = $"{routing.BaseUrl!.TrimEnd('/')}/v2/directions/{routing.Profile}/geojson";
        var body = JsonSerializer.Serialize(new
        {
            coordinates = waypoints.Select(w => new[] { w.Longitude, w.Latitude }).ToArray()
        });

        var backoff = InitialBackoff;
        for (var attempt = 1; ; attempt++)
        {
            await ThrottleAsync(routing, lane, cancellationToken);

            HttpResponseMessage? response = null;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };
                request.Headers.TryAddWithoutValidation("Authorization", routing.ApiKey);

                var client = httpClientFactory.CreateClient(HttpClientName);
                response = await client.SendAsync(request, cancellationToken);

                // Only 429 and 5xx are retried, and only because this call is a read: a route
                // request has no side effect at the provider, so a repeat is safe.
                if (IsRetryable(response.StatusCode) && attempt < MaxAttempts)
                {
                    response.Dispose();
                    response = null;
                    await Task.Delay(backoff, cancellationToken);
                    backoff += backoff;
                    continue;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning(
                        "OpenRouteService returned {StatusCode} for {WaypointCount} waypoints: {Error}",
                        (int)response.StatusCode,
                        waypoints.Count,
                        Truncate(error));
                    throw new RoutingUnavailableException(
                        TripErrorCodes.RoutingUnavailable,
                        $"OpenRouteService returned {(int)response.StatusCode}.");
                }

                var payload = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonDocument.Parse(payload);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (RoutingUnavailableException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Timeouts, socket failures and malformed payloads all degrade the same way.
                logger.LogWarning(ex, "OpenRouteService request failed.");
                throw new RoutingUnavailableException(
                    TripErrorCodes.RoutingUnavailable,
                    "The routing provider is unavailable.");
            }
            finally
            {
                response?.Dispose();
            }
        }
    }

    private static bool IsRetryable(HttpStatusCode statusCode)
        => statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;

    private static JsonElement GetFirstFeature(JsonDocument document)
    {
        if (document.RootElement.ValueKind == JsonValueKind.Object
            && document.RootElement.TryGetProperty("features", out var features)
            && features.ValueKind == JsonValueKind.Array
            && features.GetArrayLength() > 0)
        {
            return features[0];
        }

        throw new RoutingUnavailableException(
            TripErrorCodes.RoutingUnavailable,
            "OpenRouteService returned no route feature.");
    }

    private static IReadOnlyCollection<CoordinateVm> ReadGeometry(JsonElement feature)
    {
        var coordinates = new List<CoordinateVm>();
        if (feature.TryGetProperty("geometry", out var geometry)
            && geometry.TryGetProperty("coordinates", out var points)
            && points.ValueKind == JsonValueKind.Array)
        {
            foreach (var point in points.EnumerateArray())
            {
                // ORS emits [lon, lat] (plus an optional elevation) — flip it back here.
                if (point.ValueKind == JsonValueKind.Array && point.GetArrayLength() >= 2)
                {
                    coordinates.Add(new CoordinateVm(point[1].GetDouble(), point[0].GetDouble()));
                }
            }
        }

        return coordinates.Count > 0
            ? coordinates
            : throw new RoutingUnavailableException(
                TripErrorCodes.RoutingUnavailable,
                "OpenRouteService returned a route without geometry.");
    }

    private static (double DistanceMeters, int DurationSeconds) ReadSummary(JsonElement feature)
    {
        if (feature.TryGetProperty("properties", out var properties)
            && properties.TryGetProperty("summary", out var summary))
        {
            return (ReadDouble(summary, "distance"), (int)Math.Round(ReadDouble(summary, "duration")));
        }

        throw new RoutingUnavailableException(
            TripErrorCodes.RoutingUnavailable,
            "OpenRouteService returned a route without a summary.");
    }

    private static IReadOnlyCollection<RouteLegVm> ReadLegs(JsonElement feature)
    {
        var legs = new List<RouteLegVm>();
        if (feature.TryGetProperty("properties", out var properties)
            && properties.TryGetProperty("segments", out var segments)
            && segments.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var segment in segments.EnumerateArray())
            {
                legs.Add(new RouteLegVm(
                    index++,
                    ReadDouble(segment, "distance"),
                    (int)Math.Round(ReadDouble(segment, "duration"))));
            }
        }

        return legs;
    }

    private static double ReadDouble(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : 0d;

    private static Task ThrottleAsync(RoutingOptions routing, RequestLane lane, CancellationToken cancellationToken)
        => lane == RequestLane.Interactive
            ? ThrottleInteractiveAsync(routing, cancellationToken)
            : ThrottleBackgroundAsync(routing, cancellationToken);

    // The whole admission is bounded by InteractiveTimeoutSeconds: a dispatcher gets
    // ROUTING_UNAVAILABLE — which the planning handler already records as a Failed plan, leaving the
    // trip usable — rather than a request that hangs for as long as the queue happens to be.
    private static async Task ThrottleInteractiveAsync(RoutingOptions routing, CancellationToken cancellationToken)
    {
        var timeout = TimeSpan.FromSeconds(Math.Max(1, routing.InteractiveTimeoutSeconds));
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);

        Interlocked.Increment(ref _interactiveQueued);
        try
        {
            await InteractiveGate.WaitAsync(timeoutSource.Token);
            try
            {
                await PaceAsync(routing.RequestsPerSecond, timeoutSource.Token);
            }
            finally
            {
                InteractiveGate.Release();
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new RoutingUnavailableException(
                TripErrorCodes.RoutingUnavailable,
                $"The routing provider did not become available within {timeout.TotalSeconds:0} second(s).");
        }
        finally
        {
            Interlocked.Decrement(ref _interactiveQueued);
        }
    }

    private static async Task ThrottleBackgroundAsync(RoutingOptions routing, CancellationToken cancellationToken)
    {
        if (!TryConsumeBackgroundBudget(routing))
        {
            throw new RoutingUnavailableException(
                TripErrorCodes.RoutingUnavailable,
                $"The background routing budget of {Math.Max(1, routing.BackgroundRequestsPerWindow)} request(s) per {Math.Max(1, routing.BackgroundWindowSeconds)} second(s) is exhausted.");
        }

        await BackgroundGate.WaitAsync(cancellationToken);
        try
        {
            await YieldToInteractiveAsync(routing, cancellationToken);
            await PaceAsync(routing.RequestsPerSecond, cancellationToken);
        }
        finally
        {
            BackgroundGate.Release();
        }
    }

    private static async Task YieldToInteractiveAsync(RoutingOptions routing, CancellationToken cancellationToken)
    {
        var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(Math.Max(1, routing.InteractiveTimeoutSeconds));
        while (Volatile.Read(ref _interactiveQueued) > 0 && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(InteractiveYieldInterval, cancellationToken);
        }
    }

    // Rolling window rather than a per-cycle counter: the provider has no idea when a job cycle begins,
    // and a window that resets on its own also covers overlapping cycles, which is the state this budget
    // exists to survive.
    private static bool TryConsumeBackgroundBudget(RoutingOptions routing)
    {
        var window = TimeSpan.FromSeconds(Math.Max(1, routing.BackgroundWindowSeconds));
        var budget = Math.Max(1, routing.BackgroundRequestsPerWindow);

        lock (BudgetLock)
        {
            var now = DateTimeOffset.UtcNow;
            if (now - _budgetWindowStartedAt >= window)
            {
                _budgetWindowStartedAt = now;
                _budgetUsed = 0;
            }

            if (_budgetUsed >= budget)
            {
                return false;
            }

            _budgetUsed++;
            return true;
        }
    }

    // The vendor rate limit is one limit for the whole deployment, so both lanes share this clock.
    private static async Task PaceAsync(int requestsPerSecond, CancellationToken cancellationToken)
    {
        var minInterval = TimeSpan.FromSeconds(1d / Math.Max(1, requestsPerSecond));

        await PacingGate.WaitAsync(cancellationToken);
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
            PacingGate.Release();
        }
    }

    private static string Truncate(string value)
        => value.Length <= 500 ? value : string.Concat(value.AsSpan(0, 500), "…");
}
