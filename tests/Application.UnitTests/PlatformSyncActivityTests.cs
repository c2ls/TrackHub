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
using Moq;
using TrackHub.Telemetry.Application.GpsIntegration.Queries;
using TrackHub.Telemetry.Domain.Enums;
using TrackHub.Telemetry.Domain.Interfaces;
using TrackHub.Telemetry.Domain.Models;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;

namespace TrackHub.Telemetry.Application.UnitTests;

// SyncWorker liveness is derived from data recency (spec 28 ST-04): the worker has no HTTP
// listener, so the rows it writes every cycle ARE the health signal. This reader is deliberately
// unscoped — it must see every account's rows, which these tests pin.
[TestFixture]
public class PlatformSyncActivityTests
{
    private static OperatorSyncRun Run(Guid accountId, Guid operatorId, DateTimeOffset startedAt)
        => new(accountId, operatorId, (int)SyncTriggerType.Automatic, (int)OperatorSyncResult.Succeeded, startedAt) { CompletedAt = startedAt.AddSeconds(1) };

    private static OperatorHealthCheck Check(Guid accountId, Guid operatorId, DateTimeOffset startedAt)
        => new(accountId, operatorId, (int)OperatorHealthCheckType.Ping, (int)OperatorHealthStatus.Healthy, 12, startedAt, startedAt.AddSeconds(1), null, null, 0, null);

    private static AccountFeature GpsFeature(Guid accountId, bool enabled = true, DateTimeOffset? from = null, DateTimeOffset? to = null)
        => new()
        {
            AccountFeatureId = Guid.NewGuid(),
            AccountId = accountId,
            FeatureKey = FeatureKeys.GpsIntegration,
            Enabled = enabled,
            Source = "manual",
            EffectiveFrom = from,
            EffectiveTo = to
        };

    [Test]
    public async Task ReportsLatestActivityAcrossEveryAccount()
    {
        var now = DateTimeOffset.UtcNow;
        var accountA = Guid.NewGuid();
        var accountB = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.OperatorSyncRuns.AddRange(
            Run(accountA, Guid.NewGuid(), now.AddMinutes(-40)),
            Run(accountB, Guid.NewGuid(), now.AddMinutes(-2)));   // newest, different account
        context.OperatorHealthChecks.AddRange(
            Check(accountA, Guid.NewGuid(), now.AddMinutes(-90)), // outside the 60 min window
            Check(accountB, Guid.NewGuid(), now.AddMinutes(-1)));
        context.SaveChanges();
        var reader = new PlatformSyncActivityReader(context);

        var activity = await reader.GetAsync(now.AddHours(-1), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(activity.LastSyncRunAt, Is.EqualTo(now.AddMinutes(-2)), "the reader is unscoped — any account's row counts");
            Assert.That(activity.LastHealthCheckAt, Is.EqualTo(now.AddMinutes(-1)));
            Assert.That(activity.SyncRunsLastHour, Is.EqualTo(2));
            Assert.That(activity.HealthChecksLastHour, Is.EqualTo(1), "the 90-minute-old check is outside the window");
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task ReportsNoActivityWhenTheWorkerHasNeverRun()
    {
        var context = TestDb.NewContext();
        var reader = new PlatformSyncActivityReader(context);

        var activity = await reader.GetAsync(DateTimeOffset.UtcNow.AddHours(-1), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(activity.LastSyncRunAt, Is.Null);
            Assert.That(activity.LastHealthCheckAt, Is.Null);
            Assert.That(activity.SyncRunsLastHour, Is.Zero);
            Assert.That(activity.HealthChecksLastHour, Is.Zero);
            Assert.That(activity.HasEnabledGpsIntegration, Is.False, "nothing to sync ⇒ the portal renders Unknown, not Down");
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task HasEnabledGpsIntegration_RequiresBothAnEnabledFeatureAndAnOperator()
    {
        var now = DateTimeOffset.UtcNow;
        var featureOnly = Guid.NewGuid();
        var operatorOnly = Guid.NewGuid();
        var context = TestDb.NewContext();
        // An account with the feature but no operator, and an account with an operator but no feature:
        // neither means "there is work to sync".
        context.AccountFeatures.Add(GpsFeature(featureOnly));
        context.Operators.Add(new Operator { OperatorId = Guid.NewGuid(), AccountId = operatorOnly });
        context.SaveChanges();
        var reader = new PlatformSyncActivityReader(context);

        var activity = await reader.GetAsync(now.AddHours(-1), CancellationToken.None);

        Assert.That(activity.HasEnabledGpsIntegration, Is.False);
        await context.DisposeAsync();
    }

    [Test]
    public async Task HasEnabledGpsIntegration_IsTrueWhenAnEnabledAccountOwnsAnOperator()
    {
        var now = DateTimeOffset.UtcNow;
        var accountId = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.AccountFeatures.Add(GpsFeature(accountId));
        context.Operators.Add(new Operator { OperatorId = Guid.NewGuid(), AccountId = accountId });
        context.SaveChanges();
        var reader = new PlatformSyncActivityReader(context);

        var activity = await reader.GetAsync(now.AddHours(-1), CancellationToken.None);

        Assert.That(activity.HasEnabledGpsIntegration, Is.True);
        await context.DisposeAsync();
    }

    [Test]
    public async Task HasEnabledGpsIntegration_IgnoresDisabledAndOutOfWindowFeatureRows()
    {
        var now = DateTimeOffset.UtcNow;
        var disabled = Guid.NewGuid();
        var expired = Guid.NewGuid();
        var future = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.AccountFeatures.AddRange(
            GpsFeature(disabled, enabled: false),
            GpsFeature(expired, to: now.AddDays(-1)),
            GpsFeature(future, from: now.AddDays(1)));
        context.Operators.AddRange(
            new Operator { OperatorId = Guid.NewGuid(), AccountId = disabled },
            new Operator { OperatorId = Guid.NewGuid(), AccountId = expired },
            new Operator { OperatorId = Guid.NewGuid(), AccountId = future });
        context.SaveChanges();
        var reader = new PlatformSyncActivityReader(context);

        var activity = await reader.GetAsync(now.AddHours(-1), CancellationToken.None);

        Assert.That(activity.HasEnabledGpsIntegration, Is.False);
        await context.DisposeAsync();
    }

    [Test]
    public async Task Handler_ClampsTheLookbackWindow()
    {
        var reader = new Mock<IPlatformSyncActivityReader>();
        DateTimeOffset captured = default;
        reader.Setup(r => r.GetAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Callback<DateTimeOffset, CancellationToken>((since, _) => captured = since)
            .ReturnsAsync(new PlatformSyncActivityVm(null, null, 0, 0, false));
        var handler = new GetPlatformSyncActivityQueryHandler(reader.Object);

        // Zero/negative falls back to 60 minutes rather than querying "since now".
        await handler.Handle(new GetPlatformSyncActivityQuery(0), CancellationToken.None);
        Assert.That(captured, Is.LessThan(DateTimeOffset.UtcNow.AddMinutes(-59)));

        // An absurd window is capped at 24 h so this can never become a full-table scan.
        await handler.Handle(new GetPlatformSyncActivityQuery(int.MaxValue), CancellationToken.None);
        Assert.That(captured, Is.GreaterThan(DateTimeOffset.UtcNow.AddDays(-2)));
    }
}
