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

/// <summary>Text wrapped to a width: measured once, drawn line by line.</summary>
internal sealed class TextBlock
{
    private static readonly char[] LineBreaks = ['\r', '\n'];

    private readonly SKFont _font;
    private readonly SKColor _color;
    private readonly PdfAlign _align;
    private readonly string[] _lines;

    public TextBlock(string text, SKFont font, PdfColor color, PdfAlign align, float width)
    {
        _font = font;
        _color = color.ToSkia();
        _align = align;
        _lines = Wrap(text, font, width);
        Width = _lines.Length == 0 ? 0 : _lines.Max(line => font.MeasureText(line));
    }

    public int LineCount => _lines.Length;
    public float LineHeight => _font.Spacing;
    public float Width { get; }
    public float Height => _lines.Length * LineHeight;

    public void DrawLine(SKCanvas canvas, int index, float x, float y, float width)
    {
        using var paint = new SKPaint { Color = _color, IsAntialias = true };
        var (anchor, align) = _align switch
        {
            PdfAlign.Center => (x + width / 2, SKTextAlign.Center),
            PdfAlign.Right => (x + width, SKTextAlign.Right),
            _ => (x, SKTextAlign.Left)
        };

        canvas.DrawText(_lines[index], anchor, y - _font.Metrics.Ascent, align, _font, paint);
    }

    public void Draw(SKCanvas canvas, float x, float y, float width)
    {
        for (var i = 0; i < _lines.Length; i++)
        {
            DrawLine(canvas, i, x, y + i * LineHeight, width);
        }
    }

    private static string[] Wrap(string text, SKFont font, float width)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var lines = new List<string>();
        foreach (var paragraph in text.Split(LineBreaks, StringSplitOptions.RemoveEmptyEntries))
        {
            var current = string.Empty;
            foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = current.Length == 0 ? word : current + " " + word;
                if (font.MeasureText(candidate) <= width)
                {
                    current = candidate;
                    continue;
                }

                if (current.Length > 0)
                {
                    lines.Add(current);
                }

                current = word;
                while (font.MeasureText(current) > width)
                {
                    var fit = Math.Max(1, (int)font.BreakText(current, width));
                    lines.Add(current[..fit]);
                    current = current[fit..];
                }
            }

            if (current.Length > 0)
            {
                lines.Add(current);
            }
        }

        return [.. lines];
    }
}
