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
using System.Resources;
using TrackHub.Reporting.Domain.Models;

namespace TrackHub.Reporting.Domain.Helpers;

// Upstream services send the same value as "InProgress" or "IN_PROGRESS", so lookups ignore case
// and underscores.
public static class ReportValueResolver
{
    private static readonly ResourceManager ResourceManager = new(typeof(Resources.Resources)) { IgnoreCase = true };

    public static object? Localize(object? value, ReportColumn column, CultureInfo culture)
        => value is string { Length: > 0 } text && column.Vocabulary is { } vocabulary ? Localize(vocabulary, text, culture) : value;

    public static string Localize(string vocabulary, string value, CultureInfo culture)
        => ResourceManager.GetString($"{vocabulary}_{value.Replace("_", string.Empty)}", culture) ?? value;
}
