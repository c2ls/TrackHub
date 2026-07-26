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

// On-time performance (spec 11 §13): punctuality percentage, average and worst delay, and delayed
// stop counts grouped by transporter/driver/customer. Filters: DateTimeFilter1/2 = window,
// StringFilter1 = transporter id. Aggregated, so it stays inside the 500-row PDF limit — SupportsPdf.
//
// Only stops carrying BOTH a planned window end and an actual arrival are evaluated: a stop with no
// planned window has no punctuality to measure, and a stop never arrived at would otherwise be
// silently counted as on time. Early arrivals contribute a zero delay, never a negative one, so
// running early cannot mask someone else's lateness.
public sealed class TripOnTimePerformanceReport(ITripReportReader reader) : IReport
{
    public string ReportCode => TripReportCodes.OnTimePerformance;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureTripManagementFeatureAsync(cancellationToken);

        var transporterId = TripReportSupport.ParseOptionalId(filters.StringFilter1);

        var stops = await reader.GetTripStopsAsync(
            filters.DateTimeFilter1, filters.DateTimeFilter2, transporterId, driverId: null, cancellationToken);

        var evaluated = stops
            .Select(s => new
            {
                s.TransporterName,
                DriverName = s.DriverName.OrUnspecified(),
                CustomerName = s.CustomerName.OrUnspecified(),
                Delay = TripReportSupport.DelayMinutes(s.PlannedArrivalTo, s.ActualArrivalAt)
            })
            .Where(s => s.Delay.HasValue)
            .ToList();

        var rows = evaluated
            .GroupBy(s => (s.TransporterName, s.DriverName, s.CustomerName))
            .OrderBy(g => g.Key.TransporterName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(g => g.Key.DriverName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(g => g.Key.CustomerName, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var delays = g.Select(s => Math.Max(0d, s.Delay!.Value)).ToList();
                var onTime = delays.Count(d => d <= 0d);
                return new TripOnTimePerformanceRowVm(
                    g.Key.TransporterName,
                    g.Key.DriverName,
                    g.Key.CustomerName,
                    delays.Count,
                    onTime,
                    delays.Count - onTime,
                    Math.Round(onTime * 100d / delays.Count, 2),
                    Math.Round(delays.Average(), 2),
                    Math.Round(delays.Max(), 2));
            })
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
