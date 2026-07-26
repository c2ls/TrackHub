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
    // Canonical catalog of all backend factory reports with their governance metadata.
    // Category grouping, RequiredFeatureKey gating (null = global), ManagerOnly role gating and
    // SupportsPdf format support. Common report-code constants where they exist; the Reporting-local
    // document/admin codes are string literals (they never became Common constants).
    private static readonly (string Code, string Description, string Category, string? RequiredFeatureKey, bool ManagerOnly, bool SupportsPdf, int SortOrder)[] ReportCatalog =
    [
        // Operations (global + geofencing-gated)
        (Reports.LiveReport, "Live Report", "Operations", null, false, false, 10),
        (Reports.PositionRecord, "Position Record", "Operations", null, false, false, 20),
        (Reports.TransportersInGeofence, "Transporters In Geofence", "Operations", FeatureKeys.Geofencing, false, false, 30),
        (Reports.GeofenceEvents, "Geofence Events", "Operations", FeatureKeys.Geofencing, false, false, 40),

        // GPS integration
        (Reports.GpsProviderHealthSummary, "GPS Provider Health Summary", "Gps", FeatureKeys.GpsIntegration, true, true, 10),
        (Reports.GpsProviderSyncHistory, "GPS Provider Sync History", "Gps", FeatureKeys.GpsIntegration, false, false, 20),
        (Reports.GpsSyncStatistics, "GPS Sync Statistics", "Gps", FeatureKeys.GpsIntegration, false, false, 30),
        (Reports.GpsSynchronizedDeviceInventory, "GPS Synchronized Device Inventory", "Gps", FeatureKeys.GpsIntegration, false, false, 40),
        (Reports.GpsRecentlyAddedDevices, "GPS Recently Added Devices", "Gps", FeatureKeys.GpsIntegration, false, false, 50),
        (Reports.GpsUnassignedDevices, "GPS Unassigned Devices", "Gps", FeatureKeys.GpsIntegration, false, false, 60),
        (Reports.GpsIgnoredDevices, "GPS Ignored Devices", "Gps", FeatureKeys.GpsIntegration, false, false, 70),
        (Reports.GpsAssignmentHistory, "GPS Assignment History", "Gps", FeatureKeys.GpsIntegration, false, false, 80),
        (Reports.GpsLatestPositionFreshness, "GPS Latest Position Freshness", "Gps", FeatureKeys.GpsIntegration, false, false, 90),
        (Reports.GpsPositionHistory, "GPS Position History", "Gps", FeatureKeys.GpsPositionHistory, false, false, 100),

        // Documents — Reporting-local codes
        ("documents-expiring", "Documents expiring within a window", "Documents", FeatureKeys.Documents, false, true, 10),
        ("documents-missing-required", "Transporters missing required documents", "Documents", FeatureKeys.Documents, false, true, 20),
        ("documents-share-activity", "Document share activity", "Documents", FeatureKeys.Documents, false, false, 30),
        ("documents-upload-volume", "Document upload volume", "Documents", FeatureKeys.Documents, false, false, 40),

        // Workforce — Reporting-local codes. Driver personal data, so gated on the `workforce` key and
        // ManagerOnly: the feeds require Drivers/Read, which only the Manager role holds. Widening the
        // dispatcher (User) role to read driver records is the wrong trade for a report (SC-07).
        ("workforce-driver-registry", "Driver registry export", "Workforce", FeatureKeys.Workforce, true, false, 10),
        ("workforce-qualification-expirations", "Driver qualifications expiring within a window", "Workforce", FeatureKeys.Workforce, true, true, 20),
        ("workforce-assignment-history", "Driver to transporter assignment history", "Workforce", FeatureKeys.Workforce, true, false, 30),

        // Trips — Reporting-local codes. Dispatch execution data, gated on the `trip-management` key.
        // Dispatcher-facing, so not ManagerOnly: the feeds require Trips/Export, which the User role holds.
        // trip-pod-export carries receiver names, identity documents and signature coordinates, but the
        // dispatcher owns the trip that produced them; the control on bulk PII export is the export audit.
        ("trip-summary", "Trip summary by period", "Trips", FeatureKeys.TripManagement, false, false, 10),
        ("trip-detail", "Trip stop-level detail", "Trips", FeatureKeys.TripManagement, false, false, 20),
        ("trip-on-time-performance", "Trip on-time performance", "Trips", FeatureKeys.TripManagement, false, true, 30),
        ("trip-stop-dwell", "Trip stop dwell distribution", "Trips", FeatureKeys.TripManagement, false, false, 40),
        ("trip-toll-cost", "Estimated toll cost by trip", "Trips", FeatureKeys.TripManagement, false, true, 50),
        ("trip-pod-export", "Proof-of-delivery register", "Trips", FeatureKeys.TripManagement, false, false, 60),

    // Administration (global + manager-only) — Reporting-local codes
        ("accounts-by-status", "Accounts by lifecycle status", "Administration", null, true, true, 10),
        ("feature-enablement-matrix", "Feature enablement matrix across accounts", "Administration", null, true, true, 20),
        ("group-membership-export", "Group membership export", "Administration", null, true, false, 30),
    ];

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
        foreach (var (code, description, category, requiredFeatureKey, managerOnly, supportsPdf, sortOrder) in ReportCatalog)
        {
            if (reportsByCode.TryGetValue(code, out var report))
            {
                report.Description = description;
                report.Category = category;
                report.RequiredFeatureKey = requiredFeatureKey;
                report.ManagerOnly = managerOnly;
                report.SupportsPdf = supportsPdf;
                report.SortOrder = sortOrder;
            }
            else
            {
                context.Reports.Add(new Report(code, description, reportType, true, category, requiredFeatureKey, managerOnly, supportsPdf, sortOrder));
            }
        }
        await context.SaveChangesAsync();
        if (!context.TransporterTypes.Any())
        {
            // Seed every TransporterType enum value with identical defaults
            // (not custom, small icon 10x10, 120s expiration). HeavyEquipment is the only
            // type flagged custom (second ctor arg true).
            foreach (var transporterType in Enum.GetValues<TransporterType>())
            {
                var isCustom = transporterType == TransporterType.HeavyEquipment;
                context.TransporterTypes.Add(new TrackHub.Manager.Infrastructure.Entities.TransporterType(
                    (short)transporterType, isCustom, 10, 10, 120));
            }
            await context.SaveChangesAsync();
        }
        if (!context.Accounts.Any())
        {
            var accountType = (short)AccountType.Personal;
            context.Accounts.Add(new Account("Master Account", "Master Account Description", accountType, true));
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
            context.Users.Add(new User(Guid.NewGuid(), "Administrator", true, account.AccountId));
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
        var seededDefaults = await context.NotificationTemplates.Where(t => t.AccountId == null).ToListAsync();
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
