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

namespace TrackHub.TripManagement.Domain.Interfaces;

/// <summary>
/// Administrator-only toll administration. Every write is audited — station and tariff
/// changes move money on estimates (spec 11 §7.6).
/// </summary>
public interface ITollCatalogWriter
{
    Task<TollVehicleClassVm> CreateVehicleClassAsync(TollVehicleClassDto vehicleClass, CancellationToken cancellationToken);

    Task UpdateVehicleClassAsync(Guid tollVehicleClassId, TollVehicleClassDto vehicleClass, CancellationToken cancellationToken);

    Task DeactivateVehicleClassAsync(Guid tollVehicleClassId, CancellationToken cancellationToken);

    Task<TollStationVm> CreateStationAsync(TollStationDto station, CancellationToken cancellationToken);

    Task UpdateStationAsync(Guid tollStationId, TollStationDto station, CancellationToken cancellationToken);

    Task DeactivateStationAsync(Guid tollStationId, CancellationToken cancellationToken);

    /// <summary>
    /// Inserts a tariff and CLOSES the currently open row for the same
    /// <c>(station, class)</c> pair by stamping its <c>EffectiveTo</c>. Prices are never
    /// overwritten, so a historical trip's estimate stays reproducible (acceptance 21).
    /// </summary>
    Task<TollTariffVm> CreateTariffAsync(TollTariffDto tariff, CancellationToken cancellationToken);

    Task UpdateTariffAsync(Guid tollTariffId, TollTariffDto tariff, CancellationToken cancellationToken);

    Task DeleteTariffAsync(Guid tollTariffId, CancellationToken cancellationToken);

    /// <summary>
    /// Upserts a parsed CSV batch. A bad row is reported and skipped — there is no
    /// partial-failure rollback (the spec 08 geofence-import contract).
    /// </summary>
    Task<TollCatalogImportResultVm> ImportAsync(IReadOnlyCollection<TollCatalogImportRowDto> rows, CancellationToken cancellationToken);
}
