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

namespace TrackHub.Geofencing.Infrastructure.Configurations;

// Enum-backed numeric columns are stored as plain short/int, so nothing stops a caller from
// writing a value the application has no meaning for. These helpers render the CHECK predicate
// and the column comment straight from the authoritative enum, so widening the enum and
// forgetting the database is caught by the migration diff instead of by production data.
internal static class EnumColumn
{
    public static string Check<TEnum>(string column)
        where TEnum : struct, Enum
        => $"{column} in ({string.Join(", ", Numbers<TEnum>())})";

    public static string Comment<TEnum>(string description)
        where TEnum : struct, Enum
        => $"{description} Values: {string.Join(", ", Enum.GetNames<TEnum>().Select(n => $"{Number(Enum.Parse<TEnum>(n))}={n}"))}.";

    private static IEnumerable<string> Numbers<TEnum>()
        where TEnum : struct, Enum
        => Enum.GetValues<TEnum>().Select(Number);

    private static string Number<TEnum>(TEnum value)
        where TEnum : struct, Enum
        => Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
}
