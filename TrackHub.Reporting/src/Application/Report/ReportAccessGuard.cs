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
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Exceptions;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Application.Report;

// Execution-time catalog visibility enforcement shared by the export and preview handlers
//: resolve metadata, enforce the required feature key, and enforce the
// manager-only role restriction. A hidden report invoked by code fails here — never reaches its data.
internal static class ReportAccessGuard
{
    public static async Task<ReportMetadataVm> EnsureAccessAsync(
        IReportCatalogReader catalogReader,
        IAccountFeatureReader featureReader,
        IUser user,
        Guid accountId,
        string reportCode,
        CancellationToken cancellationToken)
    {
        var metadata = await catalogReader.GetReportByCodeAsync(reportCode, cancellationToken);
        if (metadata is not { Active: true })
        {
            throw new ReportNotFoundException(reportCode);
        }

        var meta = metadata.Value;
        if (!string.IsNullOrEmpty(meta.RequiredFeatureKey))
        {
            await featureReader.EnsureFeatureEnabledAsync(accountId, meta.RequiredFeatureKey, cancellationToken);
        }

        if (meta.ManagerOnly && !IsManager(user.Role))
        {
            throw new ReportAccessDeniedException(reportCode);
        }

        return meta;
    }

    public static bool IsManager(string? role)
        => string.Equals(role, Roles.Administrator, StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, Roles.Manager, StringComparison.OrdinalIgnoreCase);
}
