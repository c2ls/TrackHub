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

namespace TrackHub.Reporting.Domain.Models;

// Intermediate projections deserialized from Manager GraphQL for the workforce reports (spec 09 §13).
// Date-only fields stay DateOnly to match Manager's `Date` scalar; the row VMs widen them to a UTC
// DateTimeOffset so Excel/PDF apply the shared date column format.

public readonly record struct ReportDriverVm(
    Guid DriverId, string Name, string? Phone, string? DocumentType, string? DocumentNumber,
    bool Active, string? EmployeeCode, string? LicenseNumber, DateOnly? LicenseExpiresAt,
    Guid? DefaultTransporterId);

public readonly record struct ReportDriverQualificationVm(
    Guid DriverQualificationId, Guid DriverId, string DriverName, string QualificationType,
    string? Category, string? Number, DateOnly? IssuedAt, DateOnly? ExpiresAt,
    string? IssuingAuthority, string Status);

public readonly record struct ReportDriverAssignmentVm(
    Guid DriverId, string DriverName, Guid TransporterId, string TransporterName,
    DateTimeOffset StartsAt, DateTimeOffset? EndsAt, string AssignmentType, string Status,
    string CreatedByPrincipal);

// ---- Row VMs (property order = Excel column order; property name = resx header key) ----

public readonly record struct DriverRegistryRowVm(
    string DriverName, string EmployeeCode, string DocumentType, string DocumentNumber, string Phone,
    string LicenseNumber, DateTimeOffset? LicenseExpiresAt, string DefaultTransporterId, bool Active);

public readonly record struct QualificationExpirationRowVm(
    // `LicenseCategory` (not `Category`) because the resx key `Category` is shared with the document
    // reports and resolves to "Type"/"Tipo" — next to `QualificationType` that produced two adjacent
    // columns both headed "Type". This column carries the licence category (A1–C3 per spec 09 §6).
    string DriverName, string QualificationType, string LicenseCategory, string Number,
    string IssuingAuthority, DateTimeOffset? IssuedAt, DateTimeOffset? ExpiresAt,
    int? DaysRemaining, string Status);

public readonly record struct DriverAssignmentHistoryRowVm(
    string DriverName, string TransporterName, string AssignmentType, string Status,
    DateTimeOffset StartsAt, DateTimeOffset? EndsAt, int DurationDays, string CreatedByPrincipal);
