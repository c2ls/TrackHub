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
using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

/// <summary>
/// Provides methods to read report catalog data, applying the catalog visibility rules
/// (feature gating + manager-only) against the caller's account and role.
/// </summary>
public sealed class ReportReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IReportReader
{
    /// <summary>
    /// Retrieves the reports the current principal may see: only <c>Active</c> rows, with
    /// feature-gated rows hidden unless the caller's account has the feature enabled and in its
    /// effective window, and <c>ManagerOnly</c> rows hidden from non-privileged users. Ordered by
    /// Category, SortOrder, Code. Feature lookup is a single query (no N+1).
    /// </summary>
    public async Task<IReadOnlyCollection<ReportVm>> GetReportsAsync(CancellationToken cancellationToken)
    {
        // Global service principals (no account scope) see the whole active catalog.
        if (CanAccessAllAccounts)
        {
            return await OrderedActive(Context.Reports.Where(r => r.Active))
                .ToListAsync(cancellationToken);
        }

        var privileged = IsPrivileged;

        // A user without a resolvable account only ever sees global, non-manager-only reports.
        if (!Principal.AccountId.HasValue)
        {
            return await OrderedActive(Context.Reports
                    .Where(r => r.Active && r.RequiredFeatureKey == null && !r.ManagerOnly))
                .ToListAsync(cancellationToken);
        }

        var accountId = Principal.AccountId.Value;
        var now = DateTimeOffset.UtcNow;

        // One round-trip for the account's currently-effective, enabled feature keys.
        var enabledFeatureKeys = await Context.AccountFeatures
            .Where(f => f.AccountId == accountId
                && f.Enabled
                && (!f.EffectiveFrom.HasValue || f.EffectiveFrom <= now)
                && (!f.EffectiveTo.HasValue || f.EffectiveTo >= now))
            .Select(f => f.FeatureKey)
            .ToListAsync(cancellationToken);

        return await OrderedActive(Context.Reports
                .Where(r => r.Active
                    && (r.RequiredFeatureKey == null || enabledFeatureKeys.Contains(r.RequiredFeatureKey))
                    && (privileged || !r.ManagerOnly)))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Metadata lookup by code for Reporting's execution-time enforcement. No visibility filtering
    /// (Reporting applies feature/role gating itself) and inactive rows are returned so Reporting can
    /// reject them; returns <c>null</c> for an unknown code.
    /// </summary>
    public async Task<ReportVm?> GetReportByCodeAsync(string code, CancellationToken cancellationToken)
        => await Context.Reports
            .Where(r => r.Code == code)
            .Select(r => (ReportVm?)new ReportVm(
                r.ReportId,
                r.Code,
                r.Description,
                (ReportType)r.Type,
                r.Type,
                r.Active,
                r.Category,
                r.RequiredFeatureKey,
                r.ManagerOnly,
                r.SupportsPdf,
                r.SortOrder))
            .FirstOrDefaultAsync(cancellationToken);

    private static IQueryable<ReportVm> OrderedActive(IQueryable<Report> query)
        => query
            .OrderBy(r => r.Category)
            .ThenBy(r => r.SortOrder)
            .ThenBy(r => r.Code)
            .Select(r => new ReportVm(
                r.ReportId,
                r.Code,
                r.Description,
                (ReportType)r.Type,
                r.Type,
                r.Active,
                r.Category,
                r.RequiredFeatureKey,
                r.ManagerOnly,
                r.SupportsPdf,
                r.SortOrder));
}
