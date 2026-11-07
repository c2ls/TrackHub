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

public enum PdfAlign
{
    Left,
    Center,
    Right
}

/// <summary>Paragraph formatting; sizes and spacing in points.</summary>
public sealed record PdfTextStyle
{
    public float Size { get; init; } = 9;
    public bool Bold { get; init; }
    public PdfColor Color { get; init; } = PdfColor.Black;
    public PdfColor? Background { get; init; }
    public PdfAlign Align { get; init; }
    public float SpaceBefore { get; init; }
    public float SpaceAfter { get; init; } = 2;

    public static readonly PdfTextStyle Body = new();
    public static readonly PdfTextStyle Small = new() { Size = 8 };
    public static readonly PdfTextStyle Strong = new() { Bold = true };
    public static readonly PdfTextStyle Title = new() { Size = 16, Bold = true, SpaceAfter = 4 };
    public static readonly PdfTextStyle Subtitle = new() { Size = 11, Bold = true };
    public static readonly PdfTextStyle Heading = new() { Size = 12, Bold = true, SpaceBefore = 6, SpaceAfter = 3 };
}
