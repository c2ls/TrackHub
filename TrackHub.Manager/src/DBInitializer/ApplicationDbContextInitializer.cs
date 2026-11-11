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

using System.Text.Json;
using System.Text.Json.Serialization;
using Common.Domain.Constants;
using Microsoft.Extensions.Logging;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Common.Domain.Enums;
using TransporterType = Common.Domain.Enums.TransporterType;

namespace DBInitializer;

internal class ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger, ApplicationDbContext context)
{
    // The canonical governed catalog is the aggregate of every IReportCatalogContribution in
    // this assembly (one file per module, discovered here; core rows live in
    // CoreReportCatalogContribution).
    private static readonly IReportCatalogContribution[] ReportContributions =
        [.. typeof(ApplicationDbContextInitializer).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IReportCatalogContribution).IsAssignableFrom(t))
            .OrderBy(t => t.FullName, StringComparer.Ordinal)
            .Select(t => (IReportCatalogContribution)Activator.CreateInstance(t)!)];

    private static readonly (string Code, string Description, string Category, string? RequiredFeatureKey, bool ManagerOnly, bool SupportsPdf, int SortOrder, IReadOnlyList<ReportFilterDefinition> Filters)[] ReportCatalog =
        [.. ReportContributions.SelectMany(c => c.Reports)];

    private static readonly Dictionary<string, string> ReportGrants = ReportContributions
        .SelectMany(c => c.RequiredGrants)
        .ToDictionary(g => g.Key, g => string.Join(',', g.Value), StringComparer.Ordinal);

    // camelCase + omitted nulls: the portal parses this column as-is.
    private static readonly JsonSerializerOptions FilterJsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static string SerializeFilters(IReadOnlyList<ReportFilterDefinition> filters)
        => JsonSerializer.Serialize(filters, FilterJsonOptions);

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    // Engine-driven carriers report ignition, so it delimits their trips. Battery trackers on
    // cargo report hours apart, so only a much longer silence or jump means a lost signal.
    private static (bool AccBased, double StoppedGap, double MaxDistance, double MaxTimeGap) TripGroupingDefaults(TransporterType type)
        => type switch
        {
            TransporterType.Aircraft => (true, 10, 200, 120),
            TransporterType.Boat or TransporterType.Car or TransporterType.ConstructionVehicle
                or TransporterType.DeliveryVan or TransporterType.FleetVehicle or TransporterType.HeavyEquipment
                or TransporterType.Motorcycle or TransporterType.SchoolBus or TransporterType.Scooter
                or TransporterType.Taxi or TransporterType.Truck or TransporterType.Tractor => (true, 10, 10, 120),
            TransporterType.Asset or TransporterType.CargoContainer or TransporterType.Package
                or TransporterType.Tool => (false, 30, 500, 1440),
            _ => (false, 10, 10, 120)
        };

    public async Task TrySeedAsync()
    {
        // Default data
        // Report catalog: idempotent per-code upsert that runs every start. New rows are
        // inserted Active; existing rows get their Description + governance metadata refreshed in place,
        // but Active and Type are never overwritten (admins may have disabled a report).
        //
        // DESIGN NOTE — code is the source of truth for seeded catalog metadata. Because this refresh runs
        // on every start, a SuperAdministrator's UpdateReportCommand edits to the seeded fields
        // (Description/Category/RequiredFeatureKey/ManagerOnly/SupportsPdf/SortOrder) are TRANSIENT: they
        // revert to these code values on the next restart. Only Active (enable/disable a report) persists
        // across restarts. This is intentional — the governed catalog's shape ships with the deployment;
        // UpdateReport is for toggling availability, not for permanently re-authoring seeded metadata.
        var reportType = (short)ReportType.Basic;
        var existingReports = await context.Reports.AsTracking().ToListAsync();
        var reportsByCode = existingReports.ToDictionary(r => r.Code, StringComparer.Ordinal);
        foreach (var (code, description, category, requiredFeatureKey, managerOnly, supportsPdf, sortOrder, filters) in ReportCatalog)
        {
            var filtersJson = SerializeFilters(filters);
            if (reportsByCode.TryGetValue(code, out var report))
            {
                report.Description = description;
                report.Category = category;
                report.RequiredFeatureKey = requiredFeatureKey;
                report.ManagerOnly = managerOnly;
                report.SupportsPdf = supportsPdf;
                report.SortOrder = sortOrder;
                report.Filters = filtersJson;
                report.RequiredGrants = ReportGrants.GetValueOrDefault(code);
            }
            else
            {
                context.Reports.Add(new Report(code, description, reportType, true, category, requiredFeatureKey, managerOnly, supportsPdf, sortOrder, filtersJson)
                {
                    RequiredGrants = ReportGrants.GetValueOrDefault(code)
                });
            }
        }
        await context.SaveChangesAsync();
        if (!context.TransporterTypes.Any())
        {
            foreach (var transporterType in Enum.GetValues<TransporterType>())
            {
                var (accBased, stoppedGap, maxDistance, maxTimeGap) = TripGroupingDefaults(transporterType);
                context.TransporterTypes.Add(new TrackHub.Manager.Infrastructure.Entities.TransporterType(
                    (short)transporterType, accBased, stoppedGap, maxDistance, maxTimeGap));
            }
            await context.SaveChangesAsync();
        }
        if (!context.Accounts.Any())
        {
            var accountType = (short)AccountType.Personal;
            var masterAccount = new Account("Master Account", "Master Account Description", accountType, true);
            context.Accounts.Add(masterAccount);
            // Well-known id: Security seeds the bootstrap administrator against the same account,
            // and a minted-per-seeder guid left the two databases describing different tenants.
            context.Entry(masterAccount).Property(x => x.AccountId).CurrentValue = PlatformBootstrap.MasterAccountId;
            await context.SaveChangesAsync();
        }
        if (!context.AccountSettings.Any())
        {
            var account = await context.Accounts.FirstAsync();
            context.AccountSettings.Add(new AccountSettings(account.AccountId));
            await context.SaveChangesAsync();
        }
        if (!context.Users.Any())
        {
            var account = await context.Accounts.FirstAsync();
            context.Users.Add(new User(PlatformBootstrap.AdministratorUserId, "Administrator", true, account.AccountId));
            await context.SaveChangesAsync();
        }
        if (!context.UserSettings.Any())
        {
            var user = await context.Users.FirstAsync();
            context.UserSettings.Add(new UserSettings(user.UserId));
            await context.SaveChangesAsync();
        }

        // One-time backfill (idempotent; kept for not-yet-migrated environments):
        // platform default notification templates are NOT seeded: localized
        // text never lives in the database. Defaults come from the NotificationDefaultMessages
        // resources at render time; the templates table holds account-authored overrides only.
        // Remove rows an earlier initializer version may have seeded.
        var seededDefaults = await context.NotificationTemplates
            .AsTracking().Where(t => t.AccountId == null).ToListAsync();
        if (seededDefaults.Count > 0)
        {
            context.NotificationTemplates.RemoveRange(seededDefaults);
            await context.SaveChangesAsync();
        }

        // One-time backfill (idempotent; kept for not-yet-migrated environments):
        // rule CRUD is now gated by the `notifications` feature, so every account that already has
        // NotificationRule rows gets an enabled feature row. Runs through the entity writer path,
        // never raw SQL.
        var ruleAccounts = await context.NotificationRules.Select(r => r.AccountId).Distinct().ToListAsync();
        var gatedAccounts = await context.AccountFeatures
            .Where(f => f.FeatureKey == FeatureKeys.Notifications)
            .Select(f => f.AccountId)
            .Distinct()
            .ToListAsync();
        var missing = ruleAccounts.Except(gatedAccounts).ToList();
        if (missing.Count > 0)
        {
            foreach (var accountId in missing)
            {
                context.AccountFeatures.Add(new AccountFeature(accountId, FeatureKeys.Notifications, true, "standard", "migration", null, null, null));
            }
            await context.SaveChangesAsync();
        }
    }
}
