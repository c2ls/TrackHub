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

// Stop-level detail (spec 11 §13): every stop of every trip in the window with its planned window,
// actual arrival/departure, dwell, status and delivery outcome counts. Filters: DateTimeFilter1/2 =
// window, StringFilter1 = transporter id. Ordered by trip then stop sequence so a trip reads top to
// bottom. Excel only — a stop-level export routinely exceeds the 500-row PDF limit.
public sealed class TripDetailReport(ITripReportReader reader) : IReport
{
    public string ReportCode => TripReportCodes.Detail;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureTripManagementFeatureAsync(cancellationToken);

        var transporterId = TripReportSupport.ParseOptionalId(filters.StringFilter1);

        var stops = await reader.GetTripStopsAsync(
            filters.DateTimeFilter1, filters.DateTimeFilter2, transporterId, driverId: null, cancellationToken);

        var rows = stops
            .OrderBy(s => s.TripCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(s => s.Sequence)
            .Select(s => new TripStopDetailRowVm(
                s.TripCode,
                s.Sequence,
                s.Name,
                s.CustomerName.OrEmpty(),
                s.PlannedArrivalFrom,
                s.PlannedArrivalTo,
                s.ActualArrivalAt,
                s.ActualDepartureAt,
                TripReportSupport.DwellMinutes(s.ActualArrivalAt, s.ActualDepartureAt),
                s.Status,
                s.DeliveryCount,
                s.DeliveredCount,
                s.FailedDeliveryCount,
                s.PartialDeliveryCount))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
