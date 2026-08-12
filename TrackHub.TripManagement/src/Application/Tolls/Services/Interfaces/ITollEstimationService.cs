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

namespace TrackHub.TripManagement.Application.Tolls.Services.Interfaces;

/// <summary>
/// Folds toll-station matches over a planned route into an estimate. Used both by route planning
/// (persisted onto the plan) and by the "what-if" <c>EstimateTollsQuery</c>, which must produce an
/// identical answer without touching stored state.
/// </summary>
public interface ITollEstimationService
{
    Task<TollEstimateVm> EstimateAsync(
        IReadOnlyCollection<CoordinateVm> route,
        string? vehicleClassCode,
        DateOnly onDate,
        double toleranceMeters,
        CancellationToken cancellationToken);
}
