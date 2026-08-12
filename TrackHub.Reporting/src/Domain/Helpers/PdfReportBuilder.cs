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
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Helpers;

/// <summary>
/// Renders a <see cref="ReportDataset"/> to a single clean A4 tabular PDF via PDFsharp-MigraDoc
///: document title, optional account branding (logo + name), generation timestamp,
/// applied-filters block, a table with resx-localized headers repeated on page breaks, page numbers in
/// the footer, and image content cells (byte[] rendered inline). One shared layout — no per-report PDF.
/// </summary>
public sealed class PdfReportBuilder : IPdfReportBuilder
{
    private const string FontName = "Arial";
    private const double PageContentWidthCm = 17.0; // A4 (21cm) minus default left/right margins.

    static PdfReportBuilder()
    {
        // The default PDFsharp Windows/WSL2 font resolvers cover common fonts (e.g. Arial) without
        // shipping a font package. Idempotent — safe to set on every construction.
        GlobalFontSettings.UseWindowsFontsUnderWindows = true;
        GlobalFontSettings.UseWindowsFontsUnderWsl2 = true;
    }

    public byte[] Build(ReportDataset dataset, CultureInfo culture)
    {
        var document = new Document();
        document.Styles["Normal"]!.Font.Name = FontName;

        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;

        AddHeaderBlock(section, dataset, culture);
        AddTable(section, dataset, culture);
        AddFooter(section, culture);

        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    private static void AddHeaderBlock(Section section, ReportDataset dataset, CultureInfo culture)
    {
        if (dataset.LogoImage is { Length: > 0 } logo)
        {
            var logoImage = section.AddImage("base64:" + Convert.ToBase64String(logo));
            logoImage.Height = Unit.FromCentimeter(1.5);
            logoImage.LockAspectRatio = true;
        }

        if (!string.IsNullOrWhiteSpace(dataset.AccountName))
        {
            var account = section.AddParagraph(dataset.AccountName);
            account.Format.Font.Size = 11;
            account.Format.Font.Bold = true;
        }

        var title = section.AddParagraph(dataset.Title);
        title.Format.Font.Size = 16;
        title.Format.Font.Bold = true;
        title.Format.SpaceAfter = Unit.FromPoint(4);

        var generated = section.AddParagraph(
            $"{ReportHeaderResolver.Resolve("GeneratedAt", culture)}: " +
            dataset.GeneratedAt.ToString("yyyy-MM-dd HH:mm", culture));
        generated.Format.Font.Size = 9;

        foreach (var filter in dataset.AppliedFilters)
        {
            var line = section.AddParagraph(
                $"{ReportHeaderResolver.Resolve(filter.Key, culture)}: {filter.Value}");
            line.Format.Font.Size = 9;
        }

        section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(6);
    }

    private static void AddTable(Section section, ReportDataset dataset, CultureInfo culture)
    {
        var columns = dataset.Columns;
        if (columns.Count == 0)
        {
            return;
        }

        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.Format.Font.Size = 8;

        var columnWidth = Unit.FromCentimeter(PageContentWidthCm / columns.Count);
        foreach (var _ in columns)
        {
            table.AddColumn(columnWidth);
        }

        // Header row — repeats at the top of every page (MigraDoc HeadingFormat).
        var header = table.AddRow();
        header.HeadingFormat = true;
        header.Format.Font.Bold = true;
        header.Shading.Color = Colors.LightGray;
        for (var col = 0; col < columns.Count; col++)
        {
            header.Cells[col].AddParagraph(ReportHeaderResolver.Resolve(columns[col].PropertyName, culture));
        }

        foreach (var values in dataset.Rows)
        {
            var row = table.AddRow();
            for (var col = 0; col < columns.Count; col++)
            {
                AddCell(row.Cells[col], columns[col], values[col], culture);
            }
        }
    }

    private static void AddCell(Cell cell, ReportColumn column, object? value, CultureInfo culture)
    {
        if (value is byte[] image)
        {
            if (image.Length > 0)
            {
                var rendered = cell.AddImage("base64:" + Convert.ToBase64String(image));
                rendered.Height = Unit.FromCentimeter(1.0);
                rendered.LockAspectRatio = true;
            }
            return;
        }

        cell.AddParagraph(FormatValue(value, column, culture));
    }

    private static string FormatValue(object? value, ReportColumn column, CultureInfo culture)
        => value switch
        {
            null => string.Empty,
            string s => s,
            DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm", culture),
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm", culture),
            double d => d.ToString(NumberFormat(column), culture),
            float f => f.ToString(NumberFormat(column), culture),
            decimal m => m.ToString(NumberFormat(column), culture),
            bool b => b.ToString(),
            _ => value.ToString() ?? string.Empty
        };

    private static readonly string[] CoordinatesFields = ["Latitude", "Longitude"];

    private static string NumberFormat(ReportColumn column)
        => CoordinatesFields.Contains(column.PropertyName) ? "0.00000" : "0.00";

    private static void AddFooter(Section section, CultureInfo culture)
    {
        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Alignment = ParagraphAlignment.Center;
        footer.Format.Font.Size = 8;
        footer.AddText(ReportHeaderResolver.Resolve("Page", culture) + " ");
        footer.AddPageField();
        footer.AddText(" " + ReportHeaderResolver.Resolve("PageOf", culture) + " ");
        footer.AddNumPagesField();
    }
}
