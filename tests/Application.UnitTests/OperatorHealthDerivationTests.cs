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

using Ardalis.GuardClauses;
using TrackHub.Telemetry.Domain.Enums;
using TrackHub.Telemetry.Infrastructure.TelemetryDB;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;

namespace TrackHub.Telemetry.Application.UnitTests;

// Slice B: operator health/sync summary is DERIVED from the telemetry tables at read time (the
// denormalized operator columns are no longer written). These tests pin the derivation semantics.
[TestFixture]
public class OperatorHealthDerivationTests
{
    private static (ApplicationDbContext Context, OperatorHealthCheckReader Reader, Guid AccountId, Guid OperatorId) Arrange()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.Operators.Add(new Operator { OperatorId = operatorId, AccountId = accountId });
        context.SaveChanges();
        var reader = new OperatorHealthCheckReader(context, TestDb.PrincipalFor(accountId));
        return (context, reader, accountId, operatorId);
    }

    private static OperatorHealthCheck Check(Guid accountId, Guid operatorId, OperatorHealthStatus status, OperatorHealthCheckType type, DateTimeOffset startedAt, int? latencyMs = null, string? errorCode = null, string? errorMessage = null)
        => new(accountId, operatorId, (int)type, (int)status, latencyMs, startedAt, startedAt.AddSeconds(1), errorCode, errorMessage, 0, null);

    private static OperatorSyncRun Run(Guid accountId, Guid operatorId, OperatorSyncResult result, DateTimeOffset startedAt, int devicesSeen = 0, int positionsRead = 0)
        => new(accountId, operatorId, (int)SyncTriggerType.Automatic, (int)result, startedAt)
        {
            CompletedAt = startedAt.AddSeconds(2),
            DevicesSeen = devicesSeen,
            PositionsRead = positionsRead
        };

    [Test]
    public async Task GetLatestHealth_NoHealthChecks_ReturnsUnknown()
    {
        var (context, reader, _, operatorId) = Arrange();

        var health = await reader.GetLatestHealthAsync(operatorId, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(health.HealthStatus, Is.EqualTo(OperatorHealthStatus.Unknown));
            Assert.That(health.LastLatencyMs, Is.Null);
            Assert.That(health.LastSuccessfulSyncAt, Is.Null);
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task GetLatestHealth_UsesMostRecentCheckForStatusAndLatency()
    {
        var (context, reader, accountId, operatorId) = Arrange();
        var now = DateTimeOffset.UtcNow;
        context.OperatorHealthChecks.Add(Check(accountId, operatorId, OperatorHealthStatus.Healthy, OperatorHealthCheckType.Ping, now.AddMinutes(-10), latencyMs: 40));
        context.OperatorHealthChecks.Add(Check(accountId, operatorId, OperatorHealthStatus.Offline, OperatorHealthCheckType.Ping, now.AddMinutes(-1), latencyMs: 900, errorCode: "TIMEOUT", errorMessage: "no response"));
        context.SaveChanges();

        var health = await reader.GetLatestHealthAsync(operatorId, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(health.HealthStatus, Is.EqualTo(OperatorHealthStatus.Offline));
            Assert.That(health.LastLatencyMs, Is.EqualTo(900));
            Assert.That(health.LastFailureCode, Is.EqualTo("TIMEOUT"));
            Assert.That(health.LastFailureMessage, Is.EqualTo("no response"));
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task GetLatestHealth_PartiallySucceededRun_CountsAsLastSuccessfulSync()
    {
        var (context, reader, accountId, operatorId) = Arrange();
        var at = DateTimeOffset.UtcNow.AddMinutes(-5);
        context.OperatorSyncRuns.Add(Run(accountId, operatorId, OperatorSyncResult.PartiallySucceeded, at, devicesSeen: 3));
        context.SaveChanges();

        var health = await reader.GetLatestHealthAsync(operatorId, CancellationToken.None);

        Assert.That(health.LastSuccessfulSyncAt, Is.EqualTo(at.AddSeconds(2)));
        await context.DisposeAsync();
    }

    [Test]
    public async Task GetLatestHealth_DistinguishesDeviceSyncFromPositionSync()
    {
        var (context, reader, accountId, operatorId) = Arrange();
        var deviceAt = DateTimeOffset.UtcNow.AddMinutes(-20);
        var positionAt = DateTimeOffset.UtcNow.AddMinutes(-2);
        // A device sync (DevicesSeen > 0, PositionsRead == 0) and a position sync (PositionsRead > 0, DevicesSeen == 0).
        context.OperatorSyncRuns.Add(Run(accountId, operatorId, OperatorSyncResult.Succeeded, deviceAt, devicesSeen: 12));
        context.OperatorSyncRuns.Add(Run(accountId, operatorId, OperatorSyncResult.Succeeded, positionAt, positionsRead: 50));
        context.SaveChanges();

        var health = await reader.GetLatestHealthAsync(operatorId, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(health.LastDeviceSyncAt, Is.EqualTo(deviceAt.AddSeconds(2)), "device sync = latest run with DevicesSeen > 0");
            Assert.That(health.LastPositionSyncAt, Is.EqualTo(positionAt.AddSeconds(2)), "position sync = latest run with PositionsRead > 0");
        }
        await context.DisposeAsync();
    }

    [Test]
    public void GetLatestHealth_UnknownOperator_Throws()
    {
        var context = TestDb.NewContext();
        var reader = new OperatorHealthCheckReader(context, TestDb.PrincipalFor(Guid.NewGuid()));

        Assert.ThrowsAsync<NotFoundException>(() => reader.GetLatestHealthAsync(Guid.NewGuid(), CancellationToken.None));
        context.Dispose();
    }
}
