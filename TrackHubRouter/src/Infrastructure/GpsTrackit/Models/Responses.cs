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

namespace TrackHub.Router.Infrastructure.GpsTrackit.Models;

internal sealed record SearchPage<T>(IReadOnlyList<T>? Items, int? Total);

internal sealed record Unit(long? Id, string? Label, Asset? Asset, LastReading? LastReading)
{
    public string? Serial => Id?.ToString(CultureInfo.InvariantCulture);
}

internal sealed record Asset(string? LicensePlate);

internal sealed record LastReading(
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? UnitTime,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Latitude,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Longitude,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Heading,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Speed,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? EngineOdometer,
    string? EventName,
    ReadingAddress? Address);

internal sealed record ReadingAddress(string? Formatted, string? City, string? State, string? Country);

internal sealed record Reading(
    DateTimeOffset? UnitTime,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Latitude,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Longitude,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Heading,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Speed,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? EngineOdometer,
    [property: JsonConverter(typeof(LenientBoolConverter))] bool? IgnitionStatus,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? SatelliteCount,
    string? EventName,
    string? FormattedAddress);
