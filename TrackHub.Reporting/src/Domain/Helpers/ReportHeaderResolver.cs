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

namespace TrackHub.Reporting.Domain.Helpers;

// Resolves a report column/label key to its localized text through the Reporting ResourceManager,
// falling back to the key itself when unmapped — the same mechanism the pre-refactor ExcelHelper used.
// Exposed publicly so the Excel/PDF formatters and the Application preview handler share one resolver.
public static class ReportHeaderResolver
{
    private static readonly ResourceManager ResourceManager = new(typeof(Resources.Resources));

    public static string Resolve(string key, CultureInfo culture)
        => ResourceManager.GetString(key, culture) ?? key;
}
