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

using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Document;

// Expiring-documents report. Active documents with an ExpiresAt inside the window
// (withinDays, default 30). Owner-visibility + classification are enforced by the Manager query.
public sealed class ExpiringDocumentsReport(IDocumentReportReader reader, IReportNameReader names) : IReport
{
    private const string StaffOwner = "Staff";

    public string ReportCode => DocumentReportCodes.ExpiringDocuments;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureDocumentsFeatureAsync(cancellationToken);

        var withinDays = (int)(filters.GetNumber(FilterNames.WithinDays) ?? 30);
        var documents = await reader.GetExpiringDocumentsAsync(withinDays, cancellationToken);

        var transporters = await names.GetTransporterNamesAsync(
            documents.Where(d => !IsStaff(d.OwnerEntityType))
                .Select(d => Guid.TryParse(d.OwnerEntityId, out var id) ? id : (Guid?)null)
                .OfType<Guid>(),
            cancellationToken);
        var staff = documents.Any(d => IsStaff(d.OwnerEntityType)) ? await names.GetStaffNamesAsync(cancellationToken) : new Dictionary<Guid, string>();
        var rows = documents
            .OrderBy(d => d.ExpiresAt ?? DateTimeOffset.MaxValue)
            .ThenBy(d => d.Category, StringComparer.OrdinalIgnoreCase)
            .Select(d => new ExpiringDocumentRowVm(
                d.Category,
                d.OwnerEntityType,
                (IsStaff(d.OwnerEntityType) ? staff : transporters).NameOf(d.OwnerEntityId),
                d.FileName,
                d.Classification,
                d.Status,
                d.ExpiresAt))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }

    private static bool IsStaff(string ownerType) => string.Equals(ownerType, StaffOwner, StringComparison.OrdinalIgnoreCase);
}
