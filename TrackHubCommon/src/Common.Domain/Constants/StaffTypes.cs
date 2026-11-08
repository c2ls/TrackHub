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

public static partial class StaffTypes
{
    public const string Driver = nameof(Driver);
    public const string Operator = nameof(Operator);
    public const string DriverAssistant = nameof(DriverAssistant);
    public const string Mechanic = nameof(Mechanic);
    public const string Supervisor = nameof(Supervisor);
    public const string Administrative = nameof(Administrative);
    public const string Other = nameof(Other);

    public const string Default = Driver;

    public static IReadOnlyCollection<string> All { get; } = Build();

    public static bool IsValid(string? value) => value is not null && All.Contains(value);

    static partial void AddEditionTypes(ICollection<string> types);

    private static IReadOnlyCollection<string> Build()
    {
        var types = new List<string> { Driver, Operator, DriverAssistant, Mechanic, Supervisor, Administrative, Other };
        AddEditionTypes(types);
        return types.AsReadOnly();
    }
}
