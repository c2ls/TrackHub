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
using Common.Domain.Enums;
using TrackHub.Manager.Domain.Enums;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using Moq;
using DetectedStatus = TrackHub.Manager.Domain.Enums.DetectedStatus;

namespace Infrastructure.UnitTests;

/// <summary>
/// The synchronized-device list is one SERVER page: status, operator, "unassigned only" and
/// "recently added only" are query arguments the reader composes, not post-filters over a loaded
/// page. Each test seeds a small account and asserts the narrowing is what the projection would show
/// — the detected-status branches especially, because the stored column only ever holds Ignored or
/// Removed while Assigned/Available are derived from the active assignments.
/// </summary>
[TestFixture]
public class DeviceReaderTests
{
    private const short DeviceTypeId = 1;

    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        return principal.Object;
    }

    private static Device NewDevice(
        Guid accountId,
        Guid operatorId,
        string name,
        DetectedStatus stored = DetectedStatus.Available,
        DateTimeOffset? firstSeenAt = null)
    {
        var device = new Device(name, 1, $"serial-{name}", DeviceTypeId, null, null, null, null, (int)stored, operatorId, accountId);
        if (firstSeenAt.HasValue)
        {
            device.FirstSeenAt = firstSeenAt.Value;
        }
        if (stored == DetectedStatus.Ignored)
        {
            device.IgnoredAt = DateTimeOffset.UtcNow;
        }
        return device;
    }

    private static TransporterDeviceAssignment ActiveAssignment(Guid accountId, Guid deviceId)
        => new(accountId, Guid.NewGuid(), deviceId, DateTimeOffset.UtcNow, 1, true, (int)AssignmentStatus.Active, null, "User");

    private static SynchronizedDeviceFilter Filter(
        DetectedStatus? status = null,
        Guid? operatorId = null,
        bool unassignedOnly = false,
        DateTimeOffset? firstSeenSince = null)
        => new(status, operatorId, unassignedOnly, firstSeenSince);

    [Test]
    public async Task SearchDevices_AssignedStatus_ReturnsOnlyDevicesWithAnActiveAssignment()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        await using var context = NewContext(nameof(SearchDevices_AssignedStatus_ReturnsOnlyDevicesWithAnActiveAssignment));

        var assigned = NewDevice(accountId, operatorId, "assigned");
        var available = NewDevice(accountId, operatorId, "available");
        await context.Devices.AddRangeAsync(assigned, available);
        await context.TransporterDeviceAssignments.AddAsync(ActiveAssignment(accountId, assigned.DeviceId));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DeviceReader(context, Principal(accountId));
        var page = await reader.SearchDevicesAsync(accountId, Filter(status: DetectedStatus.Assigned), 0, 50, null, CancellationToken.None);

        // The stored column on `assigned` is Available; only the active assignment makes it Assigned,
        // so a plain column comparison would return it under Available and miss it under Assigned.
        Assert.That(page.Items.Select(d => d.DeviceId), Is.EquivalentTo([assigned.DeviceId]));
    }

    [Test]
    public async Task SearchDevices_AvailableStatus_ExcludesAssignedIgnoredAndRemoved()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        await using var context = NewContext(nameof(SearchDevices_AvailableStatus_ExcludesAssignedIgnoredAndRemoved));

        var available = NewDevice(accountId, operatorId, "available");
        var assigned = NewDevice(accountId, operatorId, "assigned");
        var ignored = NewDevice(accountId, operatorId, "ignored", DetectedStatus.Ignored);
        var removed = NewDevice(accountId, operatorId, "removed", DetectedStatus.Removed);
        await context.Devices.AddRangeAsync(available, assigned, ignored, removed);
        await context.TransporterDeviceAssignments.AddAsync(ActiveAssignment(accountId, assigned.DeviceId));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DeviceReader(context, Principal(accountId));
        var page = await reader.SearchDevicesAsync(accountId, Filter(status: DetectedStatus.Available), 0, 50, null, CancellationToken.None);

        Assert.That(page.Items.Select(d => d.DeviceId), Is.EquivalentTo([available.DeviceId]));
    }

    [Test]
    public async Task SearchDevices_UnassignedOnly_KeepsEveryStatusExceptAssigned()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        await using var context = NewContext(nameof(SearchDevices_UnassignedOnly_KeepsEveryStatusExceptAssigned));

        var available = NewDevice(accountId, operatorId, "available");
        var assigned = NewDevice(accountId, operatorId, "assigned");
        var ignored = NewDevice(accountId, operatorId, "ignored", DetectedStatus.Ignored);
        var removed = NewDevice(accountId, operatorId, "removed", DetectedStatus.Removed);
        // An ignored device that ALSO carries an active assignment. The projection resolves it to
        // Ignored (stored status wins), so "unassigned only" must keep it — which only holds because
        // the reader OR's the Ignored/Removed branches in rather than testing the assignment alone.
        var ignoredButAssigned = NewDevice(accountId, operatorId, "ignored-assigned", DetectedStatus.Ignored);
        await context.Devices.AddRangeAsync(available, assigned, ignored, removed, ignoredButAssigned);
        await context.TransporterDeviceAssignments.AddRangeAsync(
            ActiveAssignment(accountId, assigned.DeviceId),
            ActiveAssignment(accountId, ignoredButAssigned.DeviceId));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DeviceReader(context, Principal(accountId));
        var page = await reader.SearchDevicesAsync(accountId, Filter(unassignedOnly: true), 0, 50, null, CancellationToken.None);

        // "Unassigned only" is wider than any single status: available + ignored + removed, never
        // assigned. This is exactly why it is not redundant with the status dropdown.
        Assert.That(
            page.Items.Select(d => d.DeviceId),
            Is.EquivalentTo([available.DeviceId, ignored.DeviceId, removed.DeviceId, ignoredButAssigned.DeviceId]));
    }

    [Test]
    public async Task SearchDevices_RecentOnly_KeepsOnlyDevicesFirstSeenWithinTheWindow()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        await using var context = NewContext(nameof(SearchDevices_RecentOnly_KeepsOnlyDevicesFirstSeenWithinTheWindow));

        var fresh = NewDevice(accountId, operatorId, "fresh", firstSeenAt: DateTimeOffset.UtcNow.AddHours(-1));
        var stale = NewDevice(accountId, operatorId, "stale", firstSeenAt: DateTimeOffset.UtcNow.AddDays(-3));
        await context.Devices.AddRangeAsync(fresh, stale);
        await context.SaveChangesAsync(CancellationToken.None);

        var since = DateTimeOffset.UtcNow.AddHours(-24);
        var reader = new DeviceReader(context, Principal(accountId));
        var page = await reader.SearchDevicesAsync(accountId, Filter(firstSeenSince: since), 0, 50, null, CancellationToken.None);

        Assert.That(page.Items.Select(d => d.DeviceId), Is.EquivalentTo([fresh.DeviceId]));
    }

    [Test]
    public async Task SearchDevices_OperatorFilter_KeepsOnlyThatOperatorsDevices()
    {
        var accountId = Guid.NewGuid();
        var operatorA = Guid.NewGuid();
        var operatorB = Guid.NewGuid();
        await using var context = NewContext(nameof(SearchDevices_OperatorFilter_KeepsOnlyThatOperatorsDevices));

        var a = NewDevice(accountId, operatorA, "a");
        var b = NewDevice(accountId, operatorB, "b");
        await context.Devices.AddRangeAsync(a, b);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DeviceReader(context, Principal(accountId));
        var page = await reader.SearchDevicesAsync(accountId, Filter(operatorId: operatorA), 0, 50, null, CancellationToken.None);

        Assert.That(page.Items.Select(d => d.DeviceId), Is.EquivalentTo([a.DeviceId]));
    }

    [Test]
    public async Task SearchDevices_ReportsTheUnpagedTotalNotThePageLength()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        await using var context = NewContext(nameof(SearchDevices_ReportsTheUnpagedTotalNotThePageLength));

        var devices = Enumerable.Range(0, 7).Select(i => NewDevice(accountId, operatorId, $"dev-{i}")).ToList();
        await context.Devices.AddRangeAsync(devices);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DeviceReader(context, Principal(accountId));
        var page = await reader.SearchDevicesAsync(accountId, Filter(), 0, 3, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(page.Items, Has.Count.EqualTo(3));
            Assert.That(page.TotalCount, Is.EqualTo(7));
        });
    }

    [Test]
    public async Task SearchDevices_PagesTiedNamesWithoutLosingRows()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        await using var context = NewContext(nameof(SearchDevices_PagesTiedNamesWithoutLosingRows));

        var devices = Enumerable.Range(0, 7).Select(_ => NewDevice(accountId, operatorId, "same")).ToList();
        await context.Devices.AddRangeAsync(devices);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DeviceReader(context, Principal(accountId));
        var seen = new List<Guid>();
        for (var skip = 0; ; skip += 2)
        {
            var page = await reader.SearchDevicesAsync(accountId, Filter(), skip, 2, null, CancellationToken.None);
            seen.AddRange(page.Items.Select(d => d.DeviceId));
            if (page.Items.Count == 0 || seen.Count >= page.TotalCount)
            {
                break;
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(seen, Is.Unique);
            Assert.That(seen, Is.EquivalentTo(devices.Select(d => d.DeviceId)));
        });
    }

    [Test]
    public async Task DeviceLookup_CarriesOperatorIdForTheDashboardJoin()
    {
        var accountId = Guid.NewGuid();
        var operatorId = Guid.NewGuid();
        await using var context = NewContext(nameof(DeviceLookup_CarriesOperatorIdForTheDashboardJoin));

        var device = NewDevice(accountId, operatorId, "unit");
        await context.Devices.AddAsync(device);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new DeviceReader(context, Principal(accountId));
        var lookup = await reader.GetDeviceLookupByAccountAsync(accountId, 5_001, CancellationToken.None);

        Assert.That(lookup.Single().OperatorId, Is.EqualTo(operatorId));
    }
}
