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

using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Trip;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Trip;

// Estimated toll by trip/route/station/vehicle class (spec 11 §13). Filters: DateTimeFilter1/2 =
// window, StringFilter1 = transporter id. SupportsPdf.
//
// The `PartialNoTariff` column is the point of the report: a station crossed with no tariff for the
// trip's vehicle class on the plan date carries a NULL amount, not a zero. Leaving it out would turn
// a catalog gap into an invisible discount (spec 11 §7.7).
public sealed class TripTollCostReport(ITripReportReader reader) : IReport
{
    public string ReportCode => TripReportCodes.TollCost;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureTripManagementFeatureAsync(cancellationToken);

        var transporterId = TripReportSupport.ParseOptionalId(filters.StringFilter1);

        var tolls = await reader.GetTripTollsAsync(
            filters.DateTimeFilter1, filters.DateTimeFilter2, transporterId, driverId: null, cancellationToken);

        var rows = tolls
            .OrderByDescending(t => t.PlannedStartAt)
            .ThenBy(t => t.TripCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(t => t.StationName, StringComparer.OrdinalIgnoreCase)
            .Select(t => new TripTollCostRowVm(
                t.TripCode,
                t.RoutePlanId?.ToString() ?? string.Empty,
                t.StationName,
                t.RoadName.OrEmpty(),
                t.Direction.OrEmpty(),
                t.TollVehicleClass.OrEmpty(),
                t.Amount,
                t.Currency.OrEmpty(),
                !t.HasTariff))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
