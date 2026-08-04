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

using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Manager;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Document;

// Document upload-volume report: document counts by Category over the date window
// (from/to). Uses the group-scoped library search under the caller's token.
public sealed class DocumentUploadVolumeReport(IDocumentReportReader reader) : IReport
{
    public string ReportCode => DocumentReportCodes.UploadVolume;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureDocumentsFeatureAsync(cancellationToken);

        var documents = await reader.SearchDocumentsAsync(filters.GetDate(FilterNames.From), filters.GetDate(FilterNames.To), cancellationToken);

        var rows = documents
            .GroupBy(d => string.IsNullOrWhiteSpace(d.Category) ? "Other" : d.Category, StringComparer.OrdinalIgnoreCase)
            .Select(g => new DocumentUploadVolumeRowVm(g.Key, g.Count()))
            .OrderByDescending(r => r.DocumentCount)
            .ThenBy(r => r.Category, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
