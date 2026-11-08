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

internal sealed class ParagraphBlock(string text, PdfTextStyle style) : IBlock
{
    public void Layout(LayoutContext context)
    {
        var block = new TextBlock(text, context.Fonts.Get(style.Bold, style.Size), style.Color, style.Align, context.Width);

        context.Advance(style.SpaceBefore);
        for (var line = 0; line < block.LineCount; line++)
        {
            context.Place(new LineFragment(block, line, context.Width, style.Background));
        }

        context.Advance(style.SpaceAfter);
    }

    private sealed class LineFragment(TextBlock block, int line, float width, PdfColor? background) : IFragment
    {
        public float Height => block.LineHeight;

        public void Draw(SKCanvas canvas, float x, float y)
        {
            if (background is { } color)
            {
                using var fill = new SKPaint { Color = color.ToSkia() };
                canvas.DrawRect(x, y, width, Height, fill);
            }

            block.DrawLine(canvas, line, x, y, width);
        }
    }
}

internal sealed class SpaceBlock(float points) : IBlock
{
    public void Layout(LayoutContext context) => context.Advance(points);
}

internal sealed class ImageBlock(byte[] encoded, float height) : IBlock
{
    public void Layout(LayoutContext context)
    {
        if (ImageFragment.Create(encoded, height, context.Width, context) is { } image)
        {
            context.Place(image);
            context.Advance(4);
        }
    }
}

internal sealed class ImageFragment(SKImage image, float width, float height) : IFragment
{
    public float Width => width;
    public float Height => height;

    /// <summary>Decodes and scales to the height, shrinking further if the width would overflow; null when undecodable.</summary>
    public static ImageFragment? Create(byte[]? encoded, float height, float maxWidth, LayoutContext context)
    {
        if (encoded is not { Length: > 0 } || height <= 0)
        {
            return null;
        }

        var image = SKImage.FromEncodedData(encoded);
        if (image is null)
        {
            return null;
        }

        context.Track(image);
        var width = height * image.Width / image.Height;
        if (width > maxWidth)
        {
            height *= maxWidth / width;
            width = maxWidth;
        }

        return new ImageFragment(image, width, height);
    }

    public void Draw(SKCanvas canvas, float x, float y)
        => canvas.DrawImage(image, SKRect.Create(x, y, width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
}
