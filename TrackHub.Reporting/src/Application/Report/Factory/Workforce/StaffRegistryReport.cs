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

// Staff registry export (spec 09 §13). Every staff on the account with identity, license summary and
// default transporter. No filters — the account scope comes from the caller's token. Excel only.
// Carries staff personal data; the account scope is enforced by the Manager query.
public sealed class StaffRegistryReport(IWorkforceReportReader reader) : IReport
{
    public string ReportCode => WorkforceReportCodes.StaffRegistry;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureWorkforceFeatureAsync(cancellationToken);

        var staff = await reader.GetStaffMembersAsync(cancellationToken);

        var rows = staff
            .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .Select(d => new StaffRegistryRowVm(
                d.Name,
                d.StaffType.OrEmpty(),
                d.EmployeeCode.OrEmpty(),
                d.DocumentType.OrEmpty(),
                d.DocumentNumber.OrEmpty(),
                d.Phone.OrEmpty(),
                d.LicenseNumber.OrEmpty(),
                d.LicenseExpiresAt,
                d.DefaultTransporterId?.ToString() ?? string.Empty,
                d.Active))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
