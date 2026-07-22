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
/// Platform toll reference data. No <c>accountId</c> parameter anywhere by design — this is road
/// infrastructure, readable by any authenticated account user (spec 11 §5).
/// </summary>
public interface ITollCatalogReader
{
    Task<TollStationsPageVm> GetStationsPageAsync(string? search, string? country, bool? active, int skip, int take, CancellationToken cancellationToken);

    Task<TollStationDetailVm> GetStationDetailAsync(Guid tollStationId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TollVehicleClassVm>> GetVehicleClassesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stations within <paramref name="toleranceMeters"/> of the planned line, matched with
    /// <c>ST_DWithin</c> against the GiST index and de-duplicated per station. The tariff
    /// resolved is the one effective on <paramref name="onDate"/> for
    /// <paramref name="vehicleClassCode"/> — a matched station with no such tariff comes back
    /// with a null amount so the estimate can report <c>PartialNoTariff</c> instead of
    /// silently understating cost (spec 11 §6.2).
    /// </summary>
    Task<IReadOnlyCollection<TollStationMatchVm>> MatchStationsAsync(
        IReadOnlyCollection<CoordinateVm> route,
        double toleranceMeters,
        string vehicleClassCode,
        DateOnly onDate,
        CancellationToken cancellationToken);

    /// <summary>True when an open or overlapping tariff window already covers the pair — a 409.</summary>
    Task<bool> HasOverlappingTariffAsync(Guid tollStationId, string vehicleClassCode, DateOnly effectiveFrom, DateOnly? effectiveTo, Guid? excludeTariffId, CancellationToken cancellationToken);
}
