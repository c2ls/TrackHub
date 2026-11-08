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

namespace Common.Pdf;

public readonly record struct PdfCell(string Text, bool Bold = false, PdfAlign Align = PdfAlign.Left, byte[]? Image = null, float ImageHeight = 0)
{
    public static implicit operator PdfCell(string? text) => new(text ?? string.Empty);

    public static PdfCell Picture(byte[] encoded, float height) => new(string.Empty, Image: encoded, ImageHeight: height);
}

/// <summary>Bordered table flowing across pages with repeated header rows; columns are proportional or sized from content, and fill the content width.</summary>
public sealed class PdfTable
{
    internal readonly List<PdfTableRow> HeaderRows = [];
    internal readonly List<PdfTableRow> BodyRows = [];
    internal readonly double[]? RelativeWidths;

    public int ColumnCount { get; }
    public float FontSize { get; init; } = 8;

    public PdfTable(int columns)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(columns, 1);
        ColumnCount = columns;
    }

    public PdfTable(params double[] relativeWidths)
        : this(relativeWidths.Length)
        => RelativeWidths = relativeWidths;

    public PdfTable HeaderRow(params PdfCell[] cells)
    {
        HeaderRows.Add(new PdfTableRow(Pad(cells, bold: true), PdfColor.LightGray, Spans: false));
        return this;
    }

    public PdfTable Row(params PdfCell[] cells)
    {
        BodyRows.Add(new PdfTableRow(Pad(cells, bold: false), null, Spans: false));
        return this;
    }

    public PdfTable SpanRow(string text, PdfColor? background = null)
    {
        BodyRows.Add(new PdfTableRow([new PdfCell(text, Bold: true)], background ?? PdfColor.LightGray, Spans: true));
        return this;
    }

    private PdfCell[] Pad(PdfCell[] cells, bool bold)
    {
        if (cells.Length > ColumnCount)
        {
            throw new ArgumentException($"Row has {cells.Length} cells but the table has {ColumnCount} columns.", nameof(cells));
        }

        var row = new PdfCell[ColumnCount];
        for (var i = 0; i < ColumnCount; i++)
        {
            var cell = i < cells.Length ? cells[i] : new PdfCell(string.Empty);
            row[i] = bold ? cell with { Bold = true } : cell;
        }

        return row;
    }
}

internal sealed record PdfTableRow(PdfCell[] Cells, PdfColor? Background, bool Spans);
