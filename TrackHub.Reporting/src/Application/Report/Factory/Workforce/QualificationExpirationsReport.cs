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

using Common.Application.Interfaces;
using Common.Domain.Time;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Workforce;

// Staff qualification expirations (spec 09 §13). Qualifications expiring inside the window
// (withinDays, default 30), nearest expiry first. Short report — also SupportsPdf.
public sealed class QualificationExpirationsReport(IWorkforceReportReader reader, IUser user, IAccountTimeZoneResolver zones) : IReport
{
    private const int DefaultWithinDays = 30;

    public string ReportCode => WorkforceReportCodes.QualificationExpirations;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureWorkforceFeatureAsync(cancellationToken);

        var withinDays = (int)(filters.GetNumber(FilterNames.WithinDays) ?? DefaultWithinDays);
        var qualifications = await reader.GetStaffQualificationsAsync(null, withinDays, cancellationToken);
        var today = (await ReportCalendar.ForCallerAsync(user, zones, cancellationToken)).Today();

        var rows = qualifications
            .OrderBy(q => q.ExpiresAt ?? DateOnly.MaxValue)
            .ThenBy(q => q.StaffName, StringComparer.OrdinalIgnoreCase)
            .Select(q => new QualificationExpirationRowVm(
                q.StaffName,
                q.QualificationType,
                q.Category.OrEmpty(), // → LicenseCategory column

                q.Number.OrEmpty(),
                q.IssuingAuthority.OrEmpty(),
                q.IssuedAt,
                q.ExpiresAt,
                q.ExpiresAt.DaysUntil(today),
                q.Status))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
