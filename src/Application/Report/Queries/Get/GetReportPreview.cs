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

using Common.Application.Attributes;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Options;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Queries.Get;

// Same scope declaration as GetReportQuery: filter values may carry entity ids, and the
// data feeds enforce account/visibility scope under the caller's token.
[Authorize(Resource = Resources.Reports, Action = Actions.Read)]
[AccountScopeEnforcedInHandler]
public readonly record struct GetReportPreviewQuery(string ReportCode, FilterDto Filters)
    : IRequest<ReportPreviewVm>;

public class GetReportPreviewQueryHandler(
    IReportFactory factory,
    IUser user,
    IReportCatalogReader catalogReader,
    IAccountFeatureReader featureReader,
    ReportingLimitsOptions limits)
        : IRequestHandler<GetReportPreviewQuery, ReportPreviewVm>
{
    /// <summary>
    /// On-screen preview: same account/catalog/feature/role enforcement as the
    /// export, then returns the first PreviewRows rows plus accurate total/truncation metadata. Not audited
    /// (no file leaves the service).
    /// </summary>
    public async Task<ReportPreviewVm> Handle(GetReportPreviewQuery request, CancellationToken cancellationToken)
    {
        var accountId = user.AccountId ?? throw new UnauthorizedAccessException();

        await ReportAccessGuard.EnsureAccessAsync(
            catalogReader, featureReader, user, accountId, request.ReportCode, cancellationToken);

        var dataset = await factory.GetReport(request.ReportCode).GetDatasetAsync(request.Filters, cancellationToken);
        var culture = ReportCulture.Resolve(request.Filters.Language);

        var columns = dataset.Columns
            .Select(c => new ReportPreviewColumn(c.PropertyName, ReportHeaderResolver.Resolve(c.PropertyName, culture)))
            .ToArray();

        var rows = dataset.Rows.Take(limits.PreviewRows).ToArray();
        var total = dataset.RowCount;

        return new ReportPreviewVm(columns, rows, total, total > limits.PreviewRows);
    }
}
