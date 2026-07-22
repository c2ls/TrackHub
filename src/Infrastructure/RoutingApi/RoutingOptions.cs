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

namespace TrackHub.TripManagement.Infrastructure.RoutingApi;

/// <summary>
/// OpenRouteService routing settings — <c>AppSettings:Routing</c>, deployment-supplied via
/// environment variables (the spec 05 <c>AppSettings:Smtp</c>/<c>AppSettings:WhatsApp</c>
/// precedent). Pointing <see cref="BaseUrl"/> at a self-hosted ORS instance is supported and is
/// the only change such a deployment needs (spec 11 §7.2a, §14).
/// </summary>
public sealed class RoutingOptions
{
    public const string SectionName = "AppSettings:Routing";

    /// <summary>Recorded on the resulting <c>RoutePlan</c>; see <c>RoutePlanProviders</c>.</summary>
    public string Provider { get; set; } = RoutePlanProviders.OpenRouteService;

    /// <summary>ORS root, e.g. <c>https://api.openrouteservice.org</c>. Absent ⇒ not configured.</summary>
    public string? BaseUrl { get; set; }

    /// <summary>ORS API key, sent as the <c>Authorization</c> header. Absent ⇒ not configured.</summary>
    public string? ApiKey { get; set; }

    /// <summary>Routing profile. Trips are freight by default.</summary>
    public string Profile { get; set; } = "driving-hgv";

    /// <summary>Process-wide outbound rate limit, honoured by a static throttle gate.</summary>
    public int RequestsPerSecond { get; set; } = 2;

    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Waypoint ceiling; a larger request is rejected before any call goes out.</summary>
    public int MaxWaypoints { get; set; } = 50;
}
