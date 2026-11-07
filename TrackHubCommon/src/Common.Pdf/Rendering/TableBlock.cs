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

using SkiaSharp;

namespace Common.Pdf.Rendering;

internal sealed class TableBlock(PdfTable table) : IBlock
{
    private const float CellPadding = 3;
    private const float MinColumnWidth = 36;
    private const float BorderWidth = 0.5f;

    public void Layout(LayoutContext context)
    {
        var widths = ColumnWidths(context);
        var headers = table.HeaderRows.Select(row => new RowFragment(row, widths, context, table.FontSize)).ToList();
        var headerHeight = headers.Sum(header => header.Height);

        var first = true;
        foreach (var row in table.BodyRows)
        {
            var fragment = new RowFragment(row, widths, context, table.FontSize);
            if (first)
            {
                if (!context.Fits(headerHeight + fragment.Height) && !context.AtPageTop)
                {
                    context.NewPage();
                }

                headers.ForEach(context.Place);
                first = false;
            }
            else if (!context.Fits(fragment.Height))
            {
                context.NewPage();
                headers.ForEach(context.Place);
            }

            context.Place(fragment);
        }

        if (first)
        {
            headers.ForEach(context.Place);
        }

        context.Advance(6);
    }

    private float[] ColumnWidths(LayoutContext context)
    {
        var weights = table.RelativeWidths ?? NaturalWidths(context);
        var total = weights.Sum();
        return [.. weights.Select(weight => (float)(weight / total * context.Width))];
    }

    /// <summary>Widest content per column, clamped so one long column cannot starve the others.</summary>
    private double[] NaturalWidths(LayoutContext context)
    {
        var cap = context.Width * 0.4;
        var widths = new double[table.ColumnCount];
        foreach (var row in table.HeaderRows.Concat(table.BodyRows).Where(row => !row.Spans))
        {
            for (var column = 0; column < row.Cells.Length; column++)
            {
                var cell = row.Cells[column];
                var font = context.Fonts.Get(cell.Bold, table.FontSize);
                var natural = cell.Image is null ? font.MeasureText(cell.Text) : cell.ImageHeight;
                widths[column] = Math.Max(widths[column], Math.Clamp(natural + 2 * CellPadding, MinColumnWidth, cap));
            }
        }

        return [.. widths.Select(width => Math.Max(width, MinColumnWidth))];
    }

    private sealed class RowFragment : IFragment
    {
        private readonly PdfTableRow _row;
        private readonly float[] _widths;
        private readonly object?[] _contents;

        public RowFragment(PdfTableRow row, float[] widths, LayoutContext context, float fontSize)
        {
            _row = row;
            _widths = row.Spans ? [widths.Sum()] : widths;
            _contents = new object?[_row.Cells.Length];

            var minimum = context.Fonts.Get(false, fontSize).Spacing;
            var tallest = minimum;
            for (var i = 0; i < _row.Cells.Length; i++)
            {
                var cell = _row.Cells[i];
                var inner = _widths[i] - 2 * CellPadding;
                if (cell.Image is not null)
                {
                    var image = ImageFragment.Create(cell.Image, cell.ImageHeight, inner, context);
                    _contents[i] = image;
                    tallest = Math.Max(tallest, image?.Height ?? 0);
                }
                else
                {
                    var text = new TextBlock(cell.Text, context.Fonts.Get(cell.Bold, fontSize), PdfColor.Black, cell.Align, inner);
                    _contents[i] = text;
                    tallest = Math.Max(tallest, text.Height);
                }
            }

            Height = tallest + 2 * CellPadding;
        }

        public float Height { get; }

        public void Draw(SKCanvas canvas, float x, float y)
        {
            if (_row.Background is { } background)
            {
                using var fill = new SKPaint { Color = background.ToSkia() };
                canvas.DrawRect(x, y, _widths.Sum(), Height, fill);
            }

            using var border = new SKPaint { Color = PdfColor.Gray.ToSkia(), Style = SKPaintStyle.Stroke, StrokeWidth = BorderWidth };
            var left = x;
            for (var i = 0; i < _widths.Length; i++)
            {
                canvas.DrawRect(left, y, _widths[i], Height, border);
                switch (_contents[i])
                {
                    case TextBlock text:
                        text.Draw(canvas, left + CellPadding, y + CellPadding, _widths[i] - 2 * CellPadding);
                        break;
                    case ImageFragment image:
                        image.Draw(canvas, left + CellPadding, y + CellPadding);
                        break;
                }

                left += _widths[i];
            }
        }
    }
}
