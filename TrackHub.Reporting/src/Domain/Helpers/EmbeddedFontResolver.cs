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

using System.Collections.Concurrent;
using System.Reflection;
using PdfSharp.Fonts;

namespace TrackHub.Reporting.Domain.Helpers;

/// <summary>
/// Serves the report font from assembly resources instead of the host's font collection.
/// PDFsharp ships resolvers for Windows and WSL2 only: on Linux — every container and build
/// agent this service runs on — an unresolved face throws, including MigraDoc's own
/// <c>Courier New</c> error font, so the failure surfaces as a font-resolution exception that
/// hides whatever provoked it. Embedding the faces makes rendering produce byte-identical
/// output everywhere and removes the dependency on an image installing fonts.
/// </summary>
/// <remarks>
/// Every requested family maps to Liberation Sans (metric-compatible with Arial, SIL Open Font
/// License 1.1 — see <c>Fonts/LICENSE.txt</c>). The reports use one family by design, and
/// mapping the rest means no font request can fail to resolve.
/// </remarks>
internal sealed class EmbeddedFontResolver : IFontResolver
{
    public const string FamilyName = "Liberation Sans";

    private const string RegularFaceName = "LiberationSans-Regular";
    private const string BoldFaceName = "LiberationSans-Bold";
    private const string ResourceNamespace = "TrackHub.Reporting.Domain.Fonts.";

    private static readonly ConcurrentDictionary<string, byte[]> Faces = new();

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        // No italic face is embedded: the reports never ask for one, and letting PDFsharp
        // slant the regular face is better than failing if a future style does.
        => new(isBold ? BoldFaceName : RegularFaceName, mustSimulateBold: false, mustSimulateItalic: isItalic);

    public byte[]? GetFont(string faceName)
        => Faces.GetOrAdd(faceName, static name =>
        {
            var assembly = typeof(EmbeddedFontResolver).Assembly;
            using var stream = assembly.GetManifestResourceStream(ResourceNamespace + name + ".ttf")
                ?? throw new InvalidOperationException(
                    $"Embedded font face '{name}' is missing from {assembly.GetName().Name}.");

            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            return buffer.ToArray();
        });
}
