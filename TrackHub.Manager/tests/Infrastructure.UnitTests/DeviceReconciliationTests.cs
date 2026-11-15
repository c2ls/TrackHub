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
using Moq;
using TrackHub.Manager.Domain.Enums;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

// A provider catalog is reconciled in one save: devices it no longer lists are retired (status
// Removed, assignment ended, transporter kept), devices it lists again are revived and reported as
// added, and an empty catalog for a populated operator is refused instead of retiring the fleet.
[TestFixture]
public class DeviceReconciliationTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

    private static DeviceWriter Writer(ApplicationDbContext context)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.ServiceClient);
        principal.SetupGet(p => p.AccountId).Returns((Guid?)null);
        return new DeviceWriter(context as IApplicationDbContext, principal.Object, ImmediateOperatorCatalogGate.Instance);
    }

    private static DeviceDto Dto(Guid accountId, Guid operatorId, int identifier)
        => new(accountId, operatorId, $"SER-{identifier}", $"Device {identifier}", identifier, null, 4, null, "hash", "ACTIVE");

    private static async Task<(Guid AccountId, Operator Operator, Device Device, Transporter Transporter, TransporterDeviceAssignment Assignment)> SeedAsync(ApplicationDbContext context)
    {
        var accountId = Guid.NewGuid();
        var op = new Operator("Op", null, null, null, null, null, 1, accountId);
        var device = new Device("Device 1", 1, "SER-1", 4, null, null, "hash", "ACTIVE", (int)DetectedStatus.Assigned, op.OperatorId, accountId);
        var transporter = new Transporter("ABC123", 1, accountId);
        var assignment = new TransporterDeviceAssignment(accountId, transporter.TransporterId, device.DeviceId, DateTimeOffset.UtcNow.AddDays(-1), 0, true, (int)AssignmentStatus.Active, "seed", "User");
        await context.Operators.AddAsync(op);
        await context.Devices.AddAsync(device);
        await context.Transporters.AddAsync(transporter);
        await context.TransporterDeviceAssignments.AddAsync(assignment);
        await context.SaveChangesAsync(CancellationToken.None);
        return (accountId, op, device, transporter, assignment);
    }

    [Test]
    public async Task AMissingDevice_IsRetiredAndItsAssignmentEnded_TheTransporterSurvives()
    {
        await using var context = NewContext(nameof(AMissingDevice_IsRetiredAndItsAssignmentEnded_TheTransporterSurvives));
        var (accountId, op, device, transporter, assignment) = await SeedAsync(context);

        var result = await Writer(context).ReconcileSynchronizedDevicesAsync(op.OperatorId, [Dto(accountId, op.OperatorId, 2)], false, CancellationToken.None);

        var stored = await context.Devices.AsNoTracking().SingleAsync(d => d.DeviceId == device.DeviceId);
        var storedAssignment = await context.TransporterDeviceAssignments.AsNoTracking().SingleAsync(a => a.TransporterDeviceAssignmentId == assignment.TransporterDeviceAssignmentId);
        Assert.Multiple(async () =>
        {
            Assert.That(result.Added.Select(d => d.Identifier), Is.EquivalentTo(new[] { 2 }));
            Assert.That(result.Retired.Select(d => d.DeviceId), Is.EquivalentTo(new[] { device.DeviceId }));
            Assert.That(stored.DetectedStatus, Is.EqualTo((int)DetectedStatus.Removed));
            Assert.That(stored.RemovedAt, Is.Not.Null);
            Assert.That(storedAssignment.Status, Is.EqualTo((int)AssignmentStatus.Ended));
            Assert.That(await context.Transporters.AnyAsync(t => t.TransporterId == transporter.TransporterId), Is.True, "the transporter keeps its id and history");
        });
    }

    [Test]
    public async Task ADeviceListedAgain_IsRevivedOnce_AndNotRetiredTwice()
    {
        await using var context = NewContext(nameof(ADeviceListedAgain_IsRevivedOnce_AndNotRetiredTwice));
        var (accountId, op, device, _, _) = await SeedAsync(context);
        var writer = Writer(context);

        await writer.ReconcileSynchronizedDevicesAsync(op.OperatorId, [Dto(accountId, op.OperatorId, 2)], false, CancellationToken.None);
        var second = await writer.ReconcileSynchronizedDevicesAsync(op.OperatorId, [Dto(accountId, op.OperatorId, 2)], false, CancellationToken.None);
        var revived = await writer.ReconcileSynchronizedDevicesAsync(op.OperatorId, [Dto(accountId, op.OperatorId, 1), Dto(accountId, op.OperatorId, 2)], false, CancellationToken.None);

        var stored = await context.Devices.AsNoTracking().SingleAsync(d => d.DeviceId == device.DeviceId);
        Assert.Multiple(() =>
        {
            Assert.That(second.Retired, Is.Empty, "an already retired device is not reported again");
            Assert.That(revived.Added.Select(d => d.DeviceId), Is.EquivalentTo(new[] { device.DeviceId }), "the same row comes back, no duplicate is created");
            Assert.That(stored.DetectedStatus, Is.EqualTo((int)DetectedStatus.Available));
            Assert.That(stored.RemovedAt, Is.Null);
        });
    }

    [Test]
    public async Task AnEmptyCatalog_ForAPopulatedOperator_IsRefusedAndChangesNothing()
    {
        await using var context = NewContext(nameof(AnEmptyCatalog_ForAPopulatedOperator_IsRefusedAndChangesNothing));
        var (_, op, device, _, _) = await SeedAsync(context);

        await Assert.ThrowsAsync<ConflictException>(() => Writer(context).ReconcileSynchronizedDevicesAsync(op.OperatorId, [], false, CancellationToken.None));
        Assert.That((await context.Devices.AsNoTracking().SingleAsync(d => d.DeviceId == device.DeviceId)).DetectedStatus, Is.EqualTo((int)DetectedStatus.Assigned));
    }

    [Test]
    public async Task Reset_ReDetectsIgnoredDevices()
    {
        await using var context = NewContext(nameof(Reset_ReDetectsIgnoredDevices));
        var (accountId, op, device, _, _) = await SeedAsync(context);
        var tracked = await context.Devices.AsTracking().SingleAsync(d => d.DeviceId == device.DeviceId);
        tracked.DetectedStatus = (int)DetectedStatus.Ignored;
        tracked.IgnoredAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(CancellationToken.None);

        await Writer(context).ReconcileSynchronizedDevicesAsync(op.OperatorId, [Dto(accountId, op.OperatorId, 1)], false, CancellationToken.None);
        Assert.That((await context.Devices.AsNoTracking().SingleAsync(d => d.DeviceId == device.DeviceId)).DetectedStatus, Is.EqualTo((int)DetectedStatus.Ignored), "a plain sync respects the operator's ignore decision");

        await Writer(context).ReconcileSynchronizedDevicesAsync(op.OperatorId, [Dto(accountId, op.OperatorId, 1)], true, CancellationToken.None);
        var stored = await context.Devices.AsNoTracking().SingleAsync(d => d.DeviceId == device.DeviceId);
        Assert.Multiple(() =>
        {
            Assert.That(stored.DetectedStatus, Is.EqualTo((int)DetectedStatus.Available));
            Assert.That(stored.IgnoredAt, Is.Null);
        });
    }

    private static DeviceDto Unnumbered(Guid accountId, Guid operatorId, string serial)
        => new(accountId, operatorId, serial, serial, 0, null, 4, null, "hash", "ACTIVE");

    [Test]
    public async Task DevicesWithoutAProviderIdentifier_EachKeepTheirOwnRow_AcrossSyncs()
    {
        await using var context = NewContext(nameof(DevicesWithoutAProviderIdentifier_EachKeepTheirOwnRow_AcrossSyncs));
        var (accountId, op, seeded, _, _) = await SeedAsync(context);
        var writer = Writer(context);
        DeviceDto[] catalog = [Dto(accountId, op.OperatorId, 1), Unnumbered(accountId, op.OperatorId, "IMEI-A"), Unnumbered(accountId, op.OperatorId, "IMEI-B")];

        var first = await writer.ReconcileSynchronizedDevicesAsync(op.OperatorId, catalog, false, CancellationToken.None);
        var second = await writer.ReconcileSynchronizedDevicesAsync(op.OperatorId, catalog.Reverse().ToArray(), false, CancellationToken.None);

        var stored = await context.Devices.AsNoTracking().Where(d => d.OperatorId == op.OperatorId).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(first.Added.Select(d => d.Serial), Is.EquivalentTo(new[] { "IMEI-A", "IMEI-B" }));
            Assert.That(second.Added, Is.Empty);
            Assert.That(second.Retired, Is.Empty);
            Assert.That(stored.Select(d => d.Serial), Is.EquivalentTo(new[] { seeded.Serial, "IMEI-A", "IMEI-B" }));
            Assert.That(stored.Select(d => d.Identifier), Is.Unique);
            Assert.That(stored.Select(d => d.Identifier), Has.All.GreaterThan(0));
        });
    }

    [Test]
    public async Task AnAllocatedIdentifier_NeverTakesOneTheProviderSentInTheSameCatalog()
    {
        await using var context = NewContext(nameof(AnAllocatedIdentifier_NeverTakesOneTheProviderSentInTheSameCatalog));
        var (accountId, op, _, _, _) = await SeedAsync(context);

        await Writer(context).ReconcileSynchronizedDevicesAsync(
            op.OperatorId,
            [Dto(accountId, op.OperatorId, 1), Unnumbered(accountId, op.OperatorId, "IMEI-A"), Dto(accountId, op.OperatorId, 2)],
            false,
            CancellationToken.None);

        var stored = await context.Devices.AsNoTracking().Where(d => d.OperatorId == op.OperatorId).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(stored, Has.Count.EqualTo(3));
            Assert.That(stored.Single(d => d.Serial == "IMEI-A").Identifier, Is.EqualTo(3));
        });
    }
}
