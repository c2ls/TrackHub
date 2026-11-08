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

using System.Globalization;
using Common.Domain.Time;
using Common.Pdf;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Helpers;

/// <summary>
/// Renders a <see cref="ReportDataset"/> to one A4 tabular PDF: account branding, title, generation
/// stamp, applied filters, a content-sized table whose header repeats on every page, and page numbers.
/// Wide datasets switch to landscape. One shared layout — no per-report PDF.
/// </summary>
public sealed class PdfReportBuilder : IPdfReportBuilder
{
    private const int LandscapeFromColumns = 7;
    private const string TimestampFormat = "yyyy-MM-dd HH:mm";
    private static readonly string[] CoordinatesFields = ["Latitude", "Longitude"];

    public byte[] Build(ReportDataset dataset, CultureInfo culture)
    {
        string Label(string key) => ReportHeaderResolver.Resolve(key, culture);

        var page = dataset.Columns.Count >= LandscapeFromColumns ? PdfPageSetup.A4Landscape : PdfPageSetup.A4Portrait;
        var document = new PdfDocumentBuilder(page)
            .Image(dataset.LogoImage, PdfPageSetup.Cm(1.5))
            .ParagraphIf(dataset.AccountName, PdfTextStyle.Subtitle)
            .Paragraph(dataset.Title, PdfTextStyle.Title)
            .Paragraph($"{Label("GeneratedAt")}: {Local(dataset.GeneratedAt, dataset.TimeZone).ToString(TimestampFormat, culture)}")
            .Paragraph($"{Label("TimeZone")}: {dataset.TimeZone.Id}");

        foreach (var filter in dataset.AppliedFilters)
        {
            var value = filter.Value is DateTimeOffset at ? Local(at, dataset.TimeZone).ToString(TimestampFormat, culture) : filter.Value;
            document.Paragraph($"{Label(filter.Key)}: {value}");
        }

        document.Space();
        if (dataset.Columns.Count > 0)
        {
            document.Table(BuildTable(dataset, culture));
        }

        return document
            .Footer((number, total) => $"{Label("Page")} {number} {Label("PageOf")} {total}")
            .Build(new PdfMetadata { Title = dataset.Title, Author = dataset.AccountName, Created = dataset.GeneratedAt.UtcDateTime });
    }

    private static PdfTable BuildTable(ReportDataset dataset, CultureInfo culture)
    {
        var columns = dataset.Columns;
        var table = new PdfTable(columns.Count)
            .HeaderRow([.. columns.Select(column => (PdfCell)ReportHeaderResolver.Resolve(column.PropertyName, culture))]);

        foreach (var values in dataset.Rows)
        {
            var cells = new PdfCell[columns.Count];
            for (var col = 0; col < columns.Count; col++)
            {
                cells[col] = ToCell(values[col], columns[col], culture, dataset.TimeZone);
            }

            table.Row(cells);
        }

        return table;
    }

    private static PdfCell ToCell(object? value, ReportColumn column, CultureInfo culture, AccountTimeZone timeZone)
        => value switch
        {
            byte[] { Length: > 0 } image => PdfCell.Picture(image, PdfPageSetup.Cm(1.0)),
            byte[] => string.Empty,
            double or float or decimal or int or long => new PdfCell(FormatValue(value, column, culture, timeZone), Align: PdfAlign.Right),
            _ => FormatValue(value, column, culture, timeZone)
        };

    private static DateTimeOffset Local(DateTimeOffset value, AccountTimeZone timeZone) => TimeZoneInfo.ConvertTime(value, timeZone.Zone);

    private static string FormatValue(object? value, ReportColumn column, CultureInfo culture, AccountTimeZone timeZone)
        => value switch
        {
            null => string.Empty,
            string s => s,
            DateTimeOffset dto => Local(dto, timeZone).ToString(TimestampFormat, culture),
            DateOnly date => date.ToString("yyyy-MM-dd", culture),
            DateTime dt => dt.ToString(TimestampFormat, culture),
            double d => d.ToString(NumberFormat(column), culture),
            float f => f.ToString(NumberFormat(column), culture),
            decimal m => m.ToString(NumberFormat(column), culture),
            bool b => b.ToString(),
            _ => value.ToString() ?? string.Empty
        };

    private static string NumberFormat(ReportColumn column)
        => CoordinatesFields.Contains(column.PropertyName) ? "0.00000" : "0.00";
}
