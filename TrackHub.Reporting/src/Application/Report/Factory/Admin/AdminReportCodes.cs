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

namespace TrackHub.Reporting.Application.Report.Factory.Admin;

// Report codes for the lifecycle/branding reports. Kept as local literals rather than in
// the shared Common.Domain `Reports` constants to avoid a TrackHubCommon repack for report codes.
internal static class AdminReportCodes
{
    public const string AccountsByStatus = "accounts-by-status";
    public const string FeatureEnablementMatrix = "feature-enablement-matrix";
    public const string GroupMembership = "group-membership-export";
}
