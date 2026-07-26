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
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Workforce;

// Driver↔transporter assignment history (spec 09 §13). Time-bounded assignment rows, most recent
// first. Filters: DateTimeFilter1/2 = window, StringFilter1 = transporter id (optional GUID;
// unparseable values are ignored). Excel only.
//
// StringFilter1 is the portal's only picker slot, and the transporter picker is the one list the
// reports screen can populate (see TrackHub/src/layouts/reports/data/filtersData.ts) — the same
// slot `gps.assignment-history` uses. A driver-scoped variant would need a driver picker source and
// a second picker slot, neither of which exists; the reader still accepts a driverId so that
// filter can be wired without touching this contract once the portal grows one.
public sealed class AssignmentHistoryReport(IWorkforceReportReader reader) : IReport
{
    public string ReportCode => WorkforceReportCodes.AssignmentHistory;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureWorkforceFeatureAsync(cancellationToken);

        var transporterId = ParseOptionalId(filters.StringFilter1);

        var assignments = await reader.GetDriverAssignmentHistoryAsync(
            driverId: null, transporterId, filters.DateTimeFilter1, filters.DateTimeFilter2, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var rows = assignments
            .OrderByDescending(a => a.StartsAt)
            .ThenBy(a => a.DriverName, StringComparer.OrdinalIgnoreCase)
            .Select(a => new DriverAssignmentHistoryRowVm(
                a.DriverName,
                a.TransporterName,
                a.AssignmentType,
                a.Status,
                a.StartsAt,
                a.EndsAt,
                (int)((a.EndsAt ?? now) - a.StartsAt).TotalDays,
                a.CreatedByPrincipal))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }

    // Optional GUID filter slots arrive as free text — an unparseable value means "no filter"
    // rather than an error, matching the other catalog reports' filter tolerance.
    private static Guid? ParseOptionalId(string? value)
        => Guid.TryParse(value, out var id) && id != Guid.Empty ? id : null;
}
