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

using System.Globalization;
using TrackHub.Reporting.Domain.Helpers;
using TrackHub.Reporting.Domain.Interfaces;
using TrackHub.Reporting.Domain.Interfaces.Manager;

namespace TrackHub.Reporting.Application.Report.Filters;

// The PDF header echoes the applied filters; ids and stored values are replaced by what the
// person picked on screen. Only the account-status picker feeds the status filter.
public sealed class ReportFilterLabeler(IReportNameReader names) : IReportFilterLabeler
{
    public async Task<IReadOnlyList<KeyValuePair<string, object>>> LabelAsync(
        IReadOnlyList<KeyValuePair<string, object>> appliedFilters, CultureInfo culture, CancellationToken cancellationToken)
    {
        var labeled = new List<KeyValuePair<string, object>>(appliedFilters.Count);
        foreach (var (key, value) in appliedFilters)
        {
            labeled.Add(new(key, await LabelAsync(key, value, culture, cancellationToken)));
        }

        return labeled;
    }

    private async Task<object> LabelAsync(string key, object value, CultureInfo culture, CancellationToken cancellationToken)
    {
        if (value is not string text)
        {
            return value;
        }

        return key switch
        {
            "FilterStatus" => ReportValueResolver.Localize("AccountStatus", text, culture),
            "FilterTransporterId" when Guid.TryParse(text, out var id)
                => await names.GetTransporterNameAsync(id, cancellationToken) ?? text,
            "FilterOperatorId" when Guid.TryParse(text, out var id)
                => (await names.GetOperatorNamesAsync(cancellationToken)).GetValueOrDefault(id) ?? text,
            _ => text
        };
    }
}
