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

public static class FeatureKeys
{
    public const string Geofencing = "geofencing";
    public const string TripManagement = "trip-management";
    public const string DriverMobile = "driver-mobile";
    public const string PublicLinks = "public-links";
    public const string Documents = "documents";
    public const string Notifications = "notifications";
    public const string NotificationsEmail = "notifications.email";
    public const string NotificationsWhatsApp = "notifications.whatsapp";
    public const string GpsIntegration = "gps.integration";
    public const string GpsPositionHistory = "gps.positionHistory";

    /// <summary>
    /// Extended workforce capabilities (driver qualifications, time-bounded assignment history,
    /// expiration alerts, workforce reports). Driver identity itself — registry, credentials and
    /// device administration — is core platform and is deliberately NOT gated by this key.
    /// </summary>
    public const string Workforce = "workforce";
}
