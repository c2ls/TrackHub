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

namespace Common.Domain.Constants;

public static partial class Reports
{
    public const string LiveReport = nameof(LiveReport);
    public const string PositionRecord = nameof(PositionRecord);
    public const string TransportersInGeofence = nameof(TransportersInGeofence);
    public const string GeofenceEvents = nameof(GeofenceEvents);

    // GPS integration reports
    public const string GpsProviderHealthSummary = "gps.provider-health-summary";
    public const string GpsProviderSyncHistory = "gps.provider-sync-history";
    public const string GpsSyncStatistics = "gps.sync-statistics";
    public const string GpsSynchronizedDeviceInventory = "gps.synchronized-device-inventory";
    public const string GpsRecentlyAddedDevices = "gps.recently-added-devices";
    public const string GpsUnassignedDevices = "gps.unassigned-devices";
    public const string GpsIgnoredDevices = "gps.ignored-devices";
    public const string GpsAssignmentHistory = "gps.assignment-history";
    public const string GpsLatestPositionFreshness = "gps.latest-position-freshness";
    public const string GpsPositionHistory = "gps.position-history";
}
