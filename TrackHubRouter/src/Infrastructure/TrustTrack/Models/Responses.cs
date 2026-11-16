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

namespace TrackHub.Router.Infrastructure.TrustTrack.Models;

internal sealed record ContinuationPage<T>(
    [property: JsonPropertyName("continuation_token")] string? ContinuationToken,
    IReadOnlyList<T>? Results,
    IReadOnlyList<T>? Items)
{
    public IReadOnlyList<T> Entries => Results ?? Items ?? [];
}

internal sealed record TrackedObject(
    string? Id,
    string? Name,
    [property: JsonPropertyName("vehicle_params")] VehicleParams? VehicleParams,
    [property: JsonPropertyName("last_coordinate")] Position? LastCoordinate);

internal sealed record VehicleParams([property: JsonPropertyName("plate_number")] string? PlateNumber);

internal sealed record Position(
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Latitude,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Longitude,
    DateTimeOffset? Datetime,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Speed,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Direction,
    [property: JsonConverter(typeof(LenientDoubleConverter))] double? Altitude,
    [property: JsonPropertyName("satellites_count"), JsonConverter(typeof(LenientDoubleConverter))] double? SatellitesCount);

internal sealed record Coordinate(
    DateTimeOffset? Datetime,
    [property: JsonPropertyName("ignition_status"), JsonConverter(typeof(LenientBoolConverter))] bool? Ignition,
    Position? Position,
    Address? Address,
    Inputs? Inputs);

internal sealed record Address(
    string? Country,
    string? Locality,
    string? Street,
    [property: JsonPropertyName("house_number")] string? HouseNumber);

internal sealed record Inputs([property: JsonPropertyName("calculated_inputs")] CalculatedInputs? CalculatedInputs);

internal sealed record CalculatedInputs([property: JsonConverter(typeof(LenientDoubleConverter))] double? Mileage);
