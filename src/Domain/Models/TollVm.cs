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

namespace TrackHub.TripManagement.Domain.Models;

/// <summary>
/// Platform toll reference data — no <c>AccountId</c>. It describes public road infrastructure,
/// not tenant business data, so it is readable by any authenticated account user and writable
/// only by an Administrator (spec 11 §5; see findings.md SVD-12).
/// </summary>
public readonly record struct TollVehicleClassVm(
    Guid TollVehicleClassId,
    string Code,
    string Name,
    string? Description,
    int SortOrder,
    bool Active);

public readonly record struct TollStationVm(
    Guid TollStationId,
    string Name,
    string? Code,
    double Latitude,
    double Longitude,
    string? Country,
    string? Region,
    string? RoadName,
    string? Direction,
    string? Operator,
    string? Notes,
    bool Active);

public readonly record struct TollStationsPageVm(IReadOnlyCollection<TollStationVm> Items, int TotalCount);

/// <summary>
/// A station with its full tariff history. Tariffs are temporal and never overwritten, so a
/// historical trip's estimate stays explainable (spec 11 §5, acceptance 21).
/// </summary>
public readonly record struct TollStationDetailVm(
    TollStationVm Station,
    IReadOnlyCollection<TollTariffVm> Tariffs);

public readonly record struct TollTariffVm(
    Guid TollTariffId,
    Guid TollStationId,
    string TollVehicleClassCode,
    decimal Amount,
    string Currency,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);

/// <summary>
/// One station matched against a planned route. <paramref name="Amount"/> is null — not zero —
/// when no tariff covers the trip's class on the plan date; the estimate reports that as
/// <c>PartialNoTariff</c> rather than silently netting the gap to zero.
/// </summary>
public readonly record struct TollStationMatchVm(
    Guid TollStationId,
    string Name,
    string? Code,
    double Latitude,
    double Longitude,
    string? RoadName,
    string? Direction,
    decimal? Amount,
    string? Currency,
    bool HasTariff);

/// <summary>Result of toll matching, whether persisted onto a plan or computed as a what-if.</summary>
public readonly record struct TollEstimateVm(
    string TollVehicleClass,
    decimal? EstimatedTollAmount,
    string? Currency,
    string TollStatus,
    IReadOnlyCollection<TollStationMatchVm> Stations);

/// <summary>
/// Account-scoped mapping from fleet composition to a toll class. Fleet composition IS tenant
/// data, which is why this row carries an <c>AccountId</c> while the catalog above does not.
/// </summary>
public readonly record struct TransporterTollClassVm(
    Guid TransporterTollClassId,
    Guid AccountId,
    short? TransporterTypeId,
    Guid? TransporterId,
    string TollVehicleClassCode);

/// <summary>
/// Per-row outcome of a toll catalog CSV import. The batch never rolls back on a bad row — the
/// caller gets a row-level error report instead (the spec 08 geofence-import contract).
/// </summary>
public readonly record struct TollCatalogImportResultVm(
    int RowsRead,
    int StationsCreated,
    int StationsUpdated,
    int TariffsCreated,
    IReadOnlyCollection<TollCatalogImportErrorVm> Errors);

public readonly record struct TollCatalogImportErrorVm(int RowNumber, string ErrorCode, string Message);
