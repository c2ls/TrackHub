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

using TrackHub.TripManagement.Application.Tolls.Services.Interfaces;

namespace TrackHub.TripManagement.Application.Tolls.Services;

/// <summary>
/// Matches toll stations against a planned route and folds them into a <see cref="TollEstimateVm"/>.
/// <para>
/// The status trichotomy is the whole point of this service, so it is spelled out here rather than
/// left to a caller:
/// </para>
/// <list type="bullet">
///   <item><description>no stations matched at all → <c>NoStations</c> with a <b>null</b> amount.
///   Never zero-as-fact and never an error: the platform ships an empty catalog by design
///   (spec 11 §7.7), and "no toll data" is not the same claim as "this route is free".</description></item>
///   <item><description>every match priced → <c>Computed</c> with the sum.</description></item>
///   <item><description>at least one match unpriced → <c>PartialNoTariff</c> carrying only the
///   priced subtotal. <b>An estimate that quietly understates cost is worse than no estimate</b>,
///   so the gap is reported rather than netted to zero (spec 11 §18.9, acceptance 21).</description></item>
/// </list>
/// </summary>
public sealed class TollEstimationService(ITollCatalogReader tollCatalogReader) : ITollEstimationService
{
    public async Task<TollEstimateVm> EstimateAsync(
        IReadOnlyCollection<CoordinateVm> route,
        string? vehicleClassCode,
        DateOnly onDate,
        double toleranceMeters,
        CancellationToken cancellationToken)
    {
        // Without a vehicle class there is nothing to price against. Saying so beats guessing a
        // class and producing a number the operator cannot reproduce.
        if (string.IsNullOrWhiteSpace(vehicleClassCode) || route.Count == 0)
            return new TollEstimateVm(vehicleClassCode ?? string.Empty, null, null, TollStatuses.NotComputed, []);

        var matches = await tollCatalogReader.MatchStationsAsync(route, toleranceMeters, vehicleClassCode, onDate, cancellationToken);

        if (matches.Count == 0)
            return new TollEstimateVm(vehicleClassCode, null, null, TollStatuses.NoStations, []);

        decimal pricedTotal = 0m;
        var pricedCount = 0;
        string? currency = null;
        var mixedCurrency = false;

        foreach (var match in matches)
        {
            if (!match.HasTariff || match.Amount is not { } amount)
                continue;

            pricedTotal += amount;
            pricedCount++;

            if (currency is null)
            {
                currency = match.Currency;
            }
            else if (!string.Equals(currency, match.Currency, StringComparison.OrdinalIgnoreCase))
            {
                mixedCurrency = true;
            }
        }

        // Adding COP to USD produces a number that is wrong in both. Report the breakdown and
        // refuse the total rather than labelling the sum with whichever currency happened to be
        // matched first — which also made the answer depend on match ORDER.
        if (mixedCurrency)
            return new TollEstimateVm(vehicleClassCode, null, null, TollStatuses.MixedCurrency, matches);

        if (pricedCount == matches.Count)
            return new TollEstimateVm(vehicleClassCode, pricedTotal, currency, TollStatuses.Computed, matches);

        // Partial: report the priced subtotal, or null when nothing at all could be priced —
        // a zero here would read as "this route costs nothing", which is exactly the lie to avoid.
        return new TollEstimateVm(
            vehicleClassCode,
            pricedCount > 0 ? pricedTotal : null,
            currency,
            TollStatuses.PartialNoTariff,
            matches);
    }
}
