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

using Common.Application.Interfaces;
using Common.Domain.Constants;
using Moq;
using TrackHub.Manager.Domain.Enums;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class TransporterRetirementTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options);

    private static ICurrentPrincipal Administrator(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.AccountId).Returns(accountId);
        principal.SetupGet(x => x.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        principal.SetupGet(x => x.Role).Returns(Roles.Administrator);
        return principal.Object;
    }

    [Test]
    public async Task Retire_ReleasesTheDevicesAndDropsThePosition_ButKeepsTheUnitAndItsHistory()
    {
        using var context = NewContext(nameof(Retire_ReleasesTheDevicesAndDropsThePosition_ButKeepsTheUnitAndItsHistory));
        var accountId = Guid.NewGuid();
        var @operator = new Operator("Op", null, null, null, null, null, 1, accountId);
        var transporter = new Transporter("Truck 1", 1, accountId);
        var device = new Device("Device 1", 1, "SER-1", 1, null, null, null, null, (int)DetectedStatus.Assigned, @operator.OperatorId, accountId);
        context.Operators.Add(@operator);
        context.Transporters.Add(transporter);
        context.Devices.Add(device);
        context.TransporterDeviceAssignments.Add(new TransporterDeviceAssignment(accountId, transporter.TransporterId, device.DeviceId,
            DateTimeOffset.UtcNow.AddDays(-1), 0, true, (int)AssignmentStatus.Active, "seed", "User"));
        context.TransporterPositions.Add(new TransporterPosition(transporter.TransporterId, null, 4.6, -74.0, null, DateTimeOffset.UtcNow, 0, null, null, null, null, null, null, null));
        context.SaveChanges();
        context.ChangeTracker.Clear();
        var principal = Administrator(accountId);

        await new TransporterWriter(context, principal).RetireTransporterAsync(transporter.TransporterId, CancellationToken.None);

        var page = await new TransporterReader(context, principal)
            .GetTransportersByAccountAsync(accountId, 0, 50, null, CancellationToken.None);
        var visibleForHistory = await new VisibleTransporterReader(context, principal)
            .GetVisibleTransporterIdsAsync(principal.UserId!.Value, accountId, CancellationToken.None);
        Assert.That(visibleForHistory, Does.Contain(transporter.TransporterId), "history checks still see a retired unit");
        Assert.Multiple(() =>
        {
            Assert.That(context.Transporters.Single().RetiredAt, Is.Not.Null);
            Assert.That(context.TransporterDeviceAssignments.Single().Status, Is.EqualTo((int)AssignmentStatus.Ended));
            Assert.That(context.Devices.Single().DetectedStatus, Is.EqualTo((int)DetectedStatus.Available));
            Assert.That(context.TransporterPositions.Any(), Is.False);
            Assert.That(page.TotalCount, Is.Zero);
        });

        await new TransporterWriter(context, principal).RestoreTransporterAsync(transporter.TransporterId, CancellationToken.None);

        Assert.That(context.Transporters.Single().RetiredAt, Is.Null);
    }

    [Test]
    public async Task UpdateTransporter_WithAStaleVersion_IsRefused_AndWithTheLoadedOneSaves()
    {
        using var context = NewContext(nameof(UpdateTransporter_WithAStaleVersion_IsRefused_AndWithTheLoadedOneSaves));
        var accountId = Guid.NewGuid();
        var transporter = new Transporter("Truck 1", 1, accountId);
        context.Transporters.Add(transporter);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        var loaded = context.Transporters.AsNoTracking().Single().Version;

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => new TransporterWriter(context, Administrator(accountId))
            .UpdateTransporterAsync(new UpdateTransporterDto(transporter.TransporterId, "Truck 2", 1, loaded + 1), CancellationToken.None));
        context.ChangeTracker.Clear();

        await new TransporterWriter(context, Administrator(accountId))
            .UpdateTransporterAsync(new UpdateTransporterDto(transporter.TransporterId, "Truck 3", 1, loaded), CancellationToken.None);

        Assert.That(context.Transporters.AsNoTracking().Single().Name, Is.EqualTo("Truck 3"));
    }

    [Test]
    public async Task Retire_ReleasesTheStaff_SoNoStaffKeepsARetiredUnit()
    {
        using var context = NewContext(nameof(Retire_ReleasesTheStaff_SoNoStaffKeepsARetiredUnit));
        var accountId = Guid.NewGuid();
        var transporter = new Transporter("Truck 1", 1, accountId);
        var staff = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, transporter.TransporterId);
        context.Transporters.Add(transporter);
        context.StaffMembers.Add(staff);
        context.StaffTransporterAssignments.Add(new StaffTransporterAssignment(accountId, staff.StaffId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddDays(-2), null, "Regular", StaffAssignmentStatuses.Active, "user:1"));
        context.StaffTransporterAssignments.Add(new StaffTransporterAssignment(accountId, staff.StaffId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddDays(3), null, "Temporary", StaffAssignmentStatuses.Active, "user:1"));
        context.SaveChanges();
        context.ChangeTracker.Clear();

        await new TransporterWriter(context, Administrator(accountId)).RetireTransporterAsync(transporter.TransporterId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(context.StaffTransporterAssignments.Select(a => a.Status), Is.All.EqualTo(StaffAssignmentStatuses.Ended));
            Assert.That(context.StaffTransporterAssignments.All(a => a.EndsAt != null), Is.True);
            Assert.That(context.StaffMembers.Single().DefaultTransporterId, Is.Null);
        });
    }
}
