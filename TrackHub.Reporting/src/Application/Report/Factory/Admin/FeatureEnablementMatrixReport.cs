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

// Feature-enablement matrix report (SuperAdministrator): accounts × feature keys,
// enabled/tier. Two Manager calls total: the account list and one batched all-accounts feature read.
public sealed class FeatureEnablementMatrixReport(IAdminReportReader reader) : IReport
{
    public string ReportCode => AdminReportCodes.FeatureEnablementMatrix;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        var accounts = (await reader.GetAccountsAsync(cancellationToken))
            .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase);

        var featuresByAccount = (await reader.GetAllAccountFeaturesAsync(cancellationToken))
            .GroupBy(f => f.AccountId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var rows = new List<FeatureEnablementRowVm>();
        foreach (var account in accounts)
        {
            if (!featuresByAccount.TryGetValue(account.AccountId, out var features))
            {
                continue;
            }
            foreach (var feature in features.OrderBy(f => f.FeatureKey, StringComparer.OrdinalIgnoreCase))
            {
                rows.Add(new FeatureEnablementRowVm(account.Name, feature.FeatureKey, feature.Enabled, feature.Tier));
            }
        }

        return ReportDataset.Create(filters, rows);
    }
}
