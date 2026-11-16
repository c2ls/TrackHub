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

namespace Common.Domain.Helpers;

public static class UtcTimestamps
{
    public const string Sortable = "yyyy-MM-dd HH:mm:ss";
    public const string Date = "yyyy-MM-dd";
    public const string TimeOfDay = "HH:mm:ss";
    public const string Iso8601 = "yyyy-MM-dd'T'HH:mm:ss'Z'";
    public const string Iso8601Milliseconds = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

    private const double SmallestMillisecondEpoch = 1e11;
    private const double LargestMillisecondEpoch = 253402300799999;

    public static string Format(DateTimeOffset instant, string format = Sortable)
        => instant.UtcDateTime.ToString(format, CultureInfo.InvariantCulture);

    public static DateTimeOffset? Parse(string? value, string format = Sortable)
        => DateTimeOffset.TryParseExact(value, format, CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed
            : null;

    public static DateTimeOffset? FromUnixEpoch(double? secondsOrMilliseconds)
        => secondsOrMilliseconds switch
        {
            null or <= 0 or > LargestMillisecondEpoch => null,
            >= SmallestMillisecondEpoch => DateTimeOffset.FromUnixTimeMilliseconds((long)secondsOrMilliseconds.Value),
            _ => DateTimeOffset.FromUnixTimeSeconds((long)secondsOrMilliseconds.Value)
        };
}
