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

/// <summary>TrackHub Sans from assembly resources; typefaces are shared, sized fonts are per render.</summary>
internal sealed class PdfFonts : IDisposable
{
    private static readonly Lazy<SKTypeface> Regular = new(() => Load("TrackHubSans-Regular"));
    private static readonly Lazy<SKTypeface> Bold = new(() => Load("TrackHubSans-Bold"));

    private readonly Dictionary<(bool Bold, float Size), SKFont> _fonts = [];

    public SKFont Get(bool bold, float size)
    {
        if (!_fonts.TryGetValue((bold, size), out var font))
        {
            font = new SKFont(bold ? Bold.Value : Regular.Value, size) { Subpixel = true, Edging = SKFontEdging.Antialias };
            _fonts[(bold, size)] = font;
        }

        return font;
    }

    public void Dispose()
    {
        foreach (var font in _fonts.Values)
        {
            font.Dispose();
        }
    }

    private static SKTypeface Load(string face)
    {
        var assembly = typeof(PdfFonts).Assembly;
        using var stream = assembly.GetManifestResourceStream($"Common.Pdf.Fonts.{face}.ttf")
            ?? throw new InvalidOperationException($"Embedded font face '{face}' is missing from {assembly.GetName().Name}.");

        return SKTypeface.FromStream(stream)
            ?? throw new InvalidOperationException($"Embedded font face '{face}' could not be parsed.");
    }
}
