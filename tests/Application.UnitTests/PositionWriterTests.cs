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
public class PositionWriterTests
{
    private static TransporterPositionDto PositionDto(Guid transporterId, double latitude, DateTimeOffset deviceDateTime)
        => new(transporterId, null, latitude, 0, null, deviceDateTime, 0, null, null, null, null, null, null, null);

    [Test]
    public async Task Bulk_InsertsNewLatestPosition()
    {
        var transporterId = Guid.NewGuid();
        var context = TestDb.NewContext();
        var writer = new TransporterPositionWriter(context);

        await writer.BulkTransporterPositionAsync([PositionDto(transporterId, 10, DateTimeOffset.UtcNow)], CancellationToken.None);

        var row = await context.TransporterPositions.SingleAsync();
        Assert.That(row.Latitude, Is.EqualTo(10));
        await context.DisposeAsync();
    }

    [Test]
    public async Task Bulk_SkipsStaleFix_ButAppliesFresherFix()
    {
        var transporterId = Guid.NewGuid();
        var context = TestDb.NewContext();
        var existingAt = DateTime.UtcNow;
        context.TransporterPositions.Add(new TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities.TransporterPosition(transporterId, null, 1, 1, null, existingAt, TimeSpan.Zero, 0, null, null, null, null, null, null, null));
        context.SaveChanges();
        var writer = new TransporterPositionWriter(context);

        // Stale (older) fix is ignored.
        await writer.BulkTransporterPositionAsync([PositionDto(transporterId, 99, existingAt.AddHours(-1))], CancellationToken.None);
        Assert.That((await context.TransporterPositions.SingleAsync()).Latitude, Is.EqualTo(1), "older fix does not overwrite the latest");

        // Fresher fix wins.
        await writer.BulkTransporterPositionAsync([PositionDto(transporterId, 77, existingAt.AddHours(1))], CancellationToken.None);
        Assert.That((await context.TransporterPositions.SingleAsync()).Latitude, Is.EqualTo(77), "newer fix overwrites the latest");
        await context.DisposeAsync();
    }

    [Test]
    public async Task RecordSyncRun_MapsVm_AndPersists()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.Operators.Add(new Operator { OperatorId = operatorId, AccountId = accountId });
        context.SaveChanges();
        var writer = new OperatorSyncRunWriter(context, TestDb.PrincipalFor(accountId));

        var vm = await writer.RecordAsync(new OperatorSyncRunDto(accountId, operatorId, SyncTriggerType.Manual, OperatorSyncResult.PartiallySucceeded,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 5, 2, 1, 0, 0, 10, 9, 1, "E", "err", "corr"), CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(vm.Result, Is.EqualTo(OperatorSyncResult.PartiallySucceeded));
            Assert.That(vm.TriggerType, Is.EqualTo(SyncTriggerType.Manual));
            Assert.That(vm.DevicesSeen, Is.EqualTo(5));
            Assert.That(await context.OperatorSyncRuns.CountAsync(), Is.EqualTo(1));
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task RecordSyncRun_OperatorInDifferentAccount_Throws()
    {
        var caller = Guid.NewGuid();
        var operatorAccount = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        var context = TestDb.NewContext();
        context.Operators.Add(new Operator { OperatorId = operatorId, AccountId = operatorAccount });
        context.SaveChanges();
        var writer = new OperatorSyncRunWriter(context, TestDb.PrincipalFor(caller));

        Assert.ThrowsAsync<ForbiddenAccessException>(() => writer.RecordAsync(new OperatorSyncRunDto(caller, operatorId, SyncTriggerType.Manual, OperatorSyncResult.Succeeded,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 0, 0, 0, 0, 0, 0, 0, 0, null, null, null), CancellationToken.None));
        await context.DisposeAsync();
    }

    [Test]
    public async Task ResolvedAddress_WritesBothRows_ButNeverOverwritesAnExistingAddress()
    {
        var transporterId = Guid.NewGuid();
        var historyId = Guid.NewGuid();
        var context = TestDb.NewContext();
        // latest position with no address; history row that already has an address.
        context.TransporterPositions.Add(new TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities.TransporterPosition(transporterId, null, 1, 1, null, DateTime.UtcNow, TimeSpan.Zero, 0, null, null, null, null, null, null, null));
        var history = new TransporterPositionHistory(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), transporterId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, 1, 2, null, 0, null, null, "Existing St", null, null, null, null, "K1");
        typeof(TransporterPositionHistory).GetProperty(nameof(TransporterPositionHistory.TransporterPositionHistoryId))!.SetValue(history, historyId);
        context.TransporterPositionHistory.Add(history);
        context.SaveChanges();
        var writer = new ResolvedAddressWriter(context);

        var updated = await writer.PersistResolvedAddressAsync(historyId, transporterId, "New Address", "City", "State", "Country", CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated, Is.True, "the blank latest-position address was written");
            Assert.That((await context.TransporterPositions.SingleAsync()).Address, Is.EqualTo("New Address"));
            Assert.That((await context.TransporterPositionHistory.SingleAsync()).Address, Is.EqualTo("Existing St"), "already-addressed history row is not overwritten");
        }
        await context.DisposeAsync();
    }

}
