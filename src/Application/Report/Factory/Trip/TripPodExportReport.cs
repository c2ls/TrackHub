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
using TrackHub.Reporting.Domain.Interfaces.Trip;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Factory.Trip;

// Proof-of-delivery register (spec 11 §13). Filters: DateTimeFilter1/2 = window, StringFilter1 =
// transporter id. Most recent capture first. Excel only.
//
// SENSITIVE EXPORT. It carries receiver names, receiver identity documents and the coordinates where
// the signature was captured. Spec 06 §13 / spec 11 §13 call for a HIGH-SEVERITY export audit; the
// platform has no severity dimension to record it on yet — `IReportAuditWriter.RecordReportExportAsync`
// takes no severity, and neither `ReportMetadataVm` nor the Manager `Report` catalog row carries a
// sensitivity column, so severity is not derivable from the catalog either. That is finding SC-06
// (open), whose approved remedy is specs/24a-audit-event-classification.md, NOT a local flag on this
// report. Until 24a lands this export audits exactly like every other one — the same position the
// three equally sensitive `workforce-*` PII exports are in. Do not invent a divergent local
// mechanism here; wire this report's severity when 24a delivers the classification axis.
public sealed class TripPodExportReport(ITripReportReader reader) : IReport
{
    public string ReportCode => TripReportCodes.PodExport;

    public async Task<ReportDataset> GetDatasetAsync(FilterDto filters, CancellationToken cancellationToken)
    {
        await reader.EnsureTripManagementFeatureAsync(cancellationToken);

        var transporterId = TripReportSupport.ParseOptionalId(filters.StringFilter1);

        var pods = await reader.GetTripProofsOfDeliveryAsync(
            filters.DateTimeFilter1, filters.DateTimeFilter2, transporterId, driverId: null, cancellationToken);

        var rows = pods
            .OrderByDescending(p => p.CapturedAt)
            .ThenBy(p => p.TripCode, StringComparer.OrdinalIgnoreCase)
            .ThenBy(p => p.StopSequence)
            .Select(p => new TripPodExportRowVm(
                p.TripCode,
                p.StopSequence,
                p.StopName,
                p.ReceiverName,
                p.ReceiverDocument.OrEmpty(),
                p.CapturedAt,
                p.Latitude,
                p.Longitude,
                p.DocumentCount))
            .ToList();

        return ReportDataset.Create(filters, rows);
    }
}
