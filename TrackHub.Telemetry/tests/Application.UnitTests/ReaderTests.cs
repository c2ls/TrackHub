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

using Common.Domain.Constants;
using Common.Domain.Helpers;
using TrackHub.Telemetry.Domain.Enums;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;

namespace TrackHub.Telemetry.Application.UnitTests;

[TestFixture]
public class ReaderTests
{
    private static Filters NoFilters() => new(new Dictionary<string, object>());

    private static OperatorSyncRun Run(Guid accountId, Guid operatorId, DateTimeOffset startedAt, OperatorSyncResult result = OperatorSyncResult.Succeeded)
        => new(accountId, operatorId, (int)SyncTriggerType.Automatic, (int)result, startedAt) { CompletedAt = startedAt.AddSeconds(1) };

    private static OperatorHealthCheck Check(Guid accountId, Guid operatorId, OperatorHealthStatus status, DateTimeOffset startedAt, int? latencyMs = null)
        => new(accountId, operatorId, (int)OperatorHealthCheckType.Ping, (int)status, latencyMs, startedAt, startedAt.AddSeconds(1), null, null, 0, null);

    [Test]
    public async Task SyncRuns_AreAccountScoped_AndOrderedNewestFirst()
    {
        var accountA = Guid.NewGuid();
        var accountB = Guid.NewGuid();
        var op = Guid.NewGuid();
        var context = TestDb.NewContext();
        var now = DateTimeOffset.UtcNow;
        context.OperatorSyncRuns.Add(Run(accountA, op, now.AddMinutes(-5)));
        context.OperatorSyncRuns.Add(Run(accountA, op, now.AddMinutes(-1)));
        context.OperatorSyncRuns.Add(Run(accountB, Guid.NewGuid(), now)); // other account — must not leak
        context.SaveChanges();
        var reader = new OperatorSyncRunReader(context, TestDb.PrincipalFor(accountA));

        var runs = await reader.GetAsync(NoFilters(), take: 50, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(runs, Has.Count.EqualTo(2), "only the caller's account is returned");
            Assert.That(runs.All(r => r.AccountId == accountA), Is.True);
            Assert.That(runs.First().StartedAt, Is.EqualTo(now.AddMinutes(-1)), "newest first");
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task SyncRuns_TakeIsClampedToUpperBound()
    {
        var accountId = Guid.NewGuid();
        var op = Guid.NewGuid();
        var context = TestDb.NewContext();
        var now = DateTimeOffset.UtcNow;
        for (var i = 0; i < 600; i++)
        {
            context.OperatorSyncRuns.Add(Run(accountId, op, now.AddSeconds(-i)));
        }
        context.SaveChanges();
        var reader = new OperatorSyncRunReader(context, TestDb.PrincipalFor(accountId));

        var runs = await reader.GetAsync(NoFilters(), take: 10000, CancellationToken.None);

        Assert.That(runs, Has.Count.EqualTo(500), "take is clamped to the 500 upper bound");
        await context.DisposeAsync();
    }

    [Test]
    public async Task HistoryRange_ReturnsRowsWithinBounds_Ascending_CappedByMaxPoints()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var context = TestDb.NewContext();
        var t0 = DateTimeOffset.UtcNow.AddHours(-1);
        // 5 in-range points + one before the window + one after the window.
        for (var i = 0; i < 5; i++)
        {
            context.TransporterPositionHistory.Add(History(accountId, operatorId, transporterId, t0.AddMinutes(i * 5), $"K{i}"));
        }
        context.TransporterPositionHistory.Add(History(accountId, operatorId, transporterId, t0.AddMinutes(-30), "before"));
        context.TransporterPositionHistory.Add(History(accountId, operatorId, transporterId, t0.AddHours(2), "after"));
        context.SaveChanges();
        var reader = new TransporterPositionHistoryReader(context, TestDb.PrincipalFor(accountId));

        var all = await reader.GetRangeAsync(accountId, transporterId, t0, t0.AddMinutes(30), maxPoints: 10000, CancellationToken.None);
        var capped = await reader.GetRangeAsync(accountId, transporterId, t0, t0.AddMinutes(30), maxPoints: 3, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(all, Has.Count.EqualTo(5), "only rows inside [from, to]");
            Assert.That(all.Select(x => x.SourceTimestamp), Is.Ordered.Ascending);
            Assert.That(capped, Has.Count.EqualTo(3), "maxPoints caps the result");
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task HealthSummary_ComputesUptimeAndFailureCounts()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.Operators.Add(new Operator { OperatorId = operatorId, AccountId = accountId });
        var since = DateTimeOffset.UtcNow.AddHours(-1);
        context.OperatorHealthChecks.Add(Check(accountId, operatorId, OperatorHealthStatus.Healthy, since.AddMinutes(5), latencyMs: 20));
        context.OperatorHealthChecks.Add(Check(accountId, operatorId, OperatorHealthStatus.Healthy, since.AddMinutes(10), latencyMs: 40));
        context.OperatorHealthChecks.Add(Check(accountId, operatorId, OperatorHealthStatus.Degraded, since.AddMinutes(15), latencyMs: 300));
        context.OperatorHealthChecks.Add(Check(accountId, operatorId, OperatorHealthStatus.Offline, since.AddMinutes(20)));
        context.SaveChanges();
        var reader = new OperatorHealthCheckReader(context, TestDb.PrincipalFor(accountId));

        var summary = await reader.GetSummaryAsync(operatorId, since, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(summary.TotalChecks, Is.EqualTo(4));
            Assert.That(summary.HealthyChecks, Is.EqualTo(2));
            Assert.That(summary.DegradedChecks, Is.EqualTo(1));
            Assert.That(summary.OfflineChecks, Is.EqualTo(1));
            Assert.That(summary.FailureCount, Is.EqualTo(2), "degraded + offline");
            Assert.That(summary.UptimePercent, Is.EqualTo(50).Within(0.01), "2 of 4 healthy");
            Assert.That(summary.AverageLatencyMs, Is.EqualTo(120).Within(0.01), "(20+40+300)/3");
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task HealthSummary_NoChecks_ReturnsZeros()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.Operators.Add(new Operator { OperatorId = operatorId, AccountId = accountId });
        context.SaveChanges();
        var reader = new OperatorHealthCheckReader(context, TestDb.PrincipalFor(accountId));

        var summary = await reader.GetSummaryAsync(operatorId, DateTimeOffset.UtcNow.AddHours(-1), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(summary.TotalChecks, Is.EqualTo(0));
            Assert.That(summary.UptimePercent, Is.EqualTo(0));
            Assert.That(summary.AverageLatencyMs, Is.Null);
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task RetentionPolicy_ParsesRetentionDays_DefaultsAndDisabled()
    {
        var enabledCustom = Guid.NewGuid();
        var enabledDefault = Guid.NewGuid();
        var disabled = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.AccountFeatures.Add(Feature(enabledCustom, enabled: true, "{\"retentionDays\": 45}"));
        context.AccountFeatures.Add(Feature(enabledDefault, enabled: true, configurationJson: null));
        context.AccountFeatures.Add(Feature(disabled, enabled: false, "{\"retentionDays\": 45}"));
        context.SaveChanges();

        var custom = await new PositionRetentionPolicyReader(context, TestDb.PrincipalFor(enabledCustom)).GetAsync(enabledCustom, CancellationToken.None);
        var def = await new PositionRetentionPolicyReader(context, TestDb.PrincipalFor(enabledDefault)).GetAsync(enabledDefault, CancellationToken.None);
        var off = await new PositionRetentionPolicyReader(context, TestDb.PrincipalFor(disabled)).GetAsync(disabled, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(custom.HistoryEnabled, Is.True);
            Assert.That(custom.RetentionDays, Is.EqualTo(45));
            Assert.That(def.HistoryEnabled, Is.True);
            Assert.That(def.RetentionDays, Is.EqualTo(30), "default when no retentionDays configured");
            Assert.That(off.HistoryEnabled, Is.False);
        }
        await context.DisposeAsync();
    }

    private static TransporterPositionHistory History(Guid accountId, Guid operatorId, Guid transporterId, DateTimeOffset at, string key)
        => new(accountId, operatorId, Guid.NewGuid(), transporterId, at, at, 1, 2, null, 0, null, null, null, null, null, null, null, key);

    private static AccountFeature Feature(Guid accountId, bool enabled, string? configurationJson)
        => new()
        {
            AccountFeatureId = Guid.NewGuid(),
            AccountId = accountId,
            FeatureKey = FeatureKeys.GpsPositionHistory,
            Enabled = enabled,
            Source = "Account",
            EffectiveFrom = DateTimeOffset.UtcNow.AddDays(-1),
            ConfigurationJson = configurationJson
        };
}
