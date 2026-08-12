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

namespace TrackHub.Reporting.Application.Report.Factory.Admin;

// Accounts-by-status report (SuperAdministrator). Runs under the caller's token; the
// Manager `accounts` query enforces the Administrative/Read permission. Optional status filter via
// `status` (AccountStatus enum name).
public sealed class AccountsByStatusReport(IAdminReportReader reader) : IReport
{
    public string ReportCode => AdminReportCodes.AccountsByStatus;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accounts = await reader.GetAccountsAsync(cancellationToken);

        var filtered = filters.GetText(FilterNames.Status) is not { } status
            ? accounts
            : accounts.Where(a => string.Equals(a.Status, status, StringComparison.OrdinalIgnoreCase));

        var rows = filtered
            .OrderBy(a => a.Status, StringComparer.OrdinalIgnoreCase)
            .ThenBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .Select(a => new AccountByStatusRowVm(a.Name, a.Status, a.TypeId, a.Active, a.LastModified))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
