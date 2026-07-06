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

using Common.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using TrackHub.Telemetry.Domain.Enums;
using TrackHub.Telemetry.Domain.Records;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Writers;

namespace TrackHub.Telemetry.Application.UnitTests;

[TestFixture]
public class TelemetryWriterTests
{
    private static TransporterPositionHistoryDto HistoryDto(Guid accountId, Guid operatorId, Guid transporterId, Guid deviceId, string idempotencyKey)
        => new(accountId, operatorId, deviceId, transporterId, DateTimeOffset.UtcNow,
            Latitude: 1, Longitude: 2, Altitude: null, Speed: 0, Course: null, EventId: null,
            Address: null, City: null, State: null, Country: null, Attributes: null, IdempotencyKey: idempotencyKey);

    [Test]
    public async Task AppendRange_SkipsRowsWithAnExistingIdempotencyKey()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var context = TestDb.NewContext();
        // Pre-existing row for key "K1".
        context.TransporterPositionHistory.Add(new TransporterPositionHistory(
            accountId, operatorId, deviceId, transporterId, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(-1),
            1, 2, null, 0, null, null, null, null, null, null, null, "K1"));
        context.SaveChanges();
        var writer = new TransporterPositionHistoryWriter(context, TestDb.PrincipalFor(accountId));

        var appended = await writer.AppendRangeAsync(
            [HistoryDto(accountId, operatorId, transporterId, deviceId, "K1"), HistoryDto(accountId, operatorId, transporterId, deviceId, "K2")],
            CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(appended, Is.EqualTo(1), "only the new key is appended");
            Assert.That(await context.TransporterPositionHistory.CountAsync(x => x.IdempotencyKey == "K2"), Is.EqualTo(1));
            Assert.That(await context.TransporterPositionHistory.CountAsync(x => x.IdempotencyKey == "K1"), Is.EqualTo(1), "existing key not duplicated");
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task AppendRange_ResolvesDeviceIdFromActivePrimaryAssignment_WhenMissing()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        var primaryDeviceId = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.TransporterDeviceAssignments.Add(new TransporterDeviceAssignment
        {
            TransporterDeviceAssignmentId = Guid.NewGuid(),
            AccountId = accountId,
            TransporterId = transporterId,
            DeviceId = primaryDeviceId,
            Status = (int)AssignmentStatus.Active,
            IsPrimary = true,
            Priority = 0
        });
        context.SaveChanges();
        var writer = new TransporterPositionHistoryWriter(context, TestDb.PrincipalFor(accountId));

        var appended = await writer.AppendRangeAsync(
            [HistoryDto(accountId, operatorId, transporterId, Guid.Empty, "K1")],
            CancellationToken.None);

        var row = await context.TransporterPositionHistory.SingleAsync();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(appended, Is.EqualTo(1));
            Assert.That(row.DeviceId, Is.EqualTo(primaryDeviceId), "empty DeviceId resolved from the active primary assignment");
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task RecordHealth_InsertsCheck_AndDoesNotWriteOperatorSummary()
    {
        // The minimal Operator scoping entity has no health-summary columns: the writer only inserts
        // the check row (Slice B: the operator summary is derived, never written by Telemetry).
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.Operators.Add(new Operator { OperatorId = operatorId, AccountId = accountId });
        context.SaveChanges();
        var writer = new OperatorHealthCheckWriter(context, TestDb.PrincipalFor(accountId));

        var vm = await writer.RecordAsync(new OperatorHealthCheckDto(
            accountId, operatorId, OperatorHealthCheckType.Ping, OperatorHealthStatus.Degraded,
            LatencyMs: 120, StartedAt: DateTimeOffset.UtcNow, CompletedAt: DateTimeOffset.UtcNow,
            ErrorCode: "SLOW", ErrorMessage: "latency high", RetryCount: 1, CorrelationId: "c1"),
            CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.Status, Is.EqualTo(OperatorHealthStatus.Degraded));
            Assert.That(await context.OperatorHealthChecks.CountAsync(), Is.EqualTo(1));
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task RecordHealth_OperatorInDifferentAccount_Throws()
    {
        var callerAccount = Guid.NewGuid();
        var operatorAccount = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.Operators.Add(new Operator { OperatorId = operatorId, AccountId = operatorAccount });
        context.SaveChanges();
        var writer = new OperatorHealthCheckWriter(context, TestDb.PrincipalFor(callerAccount));

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.RecordAsync(new OperatorHealthCheckDto(
            callerAccount, operatorId, OperatorHealthCheckType.Ping, OperatorHealthStatus.Healthy,
            null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, null, 0, null), CancellationToken.None));
        await context.DisposeAsync();
    }
}
