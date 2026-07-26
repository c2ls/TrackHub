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

// Dwell distribution per stop/customer (spec 11 §13). Filters: DateTimeFilter1/2 = window,
// StringFilter1 = transporter id. Longest average dwell first — the reason to read the report is to
// find where time is lost. Excel only.
//
// Only completed visits (both an arrival and a departure) contribute: an open visit has no dwell
// yet, and treating it as zero would understate every average it lands in.
public sealed class TripStopDwellReport(ITripReportReader reader) : IReport
{
    public string ReportCode => TripReportCodes.StopDwell;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureTripManagementFeatureAsync(cancellationToken);

        var transporterId = TripReportSupport.ParseOptionalId(filters.StringFilter1);

        var stops = await reader.GetTripStopsAsync(
            filters.DateTimeFilter1, filters.DateTimeFilter2, transporterId, driverId: null, cancellationToken);

        var visits = stops
            .Select(s => new
            {
                s.Name,
                CustomerName = s.CustomerName.OrUnspecified(),
                Dwell = TripReportSupport.DwellMinutes(s.ActualArrivalAt, s.ActualDepartureAt)
            })
            .Where(s => s.Dwell.HasValue)
            .ToList();

        var rows = visits
            .GroupBy(s => (s.Name, s.CustomerName))
            .Select(g =>
            {
                var dwells = g.Select(s => s.Dwell!.Value).ToList();
                return new TripStopDwellRowVm(
                    g.Key.Name,
                    g.Key.CustomerName,
                    dwells.Count,
                    Math.Round(dwells.Average(), 2),
                    Math.Round(dwells.Min(), 2),
                    Math.Round(dwells.Max(), 2),
                    Math.Round(dwells.Sum(), 2));
            })
            .OrderByDescending(r => r.AverageDwellMinutes)
            .ThenBy(r => r.StopName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
