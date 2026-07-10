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

using TrackHub.Telemetry.Domain.Enums;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities;
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Readers;
using TransporterPositionEntity = TrackHub.Telemetry.Infrastructure.TelemetryDB.Entities.TransporterPosition;

namespace TrackHub.Telemetry.Application.UnitTests;

// The batched live-map read must return the union of the singular per-operator reads in one
// call, with identical visibility scoping.
[TestFixture]
public class BatchedPositionsTests
{
    private static TransporterPositionEntity Position(Guid transporterId)
        => new(transporterId, null, 4.6, -74.0, null, DateTime.UtcNow, TimeSpan.Zero, 0, null, null, null, null, null, null, null);

    private static (Infrastructure.TelemetryDB.ApplicationDbContext Context, Guid AccountId, Guid UserId, Guid OperatorA, Guid OperatorB, Guid TransporterA, Guid TransporterB) Seed()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var operatorA = Guid.NewGuid();
        var operatorB = Guid.NewGuid();
        var transporterA = Guid.NewGuid();
        var transporterB = Guid.NewGuid();
        const long groupId = 7L;

        var context = TestDb.NewContext();
        context.Transporters.Add(new Transporter { TransporterId = transporterA, AccountId = accountId, Name = "A", TransporterTypeId = 1 });
        context.Transporters.Add(new Transporter { TransporterId = transporterB, AccountId = accountId, Name = "B", TransporterTypeId = 1 });
        context.Groups.Add(new Group { GroupId = groupId, AccountId = accountId });
        context.Set<TransporterGroup>().Add(new TransporterGroup { TransporterId = transporterA, GroupId = groupId });
        context.Set<TransporterGroup>().Add(new TransporterGroup { TransporterId = transporterB, GroupId = groupId });
        context.UsersGroup.Add(new UserGroup { UserId = userId, GroupId = groupId });

        var deviceA = new Device { DeviceId = Guid.NewGuid(), OperatorId = operatorA, AccountId = accountId };
        var deviceB = new Device { DeviceId = Guid.NewGuid(), OperatorId = operatorB, AccountId = accountId };
        context.Set<Device>().AddRange(deviceA, deviceB);
        context.Set<TransporterDeviceAssignment>().AddRange(
            new TransporterDeviceAssignment { TransporterDeviceAssignmentId = Guid.NewGuid(), AccountId = accountId, TransporterId = transporterA, DeviceId = deviceA.DeviceId, Status = (int)AssignmentStatus.Active, Device = deviceA },
            new TransporterDeviceAssignment { TransporterDeviceAssignmentId = Guid.NewGuid(), AccountId = accountId, TransporterId = transporterB, DeviceId = deviceB.DeviceId, Status = (int)AssignmentStatus.Active, Device = deviceB });

        context.TransporterPositions.AddRange(Position(transporterA), Position(transporterB));
        context.SaveChanges();
        return (context, accountId, userId, operatorA, operatorB, transporterA, transporterB);
    }

    [Test]
    public async Task BatchedRead_ReturnsUnionOfPerOperatorReads()
    {
        var (context, accountId, userId, operatorA, operatorB, transporterA, transporterB) = Seed();
        context.Users.Add(new User { UserId = userId, AccountId = accountId });
        context.SaveChanges();
        var reader = new TransporterPositionReader(context, new VisibleTransporterReader(context, TestDb.PrincipalFor(accountId, Common.Application.Interfaces.PrincipalType.User, userId, role: null)));

        var single = await reader.GetTransporterPositionsAsync(userId, operatorA, CancellationToken.None);
        var batched = await reader.GetTransporterPositionsAsync(userId, new[] { operatorA, operatorB }, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(single.Select(p => p.TransporterId), Is.EquivalentTo(new[] { transporterA }));
            Assert.That(batched.Select(p => p.TransporterId), Is.EquivalentTo(new[] { transporterA, transporterB }));
        }
        await context.DisposeAsync();
    }

    [Test]
    public async Task BatchedRead_EmptyOperatorList_ReturnsEmpty()
    {
        var (context, accountId, userId, _, _, _, _) = Seed();
        context.Users.Add(new User { UserId = userId, AccountId = accountId });
        context.SaveChanges();
        var reader = new TransporterPositionReader(context, new VisibleTransporterReader(context, TestDb.PrincipalFor(accountId, Common.Application.Interfaces.PrincipalType.User, userId, role: null)));

        var batched = await reader.GetTransporterPositionsAsync(userId, Array.Empty<Guid>(), CancellationToken.None);

        Assert.That(batched, Is.Empty);
        await context.DisposeAsync();
    }
}
