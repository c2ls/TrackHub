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

using Common.Domain.Helpers;
using FluentAssertions;

namespace Common.Domain.Tests.Helpers;

public class UtcTimestampsTests
{
    private static readonly DateTimeOffset Instant = new(2026, 3, 1, 10, 15, 30, 250, TimeSpan.Zero);

    [Theory]
    [InlineData(UtcTimestamps.Sortable, "2026-03-01 10:15:30")]
    [InlineData(UtcTimestamps.Iso8601, "2026-03-01T10:15:30Z")]
    [InlineData(UtcTimestamps.Iso8601Milliseconds, "2026-03-01T10:15:30.250Z")]
    [InlineData(UtcTimestamps.Date, "2026-03-01")]
    [InlineData(UtcTimestamps.TimeOfDay, "10:15:30")]
    public void Format_WritesTheUtcInstant_WhateverTheOffset(string format, string expected)
        => UtcTimestamps.Format(Instant.ToOffset(TimeSpan.FromHours(-5)), format).Should().Be(expected);

    [Fact]
    public void Parse_ReadsAnUnzonedValueAsUtc()
        => UtcTimestamps.Parse("2026-03-01 10:15:30").Should().Be(new DateTimeOffset(2026, 3, 1, 10, 15, 30, TimeSpan.Zero));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("01/03/2026 10:15")]
    public void Parse_UnparsableValue_IsNull(string? value)
        => UtcTimestamps.Parse(value).Should().BeNull();

    [Theory]
    [InlineData(1772360130d)]
    [InlineData(1772360130000d)]
    public void FromUnixEpoch_AcceptsSecondsOrMilliseconds(double epoch)
        => UtcTimestamps.FromUnixEpoch(epoch).Should().Be(DateTimeOffset.FromUnixTimeSeconds(1772360130));

    [Theory]
    [InlineData(null)]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(1e18)]
    public void FromUnixEpoch_MissingOrOutOfRange_IsNull(double? epoch)
        => UtcTimestamps.FromUnixEpoch(epoch).Should().BeNull();
}
