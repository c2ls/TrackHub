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

// Group-membership export (Account Administrator, account-scoped): one row per group ↔
// user/transporter assignment. The Manager `groupsByAccount` query is scoped to the caller's account.
public sealed class GroupMembershipReport(IAdminReportReader reader) : IReport
{
    private const string MemberTypeUser = "User";
    private const string MemberTypeTransporter = "Transporter";

    public string ReportCode => AdminReportCodes.GroupMembership;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var groups = (await reader.GetGroupsByAccountAsync(cancellationToken))
            .OrderBy(g => g.Name, StringComparer.OrdinalIgnoreCase);

        var rows = new List<GroupMembershipRowVm>();
        foreach (var group in groups)
        {
            var users = await reader.GetUsersByGroupAsync(group.GroupId, cancellationToken);
            foreach (var user in users.OrderBy(u => u.Username, StringComparer.OrdinalIgnoreCase))
            {
                rows.Add(new GroupMembershipRowVm(group.Name, MemberTypeUser, user.Username, user.UserId.ToString()));
            }

            var transporters = await reader.GetTransportersByGroupAsync(group.GroupId, cancellationToken);
            foreach (var transporter in transporters.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase))
            {
                rows.Add(new GroupMembershipRowVm(group.Name, MemberTypeTransporter, transporter.Name, transporter.TransporterId.ToString()));
            }
        }

        return ReportDataset.Create(filters, rows);
    }
}
