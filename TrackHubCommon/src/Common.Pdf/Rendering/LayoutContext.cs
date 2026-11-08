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

internal interface IFragment
{
    float Height { get; }
    void Draw(SKCanvas canvas, float x, float y);
}

internal interface IBlock
{
    void Layout(LayoutContext context);
}

internal sealed class LayoutContext(PdfPageSetup page, PdfFonts fonts) : IDisposable
{
    private readonly List<IDisposable> _resources = [];
    private float _y = page.Margin;

    public PdfFonts Fonts { get; } = fonts;
    public float Width { get; } = page.ContentWidth;
    public List<List<(IFragment Fragment, float Y)>> Pages { get; } = [[]];

    public bool AtPageTop => _y <= page.Margin;

    public bool Fits(float height) => _y + height <= page.ContentBottom;

    public void Place(IFragment fragment)
    {
        if (!Fits(fragment.Height) && !AtPageTop)
        {
            NewPage();
        }

        Pages[^1].Add((fragment, _y));
        _y += fragment.Height;
    }

    /// <summary>Vertical spacing; collapses at the top of a page and never spills onto the next.</summary>
    public void Advance(float height)
    {
        if (!AtPageTop)
        {
            _y = Math.Min(_y + height, page.ContentBottom);
        }
    }

    public void NewPage()
    {
        Pages.Add([]);
        _y = page.Margin;
    }

    public T Track<T>(T resource) where T : IDisposable
    {
        _resources.Add(resource);
        return resource;
    }

    public void Dispose()
    {
        foreach (var resource in _resources)
        {
            resource.Dispose();
        }
    }
}
