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
using System.Text.Json.Serialization;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.Mapon.Models;

internal sealed record MaponResponse<T>(T? Data, MaponError? Error);

internal sealed record MaponError(int? Code, string? Msg);

internal sealed record UnitList(IReadOnlyList<Unit>? Units);

internal sealed record Unit(
    [property: JsonPropertyName("unit_id")] long? UnitId,
    string? Number,
    string? Label,
    double? Lat,
    double? Lng,
    [property: JsonPropertyName("last_update")] DateTimeOffset? LastUpdate,
    int? Speed,
    int? Direction,
    double? Mileage,
    IgnitionValue? Ignition,
    AltitudeValue? Altitude)
{
    public string? Serial => UnitId?.ToString(CultureInfo.InvariantCulture);
}

internal sealed record IgnitionValue([property: JsonConverter(typeof(LenientBoolConverter))] bool? Value);

internal sealed record AltitudeValue([property: JsonConverter(typeof(LenientDoubleConverter))] double? Value);

internal sealed record RouteList(IReadOnlyList<UnitRoutes>? Units);

internal sealed record UnitRoutes(IReadOnlyList<Route>? Routes);

internal sealed record Route(string? Type, [property: JsonPropertyName("decoded_route")] DecodedRoute? DecodedRoute);

internal sealed record DecodedRoute(IReadOnlyList<RoutePoint>? Points);

internal sealed record RoutePoint(DateTimeOffset? Gmt, double? Lat, double? Lng, int? Speed);
