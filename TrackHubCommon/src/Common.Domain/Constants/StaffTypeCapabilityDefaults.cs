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

public static partial class StaffTypeCapabilityDefaults
{
    public static IReadOnlySet<string> For(string staffType)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        switch (staffType)
        {
            case StaffTypes.Driver:
                set.UnionWith([StaffCapabilities.Vehicle, StaffCapabilities.Status, StaffCapabilities.Capture, StaffCapabilities.Trips]);
                break;
            case StaffTypes.Operator:
                set.UnionWith([StaffCapabilities.Vehicle, StaffCapabilities.Status, StaffCapabilities.Capture]);
                break;
            case StaffTypes.DriverAssistant:
            case StaffTypes.Mechanic:
            case StaffTypes.Supervisor:
                set.UnionWith([StaffCapabilities.Status, StaffCapabilities.Capture]);
                break;
            case StaffTypes.Administrative:
            case StaffTypes.Other:
                set.Add(StaffCapabilities.Status);
                break;
        }

        AddEditionDefaults(staffType, set);
        return set;
    }

    static partial void AddEditionDefaults(string staffType, ISet<string> capabilities);
}
