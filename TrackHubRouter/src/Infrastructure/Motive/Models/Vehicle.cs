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

namespace TrackHub.Router.Infrastructure.Motive.Models;

internal sealed record VehiclesResponse(
    IEnumerable<VehicleEntry>? Vehicles,
    Pagination? Pagination);

internal readonly record struct VehicleEntry(Vehicle Vehicle);

internal readonly record struct Vehicle(
    long Id,
    string? Number,
    [property: JsonPropertyName("current_location")] VehicleLocation? CurrentLocation);

internal readonly record struct VehicleLocation(
    [property: JsonPropertyName("located_at"), JsonConverter(typeof(UtcDateTimeOffsetJsonConverter))] DateTimeOffset LocatedAt,
    double Lat,
    double Lon,
    double? Bearing,
    double? Speed,
    string? Description,
    double? Odometer,
    [property: JsonPropertyName("engine_hours")] double? EngineHours);

internal readonly record struct Pagination(
    [property: JsonPropertyName("per_page")] int PerPage,
    [property: JsonPropertyName("page_no")] int PageNo,
    int Total);

internal sealed record VehicleLocationsResponse(
    [property: JsonPropertyName("vehicle_locations")] IEnumerable<VehicleLocationEntry>? VehicleLocations,
    Pagination? Pagination = null);

internal readonly record struct VehicleLocationEntry(
    [property: JsonPropertyName("vehicle_location")] VehicleLocation VehicleLocation);
