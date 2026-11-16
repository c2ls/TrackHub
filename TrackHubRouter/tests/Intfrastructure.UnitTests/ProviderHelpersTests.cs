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

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrackHub.Router.Domain.Helpers;

namespace TrackHub.Router.Infrastructure.Tests;

[TestFixture]
public class ProviderHelpersTests
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan TimerSlack = TimeSpan.FromMilliseconds(20);

    private sealed record Flag([property: JsonConverter(typeof(LenientBoolConverter))] bool? Value);

    [TestCase("\"on\"", true)]
    [TestCase("\"OFF\"", false)]
    [TestCase("1", true)]
    [TestCase("0", false)]
    [TestCase("\"unknown\"", null)]
    public void LenientBoolConverter_ReadsProviderOnOffFlags(string json, bool? expected)
        => Assert.That(JsonSerializer.Deserialize<Flag>($"{{\"Value\":{json}}}")!.Value, Is.EqualTo(expected));

    [Test]
    public async Task RequestPacing_SpacesRequestsOfOneCredential_ButNotOfAnother()
    {
        var credential = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        await RequestPacing.RunAsync("test", credential, Interval, () => Task.FromResult(0), CancellationToken.None);
        await RequestPacing.RunAsync("test", Guid.NewGuid(), Interval, () => Task.FromResult(0), CancellationToken.None);
        var unpacedElapsed = stopwatch.Elapsed;
        await RequestPacing.RunAsync("test", credential, Interval, () => Task.FromResult(0), CancellationToken.None);

        Assert.That(unpacedElapsed, Is.LessThan(Interval));
        Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(Interval - TimerSlack));
    }

    [Test]
    public async Task RequestPacing_FailedRequest_StillCountsTowardsTheSpacing()
    {
        var credential = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            RequestPacing.RunAsync<int>("test", credential, Interval, () => throw new InvalidOperationException(), CancellationToken.None));
        await RequestPacing.RunAsync("test", credential, Interval, () => Task.FromResult(0), CancellationToken.None);

        Assert.That(stopwatch.Elapsed, Is.GreaterThanOrEqualTo(Interval - TimerSlack));
    }
}
