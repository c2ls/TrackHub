// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

using System.Text.Json;
using Common.Application.Attributes;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Reporting.Domain.Exceptions;
using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Factory;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Options;
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Application.Report.Queries.Get;

// Filter values may carry entity ids (e.g. transporterId); scope is enforced by the data
// feeds — every reader queries the source service under the caller's token, and the source
// applies its own account/visibility scoping. A foreign id therefore yields no rows.
[Authorize(Resource = Resources.Reports, Action = Actions.Read)]
[AccountScopeEnforcedInHandler]
public readonly record struct GetReportQuery(string ReportCode, FilterDto Filters, string Format = "xlsx")
    : IRequest<ReportFileVm>
{
    public const string FormatXlsx = "xlsx";
    public const string FormatPdf = "pdf";
}

public class GetReportQueryHandler(
    IReportFactory factory,
    IUser user,
    IReportAuditWriter reportAuditWriter,
    IReportCatalogReader catalogReader,
    IAccountFeatureReader featureReader,
    IReportBrandingReader brandingReader,
    IExcelHelper excelHelper,
    IPdfReportBuilder pdfReportBuilder,
    ReportingLimitsOptions limits)
        : IRequestHandler<GetReportQuery, ReportFileVm>
{
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string PdfContentType = "application/pdf";

    /// <summary>
    /// Executes the governed report pipeline: authorize the account, normalize the
    /// requested format, resolve + enforce catalog metadata (feature key, manager-only), run the report
    /// into a dataset, render it to the requested format (enforcing the row limits), and audit the export
    /// with the real format and row count.
    /// </summary>
    public async Task<ReportFileVm> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        var accountId = user.AccountId ?? throw new UnauthorizedAccessException();

        var format = (request.Format ?? GetReportQuery.FormatXlsx).Trim().ToLowerInvariant();
        if (format is not GetReportQuery.FormatXlsx and not GetReportQuery.FormatPdf)
        {
            throw new UnsupportedReportFormatException(format);
        }

        var metadata = await ReportAccessGuard.EnsureAccessAsync(
            catalogReader, featureReader, user, accountId, request.ReportCode, cancellationToken);

        if (format == GetReportQuery.FormatPdf && !metadata.SupportsPdf)
        {
            throw UnsupportedReportFormatException.ExcelOnly(format);
        }

        var dataset = await factory.GetReport(request.ReportCode).GetDatasetAsync(request.Filters, cancellationToken);
        var culture = ReportCulture.Resolve(request.Filters.Language);

        byte[] content;
        string contentType;
        string extension;
        if (format == GetReportQuery.FormatPdf)
        {
            if (dataset.RowCount > limits.MaxPdfRows)
            {
                throw ReportLimitExceededException.ForPdf(limits.MaxPdfRows);
            }

            // Brand the PDF with the account's name/logo. Null-tolerant: a missing or
            // failed branding lookup returns null and the PDF renders without the branding block.
            var branding = await brandingReader.GetBrandingAsync(accountId, cancellationToken);
            if (branding is { } brand)
            {
                dataset = dataset.WithBranding(brand.AccountName, brand.LogoImage);
            }

            content = pdfReportBuilder.Build(dataset, culture);
            contentType = PdfContentType;
            extension = ".pdf";
        }
        else
        {
            content = excelHelper.Export(dataset, culture);
            contentType = XlsxContentType;
            extension = ".xlsx";
        }

        await reportAuditWriter.RecordReportExportAsync(
            accountId,
            user.PrincipalType.ToString(),
            user.UserId?.ToString() ?? user.ClientId ?? user.SubjectId ?? string.Empty,
            request.ReportCode,
            JsonSerializer.Serialize(request.Filters),
            dataset.RowCount,
            format,
            user.CorrelationId,
            cancellationToken);

        return new ReportFileVm(content, contentType, extension, dataset.RowCount);
    }
}
