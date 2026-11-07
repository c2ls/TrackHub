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

using Common.Pdf.Rendering;
using SkiaSharp;

namespace Common.Pdf;

/// <summary>Flow layout over SkiaSharp's PDF backend; reads neither the clock nor host fonts, so equal input gives equal bytes.</summary>
public sealed class PdfDocumentBuilder(PdfPageSetup? page = null)
{
    private readonly PdfPageSetup _page = page ?? PdfPageSetup.A4Portrait;
    private readonly List<IBlock> _blocks = [];
    private Func<int, int, string>? _footer;

    public PdfDocumentBuilder Paragraph(string? text, PdfTextStyle? style = null)
    {
        _blocks.Add(new ParagraphBlock(text ?? string.Empty, style ?? PdfTextStyle.Body));
        return this;
    }

    public PdfDocumentBuilder ParagraphIf(string? text, PdfTextStyle? style = null)
        => string.IsNullOrWhiteSpace(text) ? this : Paragraph(text, style);

    public PdfDocumentBuilder Image(byte[]? encoded, float height)
    {
        if (encoded is { Length: > 0 })
        {
            _blocks.Add(new ImageBlock(encoded, height));
        }

        return this;
    }

    public PdfDocumentBuilder Space(float points = 6)
    {
        _blocks.Add(new SpaceBlock(points));
        return this;
    }

    public PdfDocumentBuilder Table(PdfTable table)
    {
        _blocks.Add(new TableBlock(table));
        return this;
    }

    /// <summary>Centered footer text from the 1-based page number and the page count.</summary>
    public PdfDocumentBuilder Footer(Func<int, int, string> pageText)
    {
        _footer = pageText;
        return this;
    }

    public byte[] Build(PdfMetadata? metadata = null)
    {
        using var fonts = new PdfFonts();
        using var layout = new LayoutContext(_page, fonts);
        foreach (var block in _blocks)
        {
            block.Layout(layout);
        }

        using var stream = new MemoryStream();
        using (var document = SKDocument.CreatePdf(stream, ToSkia(metadata)))
        {
            var footerFont = fonts.Get(bold: false, size: 8);
            using var footerPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };

            for (var index = 0; index < layout.Pages.Count; index++)
            {
                var canvas = document.BeginPage(_page.Width, _page.Height);
                foreach (var (fragment, y) in layout.Pages[index])
                {
                    fragment.Draw(canvas, _page.Margin, y);
                }

                if (_footer is not null)
                {
                    canvas.DrawText(_footer(index + 1, layout.Pages.Count), _page.Width / 2, _page.Height - _page.Margin / 2, SKTextAlign.Center, footerFont, footerPaint);
                }

                document.EndPage();
            }

            document.Close();
        }

        return stream.ToArray();
    }

    private static SKDocumentPdfMetadata ToSkia(PdfMetadata? metadata)
    {
        var skia = SKDocumentPdfMetadata.Default;
        if (metadata is null)
        {
            return skia;
        }

        skia.Title = metadata.Title;
        skia.Subject = metadata.Subject;
        skia.Author = metadata.Author;
        skia.Creation = metadata.Created?.ToLocalTime();
        skia.Modified = metadata.Modified?.ToLocalTime();
        return skia;
    }
}
