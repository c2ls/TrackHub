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
using Common.Domain.Json;

namespace TrackHub.Router.Infrastructure.Samsara.Models;

/// <summary>
/// Samsara GPS data model
/// </summary>
internal readonly record struct GpsData(
    // Samsara sends RFC 3339 timestamps with an explicit offset ("Z"); honour it and normalize to UTC.
    [property: JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))] DateTimeOffset Time,
    double Latitude,
    double Longitude,
    double HeadingDegrees,
    double SpeedMilesPerHour,
    bool IsEcuSpeed,
    ReverseGeo? ReverseGeo
);

/// <summary>
/// Reverse geocoding data
/// </summary>
internal readonly record struct ReverseGeo(
    string FormattedLocation
);
