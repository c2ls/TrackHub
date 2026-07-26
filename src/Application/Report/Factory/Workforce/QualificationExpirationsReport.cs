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

// Driver qualification expirations (spec 09 §13). Qualifications expiring inside the window
// (NumericFilter1 days, default 30), nearest expiry first. Short report — also SupportsPdf.
public sealed class QualificationExpirationsReport(IWorkforceReportReader reader) : IReport
{
    private const int DefaultWithinDays = 30;

    public string ReportCode => WorkforceReportCodes.QualificationExpirations;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureWorkforceFeatureAsync(cancellationToken);

        var withinDays = (int)(filters.NumericFilter1 ?? DefaultWithinDays);
        var qualifications = await reader.GetDriverQualificationsAsync(null, withinDays, cancellationToken);

        var rows = qualifications
            .OrderBy(q => q.ExpiresAt ?? DateOnly.MaxValue)
            .ThenBy(q => q.DriverName, StringComparer.OrdinalIgnoreCase)
            .Select(q => new QualificationExpirationRowVm(
                q.DriverName,
                q.QualificationType,
                q.Category.OrEmpty(), // → LicenseCategory column

                q.Number.OrEmpty(),
                q.IssuingAuthority.OrEmpty(),
                q.IssuedAt.ToUtcInstant(),
                q.ExpiresAt.ToUtcInstant(),
                q.ExpiresAt.DaysUntil(),
                q.Status))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
