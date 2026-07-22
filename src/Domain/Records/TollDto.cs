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

namespace TrackHub.TripManagement.Domain.Records;

public readonly record struct TollVehicleClassDto(
    string Code,
    string Name,
    string? Description,
    int SortOrder);

public readonly record struct TollStationDto(
    string Name,
    string? Code,
    double Latitude,
    double Longitude,
    string? Country,
    string? Region,
    string? RoadName,
    string? Direction,
    string? Operator,
    string? Notes);

/// <summary>
/// A tariff write. Creating one CLOSES the currently open row for the same
/// <c>(station, class)</c> pair rather than overwriting it — price history is append-only so a
/// historical trip's estimate stays reproducible (spec 11 §5, §7.6).
/// </summary>
public readonly record struct TollTariffDto(
    Guid TollStationId,
    string TollVehicleClassCode,
    decimal Amount,
    string Currency,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);

/// <summary>One parsed CSV row from the toll catalog import.</summary>
public readonly record struct TollCatalogImportRowDto(
    int RowNumber,
    string? StationCode,
    string? StationName,
    double? Latitude,
    double? Longitude,
    string? Country,
    string? Region,
    string? RoadName,
    string? Direction,
    string? VehicleClassCode,
    decimal? Amount,
    string? Currency,
    DateOnly? EffectiveFrom);
