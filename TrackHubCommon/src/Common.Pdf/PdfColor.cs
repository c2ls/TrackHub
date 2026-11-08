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
using SkiaSharp;

namespace Common.Pdf;

public readonly record struct PdfColor(byte R, byte G, byte B)
{
    public static readonly PdfColor Black = new(0, 0, 0);
    public static readonly PdfColor White = new(255, 255, 255);
    public static readonly PdfColor Gray = new(0x60, 0x60, 0x60);
    public static readonly PdfColor LightGray = new(0xD9, 0xD9, 0xD9);
    public static readonly PdfColor LightYellow = new(0xFF, 0xF4, 0xC2);

    /// <summary>Parses <c>#RRGGBB</c>; anything else yields <paramref name="fallback"/>.</summary>
    public static PdfColor Parse(string? hex, PdfColor fallback)
    {
        var digits = hex?.Trim().TrimStart('#');
        return digits is { Length: 6 }
            && uint.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb)
            ? new PdfColor((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb)
            : fallback;
    }

    internal SKColor ToSkia() => new(R, G, B);
}
