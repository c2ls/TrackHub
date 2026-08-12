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

namespace TrackHub.Manager.Domain.Models;

/// <summary>
/// A driver qualification (license, medical exam, training, background check, HAZMAT permit, ...).
/// <paramref name="DriverName"/> is denormalized so the account-wide expirations view does not need a
/// second round-trip per row. <paramref name="Status"/> carries only the stored lifecycle state —
/// expiry is derived by the consumer from <paramref name="ExpiresAt"/>.
/// </summary>
public readonly record struct DriverQualificationVm(
    Guid DriverQualificationId,
    Guid AccountId,
    Guid DriverId,
    string DriverName,
    string QualificationType,
    string? Category,
    string? Number,
    DateOnly? IssuedAt,
    DateOnly? ExpiresAt,
    string? IssuingAuthority,
    string Status,
    Guid? DocumentId,
    string? Notes,
    DateTimeOffset LastModified);

/// <summary>
/// A time-bounded driver↔transporter assignment. <paramref name="TransporterName"/> is denormalized
/// for the history table.
/// </summary>
public readonly record struct DriverTransporterAssignmentVm(
    Guid DriverTransporterAssignmentId,
    Guid AccountId,
    Guid DriverId,
    string DriverName,
    Guid TransporterId,
    string TransporterName,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    string AssignmentType,
    string Status,
    string CreatedByPrincipal,
    DateTimeOffset LastModified);

/// <summary>
/// Driver self-service view (spec 09 §7.3, consumed by spec 10). The handler pins the driver id from
/// the principal, so this never carries another driver's data.
/// </summary>
public readonly record struct MyDriverProfileVm(
    DriverVm Driver,
    IReadOnlyCollection<DriverQualificationVm> Qualifications,
    IReadOnlyCollection<DriverTransporterAssignmentVm> ActiveAssignments);
