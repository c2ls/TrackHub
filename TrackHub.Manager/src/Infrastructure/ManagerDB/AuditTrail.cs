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
using TrackHub.Manager.Infrastructure.Entities;

namespace TrackHub.Manager.Infrastructure.ManagerDB;

/// <summary>
/// Builds the audit row for a mutation. Lives outside <see cref="AccountScopedDataAccess"/> so the
/// writers that carry their own access rule (the identity replicas) record the same shape.
/// </summary>
internal static class AuditTrail
{
    internal static AuditEvent Create(
        ICurrentPrincipal principal,
        Guid accountId,
        string action,
        string resourceType,
        string resourceId,
        string? oldValuesJson,
        string? newValuesJson)
        => new(
            accountId,
            principal.PrincipalType.ToString(),
            principal.UserId?.ToString() ?? principal.StaffId?.ToString() ?? principal.ClientId ?? principal.SubjectId ?? "unknown",
            action,
            resourceType,
            resourceId,
            "Succeeded",
            oldValuesJson,
            newValuesJson,
            null,
            null,
            null,
            principal.CorrelationId);
}
