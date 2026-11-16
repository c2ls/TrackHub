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

using System.Text.Json.Serialization;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.Tracksolid.Models;

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
internal sealed record TracksolidResponse<T>(
    int Code,
    string? Message,
    T? Result
);

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
internal sealed record TokenResult(
    string? AccessToken,
    int ExpiresIn,
    string? RefreshToken
);

internal sealed record DeviceRecord(
    string Imei,
    string? DeviceName
);

internal sealed record LocationRecord(
    string Imei,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Lat,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Lng,
    string? GpsTime,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Speed,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Direction,
    string? AccStatus,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? GpsNum,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? CurrentMileage,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Temperature
);

internal sealed record TrackPoint(
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Lat,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Lng,
    string? GpsTime,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Direction,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? GpsSpeed,
    string? Ignition,
    string? AccStatus
);
