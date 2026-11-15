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

namespace TrackHub.Router.Infrastructure.Cartrack.Models;

internal sealed record Page<T>(
    IEnumerable<T>? Data,
    PageMeta? Meta);

internal readonly record struct PageMeta(
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("last_page")] int LastPage);

internal readonly record struct Vehicle(
    [property: JsonPropertyName("vehicle_id")] long VehicleId,
    string Registration,
    [property: JsonPropertyName("vehicle_name")] string? VehicleName);

internal readonly record struct VehicleStatus(
    [property: JsonPropertyName("vehicle_id")] long VehicleId,
    string Registration,
    [property: JsonPropertyName("event_ts"), JsonConverter(typeof(UtcNullableDateTimeOffsetJsonConverter))] DateTimeOffset? EventTs,
    double? Bearing,
    double? Speed,
    bool? Ignition,
    double? Odometer,
    double? Clock,
    double? Altitude,
    double? Temp1,
    StatusLocation? Location);

internal readonly record struct StatusLocation(
    [property: JsonConverter(typeof(UtcNullableDateTimeOffsetJsonConverter))] DateTimeOffset? Updated,
    double? Latitude,
    double? Longitude,
    [property: JsonPropertyName("position_description")] string? PositionDescription);

internal readonly record struct VehicleEvent(
    [property: JsonPropertyName("event_ts"), JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))] DateTimeOffset EventTs,
    [property: JsonPropertyName("received_ts"), JsonConverter(typeof(UtcNullableDateTimeOffsetJsonConverter))] DateTimeOffset? ReceivedTs,
    double? Latitude,
    double? Longitude,
    [property: JsonPropertyName("gps_fix_type")] int? GpsFixType,
    double? Bearing,
    double? Speed,
    bool? Ignition,
    double? Odometer,
    double? Clock,
    double? Altitude,
    double? Temp1,
    [property: JsonPropertyName("position_description")] string? PositionDescription);
