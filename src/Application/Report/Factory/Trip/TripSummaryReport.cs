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

// Trips by period (spec 11 §13): plan versus reality on one line per trip. Filters:
// DateTimeFilter1/2 = window, StringFilter1 = transporter id (optional GUID; unparseable values are
// ignored — the portal's only picker slot). Most recent departure first. Excel only.
public sealed class TripSummaryReport(ITripReportReader reader) : IReport
{
    public string ReportCode => TripReportCodes.Summary;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureTripManagementFeatureAsync(cancellationToken);

        var transporterId = TripReportSupport.ParseOptionalId(filters.StringFilter1);

        var trips = await reader.GetTripsAsync(
            filters.DateTimeFilter1, filters.DateTimeFilter2, transporterId, driverId: null, cancellationToken);

        var rows = trips
            .OrderByDescending(t => t.PlannedStartAt)
            .ThenBy(t => t.Code, StringComparer.OrdinalIgnoreCase)
            .Select(t => new TripSummaryRowVm(
                t.Code,
                t.Status,
                t.TransporterName,
                t.DriverName.OrEmpty(),
                t.PlannedStartAt,
                t.ActualStartAt,
                t.PlannedEndAt,
                t.ActualEndAt,
                t.PlannedDistanceMeters.ToKilometers(),
                t.ActualDistanceMeters.ToKilometers(),
                t.StopCount,
                t.IsOnTime(),
                t.EstimatedTollAmount,
                t.TollCurrency.OrEmpty()))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
