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

using System.Globalization;
using ClosedXML.Excel;
using Common.Domain.Extensions;
using TrackHub.Reporting.Domain.Exceptions;
using TrackHub.Reporting.Domain.Interfaces.Helpers;
using TrackHub.Reporting.Domain.Models;
using TrackHub.Reporting.Domain.Options;

namespace TrackHub.Reporting.Domain.Helpers;

/// <summary>
/// Helper class for exporting a <see cref="ReportDataset"/> to Excel format. Produces output equivalent
/// to the pre-refactor generic pipeline: title cell A1, a resx-localized header row, typed/formatted
/// data rows, and a single ClosedXML table on a sheet named "Report".
/// </summary>
public sealed class ExcelHelper(ReportingLimitsOptions limits) : IExcelHelper
{
    private static readonly string[] CoordinatesFields = ["Latitude", "Longitude"];

    public byte[] Export(ReportDataset dataset, CultureInfo culture)
    {
        if (dataset.RowCount > limits.MaxExportRows)
            throw new ReportLimitExceededException(limits.MaxExportRows);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");

        worksheet.Cell("A1").Value = GetDateLabel(dataset.FromDate, dataset.ToDate, dataset.Title);
        worksheet.Cell("A1").Style.Font.SetBold(true);
        worksheet.Cell("A1").Style.Font.FontSize = 16;

        var columns = dataset.Columns;
        var columnCount = columns.Count;
        if (columnCount == 0)
        {
            // No columns to project — return a workbook carrying just the title.
            using var titleOnly = new MemoryStream();
            workbook.SaveAs(titleOnly);
            return titleOnly.ToArray();
        }

        // Header row at row 2 (property names first, so the table's field names stay unique), data below.
        for (var col = 0; col < columnCount; col++)
        {
            worksheet.Cell(2, col + 1).Value = columns[col].PropertyName;
        }

        for (var row = 0; row < dataset.Rows.Count; row++)
        {
            var values = dataset.Rows[row];
            for (var col = 0; col < columnCount; col++)
            {
                SetCell(worksheet.Cell(3 + row, col + 1), values[col]);
            }
        }

        var tableRange = worksheet.Range(2, 1, 2 + dataset.Rows.Count, columnCount);
        var table = tableRange.CreateTable();
        table.Theme = XLTableTheme.TableStyleMedium9;

        // Per-type column formats + localized headers (overwriting the property-name placeholders).
        for (var col = 0; col < columnCount; col++)
        {
            SetColumnFormat(worksheet, col + 1, columns[col].PropertyType, columns[col].PropertyName);
            table.Field(col).Name = ReportHeaderResolver.Resolve(columns[col].PropertyName, culture);
        }

        worksheet.Range(1, 1, 1, columnCount).Merge();
        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    // Writes an arbitrary boxed cell value using ClosedXML's typed conversions so the per-column number
    // and date formats apply (DateTimeOffset is normalized to its UTC DateTime, per the UTC-everywhere rule).
    private static void SetCell(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                break;
            case string s:
                cell.Value = s;
                break;
            case bool b:
                cell.Value = b;
                break;
            case DateTimeOffset dto:
                cell.Value = dto.UtcDateTime;
                break;
            case DateTime dt:
                cell.Value = dt;
                break;
            case double d:
                cell.Value = d;
                break;
            case float f:
                cell.Value = f;
                break;
            case decimal m:
                cell.Value = m;
                break;
            case int i:
                cell.Value = i;
                break;
            case long l:
                cell.Value = l;
                break;
            case short sh:
                cell.Value = sh;
                break;
            case byte bt:
                cell.Value = bt;
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    /// <summary>
    /// Sets the format of a column based on the property type (unchanged from the pre-refactor helper).
    /// </summary>
    private static void SetColumnFormat(IXLWorksheet worksheet, int colNumber, Type propertyType, string propertyName)
    {
        switch (propertyType)
        {
            case Type t when t == typeof(DateTimeOffset) || t == typeof(DateTimeOffset?):
                worksheet.Column(colNumber).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
                break;
            case Type t when t == typeof(double) || t == typeof(decimal):
                worksheet.Column(colNumber).Style.NumberFormat.Format
                    = CoordinatesFields.Contains(propertyName) ? "0.00000" : "0.00";
                break;
            case Type t when t == typeof(int):
                worksheet.Column(colNumber).Style.NumberFormat.Format = "#";
                break;
            case Type t when t == typeof(bool):
                worksheet.Column(colNumber).Style.NumberFormat.Format = "@";
                break;
            case Type t when t == typeof(string):
                worksheet.Column(colNumber).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                break;
            default:
                // do nothing
                break;
        }
    }

    /// <summary>
    /// Generates a label for the date range of the report (unchanged from the pre-refactor helper).
    /// </summary>
    private static string GetDateLabel(DateTimeOffset? fromDate, DateTimeOffset? toDate, string title)
        => toDate != null
            ? $"{title} - ({fromDate.FormatDateTime()} - {toDate.FormatDateTime()})"
            : fromDate != null ? $"{title} - ({fromDate.FormatDate()})" : title;
}
