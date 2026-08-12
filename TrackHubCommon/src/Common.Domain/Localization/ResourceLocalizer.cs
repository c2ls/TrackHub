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
using System.Reflection;
using System.Resources;

namespace Common.Domain.Localization;

/// <summary>
/// Shared server-side localization primitive over .resx resources (localized strings never live
/// in code or the database — they belong in resources).
///
/// Two lookup modes:
/// <list type="bullet">
/// <item><see cref="GetString(string)"/> — ambient <see cref="CultureInfo.CurrentCulture"/>, for
/// request-scoped surfaces where <c>UseRequestLocalization</c> has set the culture (e.g. the
/// AuthorityServer login/validation messages).</item>
/// <item><see cref="GetString(string, string?)"/> — explicit locale per call, for per-recipient
/// rendering and background jobs that have no ambient request culture (e.g. notification
/// dispatch).</item>
/// </list>
/// Both fall back to the neutral resource and finally to an empty string, never throwing for a
/// missing translation.
/// </summary>
public sealed class ResourceLocalizer(string baseName, Assembly assembly)
{
    private readonly ResourceManager _resources = new(baseName, assembly);

    /// <summary>Resolves <paramref name="key"/> for the ambient request culture.</summary>
    public string GetString(string key)
        => _resources.GetString(key, CultureInfo.CurrentCulture)
            ?? _resources.GetString(key, CultureInfo.InvariantCulture)
            ?? string.Empty;

    /// <summary>Resolves <paramref name="key"/> for an explicit locale (e.g. the recipient's).</summary>
    public string GetString(string key, string? locale)
        => _resources.GetString(key, ResolveCulture(locale))
            ?? _resources.GetString(key, CultureInfo.InvariantCulture)
            ?? string.Empty;

    /// <summary>
    /// Normalizes a stored language preference ("es", "es-CO", "EN_us", …) to its two-letter
    /// lowercase code; null when empty or unusable. Callers validate the result against their own
    /// supported set.
    /// </summary>
    public static string? NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        var code = language.Trim().ToLowerInvariant();
        if (code.Length > 2)
        {
            code = code[..2];
        }

        return code.Length == 2 && code.All(char.IsAsciiLetterLower) ? code : null;
    }

    private static CultureInfo ResolveCulture(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return CultureInfo.InvariantCulture;
        }

        try
        {
            return CultureInfo.GetCultureInfo(locale);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.InvariantCulture;
        }
    }
}
