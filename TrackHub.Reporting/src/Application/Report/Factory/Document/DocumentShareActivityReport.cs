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

// Document share-activity report: every Document public-link grant with its access count,
// last-access, expiry, and revocation. Filters the account's grants to ResourceType = "Document".
public sealed class DocumentShareActivityReport(IDocumentReportReader reader, IReportNameReader names) : IReport
{
    private const string DocumentResourceType = "Document";

    public string ReportCode => DocumentReportCodes.ShareActivity;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureDocumentsFeatureAsync(cancellationToken);

        var shares = await reader.GetDocumentSharesByAccountAsync(cancellationToken);

        var documentShares = shares
            .Where(s => string.Equals(s.ResourceType, DocumentResourceType, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var documents = await names.GetDocumentNamesAsync(
            documentShares.Select(s => Guid.TryParse(s.ResourceId, out var id) ? id : Guid.Empty).Where(id => id != Guid.Empty), cancellationToken);

        var rows = documentShares
            .OrderByDescending(s => s.AccessCount)
            .ThenBy(s => s.ResourceId, StringComparer.OrdinalIgnoreCase)
            .Select(s => new DocumentShareActivityRowVm(documents.NameOf(s.ResourceId), s.Scopes, s.Purpose, s.AccessCount, s.LastAccessedAt, s.ExpiresAt, s.RevokedAt))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
