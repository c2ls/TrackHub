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
using static DBInitializer.ReportFilter;

namespace DBInitializer;

/// <summary>
/// The core platform's factory reports. Common report-code constants where they exist; the
/// Reporting-local document/admin codes are string literals (they never became Common
/// constants). Feature modules add their own <see cref="IReportCatalogContribution"/> files
/// rather than growing this array. Filter sets mirror what each Reporting factory actually
/// reads — a filter listed here but unread by the factory is a silent lie in the UI.
/// </summary>
internal sealed class CoreReportCatalogContribution : IReportCatalogContribution
{
    public IReadOnlyList<(string Code, string Description, string Category, string? RequiredFeatureKey, bool ManagerOnly, bool SupportsPdf, int SortOrder, IReadOnlyList<ReportFilterDefinition> Filters)> Reports { get; } =
    [
        // Operations (global + geofencing-gated)
        (Common.Domain.Constants.Reports.LiveReport, "Live Report", "Operations", null, false, false, 10, []),
        (Common.Domain.Constants.Reports.PositionRecord, "Position Record", "Operations", null, false, false, 20, [Transporter, From, To]),
        (Common.Domain.Constants.Reports.TransportersInGeofence, "Transporters In Geofence", "Operations", FeatureKeys.Geofencing, false, false, 30, []),
        (Common.Domain.Constants.Reports.GeofenceEvents, "Geofence Events", "Operations", FeatureKeys.Geofencing, false, false, 40, [Transporter, Geofence, From, To]),

        // GPS integration
        (Common.Domain.Constants.Reports.GpsProviderHealthSummary, "GPS Provider Health Summary", "Gps", FeatureKeys.GpsIntegration, true, true, 10, [LookbackHours]),
        (Common.Domain.Constants.Reports.GpsProviderSyncHistory, "GPS Provider Sync History", "Gps", FeatureKeys.GpsIntegration, false, false, 20, [Operator, MaxRows, From, To]),
        (Common.Domain.Constants.Reports.GpsSyncStatistics, "GPS Sync Statistics", "Gps", FeatureKeys.GpsIntegration, false, false, 30, [MaxRows, From, To]),
        (Common.Domain.Constants.Reports.GpsSynchronizedDeviceInventory, "GPS Synchronized Device Inventory", "Gps", FeatureKeys.GpsIntegration, false, false, 40, [Operator]),
        (Common.Domain.Constants.Reports.GpsRecentlyAddedDevices, "GPS Recently Added Devices", "Gps", FeatureKeys.GpsIntegration, false, false, 50, [WithinDays, From]),
        (Common.Domain.Constants.Reports.GpsUnassignedDevices, "GPS Unassigned Devices", "Gps", FeatureKeys.GpsIntegration, false, false, 60, []),
        (Common.Domain.Constants.Reports.GpsIgnoredDevices, "GPS Ignored Devices", "Gps", FeatureKeys.GpsIntegration, false, false, 70, [Operator]),
        (Common.Domain.Constants.Reports.GpsAssignmentHistory, "GPS Assignment History", "Gps", FeatureKeys.GpsIntegration, false, false, 80, [Transporter, From, To]),
        (Common.Domain.Constants.Reports.GpsLatestPositionFreshness, "GPS Latest Position Freshness", "Gps", FeatureKeys.GpsIntegration, false, false, 90, []),
        (Common.Domain.Constants.Reports.GpsPositionHistory, "GPS Position History", "Gps", FeatureKeys.GpsPositionHistory, false, false, 100, [Transporter, Device, MaxRows, From, To]),

        // Documents — Reporting-local codes
        ("documents-expiring", "Documents expiring within a window", "Documents", FeatureKeys.Documents, false, true, 10, [WithinDays]),
        ("documents-missing-required", "Transporters missing required documents", "Documents", FeatureKeys.Documents, false, true, 20, []),
        ("documents-share-activity", "Document share activity", "Documents", FeatureKeys.Documents, false, false, 30, []),
        ("documents-upload-volume", "Document upload volume", "Documents", FeatureKeys.Documents, false, false, 40, [From, To]),

        // Workforce — Reporting-local codes. Driver personal data, so gated on the `workforce` key and
        // ManagerOnly: the feeds require Drivers/Read, which only the Manager role holds. Widening the
        // dispatcher (User) role to read driver records is the wrong trade for a report (SC-07).
        ("workforce-driver-registry", "Driver registry export", "Workforce", FeatureKeys.Workforce, true, false, 10, []),
        ("workforce-qualification-expirations", "Driver qualifications expiring within a window", "Workforce", FeatureKeys.Workforce, true, true, 20, [WithinDays]),
        ("workforce-assignment-history", "Driver to transporter assignment history", "Workforce", FeatureKeys.Workforce, true, false, 30, [Transporter, From, To]),

        // Trips — Reporting-local codes. Dispatch execution data, gated on the `trip-management` key.
        // Dispatcher-facing, so not ManagerOnly: the feeds require Trips/Export, which the User role holds.
        // trip-pod-export carries receiver names, identity documents and signature coordinates, but the
        // dispatcher owns the trip that produced them; the control on bulk PII export is the export audit.
        ("trip-summary", "Trip summary by period", "Trips", FeatureKeys.TripManagement, false, false, 10, [Transporter, From, To]),
        ("trip-detail", "Trip stop-level detail", "Trips", FeatureKeys.TripManagement, false, false, 20, [Transporter, From, To]),
        ("trip-on-time-performance", "Trip on-time performance", "Trips", FeatureKeys.TripManagement, false, true, 30, [Transporter, From, To]),
        ("trip-stop-dwell", "Trip stop dwell distribution", "Trips", FeatureKeys.TripManagement, false, false, 40, [Transporter, From, To]),
        ("trip-toll-cost", "Estimated toll cost by trip", "Trips", FeatureKeys.TripManagement, false, true, 50, [Transporter, From, To]),
        ("trip-pod-export", "Proof-of-delivery register", "Trips", FeatureKeys.TripManagement, false, false, 60, [Transporter, From, To]),

        // Administration (global + manager-only) — Reporting-local codes
        ("accounts-by-status", "Accounts by lifecycle status", "Administration", null, true, true, 10, [Status]),
        ("feature-enablement-matrix", "Feature enablement matrix across accounts", "Administration", null, true, true, 20, []),
        ("group-membership-export", "Group membership export", "Administration", null, true, false, 30, []),
    ];
}
