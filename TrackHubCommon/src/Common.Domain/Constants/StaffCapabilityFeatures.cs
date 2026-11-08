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

namespace Common.Domain.Constants;

public static partial class StaffCapabilityFeatures
{
    private static readonly IReadOnlyDictionary<string, string> Map = Build();

    public static string? FeatureKeyFor(string capability) => Map.GetValueOrDefault(capability);

    public static IReadOnlySet<string> Available(IEnumerable<string> capabilities, IEnumerable<string> enabledFeatureKeys)
    {
        var enabled = new HashSet<string>(enabledFeatureKeys, StringComparer.Ordinal);
        return capabilities.Where(c => Map.TryGetValue(c, out var key) && enabled.Contains(key)).ToHashSet(StringComparer.Ordinal);
    }

    static partial void AddEditionFeatures(IDictionary<string, string> map);

    private static IReadOnlyDictionary<string, string> Build()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [StaffCapabilities.Vehicle] = FeatureKeys.StaffMobile,
            [StaffCapabilities.Capture] = FeatureKeys.Documents,
            [StaffCapabilities.Trips] = FeatureKeys.TripManagement,
        };
        AddEditionFeatures(map);
        return map;
    }
}
