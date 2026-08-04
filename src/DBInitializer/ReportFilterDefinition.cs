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

namespace DBInitializer;

/// <summary>
/// One filter a report's UI form exposes, seeded as JSON into the catalog's
/// <c>filters</c> column (camelCase, nulls omitted). <c>Name</c> is the key the
/// portal sends back in the report request's named filter values; the Reporting
/// factories read the same key. <c>Type</c> is <c>text</c> | <c>guid</c> |
/// <c>datetime</c> | <c>number</c>. <c>Source</c> names the portal picker list
/// (<c>transporters</c> | <c>operators</c> | <c>geofences</c> | <c>accountStatus</c>;
/// null = free input by type). <c>LabelKey</c> is a portal i18n resource key —
/// never a localized string. Every filter is optional by contract: an absent or
/// empty value means "no filter" (rendered as "All" in pickers).
/// </summary>
internal sealed record ReportFilterDefinition(string Name, string Type, string LabelKey, string? Source = null);

/// <summary>
/// The shared filter vocabulary report rows compose their filter sets from. Reports
/// reusing a semantic (e.g. a transporter picker) must reuse the same definition so
/// filter names stay uniform across the catalog.
/// </summary>
internal static class ReportFilter
{
    public static readonly ReportFilterDefinition Transporter = new("transporterId", "guid", "reports.transporter", "transporters");
    public static readonly ReportFilterDefinition Operator = new("operatorId", "guid", "reports.operator", "operators");
    public static readonly ReportFilterDefinition Geofence = new("geofenceId", "guid", "reports.geofence", "geofences");
    // Free-text GUID: there is no device picker source in the portal.
    public static readonly ReportFilterDefinition Device = new("deviceId", "guid", "reports.device");
    public static readonly ReportFilterDefinition Status = new("status", "text", "reports.status", "accountStatus");
    public static readonly ReportFilterDefinition From = new("from", "datetime", "reports.from");
    public static readonly ReportFilterDefinition To = new("to", "datetime", "reports.to");
    public static readonly ReportFilterDefinition MaxRows = new("maxRows", "number", "reports.maxRows");
    public static readonly ReportFilterDefinition WithinDays = new("withinDays", "number", "reports.withinDays");
    public static readonly ReportFilterDefinition LookbackHours = new("lookbackHours", "number", "reports.lookbackHours");
}
