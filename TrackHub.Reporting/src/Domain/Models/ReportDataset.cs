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
using TrackHub.Reporting.Domain.Records;

namespace TrackHub.Reporting.Domain.Models;

// One column of a report dataset. PropertyName is the resx key used to resolve the localized
// header at format time (ExcelHelper / PdfReportBuilder), matching the pre-refactor behavior.
public readonly record struct ReportColumn(string PropertyName, Type PropertyType);

// The format-agnostic tabular result of running a report. Produced once per report
// (IReport.GetDatasetAsync) and consumed by every output format: Excel, PDF, and JSON preview.
public sealed class ReportDataset
{
    public required string Title { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
    public required IReadOnlyList<ReportColumn> Columns { get; init; }
    public required IReadOnlyList<object?[]> Rows { get; init; }
    public IReadOnlyList<KeyValuePair<string, string>> AppliedFilters { get; init; } = [];

    // Optional account branding rendered on the PDF header block when present.
    public string? AccountName { get; init; }
    public byte[]? LogoImage { get; init; }

    public int RowCount => Rows.Count;

    // Returns a copy of this dataset with the account branding block populated. Used by
    // the export pipeline to attach branding fetched from Manager to a PDF export without the report itself
    // needing to know about branding. All other fields (data, columns, filters) are carried over unchanged.
    public ReportDataset WithBranding(string? accountName, byte[]? logoImage) => new()
    {
        Title = Title,
        FromDate = FromDate,
        ToDate = ToDate,
        GeneratedAt = GeneratedAt,
        Columns = Columns,
        Rows = Rows,
        AppliedFilters = AppliedFilters,
        AccountName = accountName,
        LogoImage = logoImage
    };

    // Generic factory: reflects T's public instance properties (declaration order) into Columns and
    // flattens each row into an object?[] in the same order. Title/date range/applied-filters are
    // derived from the FilterDto. This is the single reflection step both Excel and PDF share, so the
    // column order stays identical to the pre-refactor ClosedXML InsertTable output.
    public static ReportDataset Create<T>(
        FilterDto filters,
        IEnumerable<T> rows,
        string? accountName = null,
        byte[]? logoImage = null)
    {
        var properties = typeof(T).GetProperties();
        var columns = new ReportColumn[properties.Length];
        for (var i = 0; i < properties.Length; i++)
        {
            columns[i] = new ReportColumn(properties[i].Name, properties[i].PropertyType);
        }

        var materialized = rows as ICollection<T> ?? [.. rows];
        var data = new List<object?[]>(materialized.Count);
        foreach (var row in materialized)
        {
            var values = new object?[properties.Length];
            for (var i = 0; i < properties.Length; i++)
            {
                values[i] = properties[i].GetValue(row);
            }
            data.Add(values);
        }

        return new ReportDataset
        {
            Title = filters.Name,
            FromDate = filters.GetDate(FilterNames.From),
            ToDate = filters.GetDate(FilterNames.To),
            GeneratedAt = DateTimeOffset.UtcNow,
            Columns = columns,
            Rows = data,
            AppliedFilters = BuildAppliedFilters(filters),
            AccountName = accountName,
            LogoImage = logoImage
        };
    }

    // Echoes each provided filter as ("Filter" + PascalCase(name), value) — e.g.
    // transporterId → FilterTransporterId — so PdfReportBuilder resolves the key as a resx
    // label. Date-parseable values are normalized to "yyyy-MM-dd HH:mm"; everything else is
    // echoed raw.
    private static IReadOnlyList<KeyValuePair<string, string>> BuildAppliedFilters(FilterDto filters)
    {
        var applied = new List<KeyValuePair<string, string>>();
        foreach (var (name, _) in filters.Values)
        {
            var text = filters.GetText(name);
            if (text is null)
            {
                continue;
            }

            var value = filters.GetDate(name) is { } date
                ? date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                : text;
            applied.Add(new KeyValuePair<string, string>(
                $"Filter{char.ToUpperInvariant(name[0])}{name[1..]}", value));
        }

        return applied;
    }
}
