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
// first. Filters: from/to = window, transporterId = optional unit (unparseable values are
// ignored). Excel only.
//
// A driver-scoped variant would need a driver picker source in the portal, which does not exist;
// the reader still accepts a driverId so that filter can be wired — a `driverId` filter definition
// in the catalog row plus a portal picker source — without touching this contract.
public sealed class AssignmentHistoryReport(IWorkforceReportReader reader) : IReport
{
    public string ReportCode => WorkforceReportCodes.AssignmentHistory;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureWorkforceFeatureAsync(cancellationToken);

        var transporterId = filters.GetGuid(FilterNames.Transporter);

        var assignments = await reader.GetDriverAssignmentHistoryAsync(
            driverId: null, transporterId, filters.GetDate(FilterNames.From), filters.GetDate(FilterNames.To), cancellationToken);

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
}
