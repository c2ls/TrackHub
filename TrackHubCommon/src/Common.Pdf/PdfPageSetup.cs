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

/// <summary>Page size and margin in points (1/72 in).</summary>
public sealed record PdfPageSetup(float Width, float Height, float Margin)
{
    private const float A4Short = 595.28f;
    private const float A4Long = 841.89f;

    public static readonly PdfPageSetup A4Portrait = new(A4Short, A4Long, Cm(2));
    public static readonly PdfPageSetup A4Landscape = new(A4Long, A4Short, Cm(2));

    public static float Cm(double centimetres) => (float)(centimetres * 72 / 2.54);

    internal float ContentWidth => Width - 2 * Margin;
    internal float ContentBottom => Height - Margin;
}
