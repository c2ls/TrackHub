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

// Missing-required-documents report. For each group-visible transporter, lists the
// required document types (enabled + required) that lack an Active document. Three Manager calls total:
// the feature gate, the document types, and one batched compliance read (owner visibility enforced
// server-side — previously one documentsForOwner call per transporter).
public sealed class MissingRequiredDocumentsReport(IDocumentReportReader reader) : IReport
{
    private const string OwnerTypeTransporter = "Transporter";

    public string ReportCode => DocumentReportCodes.MissingRequiredDocuments;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureDocumentsFeatureAsync(cancellationToken);

        var requiredCategories = (await reader.GetDocumentTypesAsync(cancellationToken))
            .Where(t => t.Required && t.Enabled)
            .Select(t => t.Category)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rows = new List<MissingRequiredDocumentRowVm>();
        if (requiredCategories.Count == 0)
        {
            return ReportDataset.Create(filters, rows);
        }

        var compliance = await reader.GetTransporterDocumentComplianceAsync(cancellationToken);
        foreach (var transporter in compliance.OrderBy(t => t.TransporterName, StringComparer.OrdinalIgnoreCase))
        {
            var activeCategories = transporter.ActiveCategories.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var category in requiredCategories.Where(c => !activeCategories.Contains(c)).OrderBy(c => c, StringComparer.OrdinalIgnoreCase))
            {
                rows.Add(new MissingRequiredDocumentRowVm(OwnerTypeTransporter, transporter.TransporterName, transporter.TransporterId.ToString(), category));
            }
        }

        return ReportDataset.Create(filters, rows);
    }
}
