using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Enums;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

/// <summary>
/// The production context is NoTracking by default, so every context here is built NoTracking too:
/// a writer that mutates a loaded row without attaching it passes under the InMemory tracking
/// default while silently persisting nothing in production ("End Assignment does nothing").
/// </summary>
[TestFixture]
public class TransporterDeviceAssignmentWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.AccountId).Returns(accountId);
        principal.SetupGet(x => x.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        return principal.Object;
    }

    private static (Transporter Transporter, Device Device) Seed(ApplicationDbContext context, Guid accountId)
    {
        var @operator = new Operator("Op", null, null, null, null, null, 1, accountId);
        var transporter = new Transporter("Truck 1", 1, accountId);
        var device = new Device("Device 1", 1, "SER-1", 1, null, null, null, null,
            (int)DetectedStatus.Available, @operator.OperatorId, accountId);
        context.Operators.Add(@operator);
        context.Transporters.Add(transporter);
        context.Devices.Add(device);
        context.SaveChanges();
        // Added entities stay tracked even in a NoTracking context; drop them so the writer
        // works against untracked query results exactly as it does in production.
        context.ChangeTracker.Clear();
        return (transporter, device);
    }

    private static TransporterDeviceAssignment SeedActiveAssignment(
        ApplicationDbContext context, Guid accountId, Guid transporterId, Guid deviceId, bool isPrimary = true)
    {
        var assignment = new TransporterDeviceAssignment(accountId, transporterId, deviceId,
            DateTimeOffset.UtcNow.AddDays(-1), 0, isPrimary, (int)AssignmentStatus.Active, "seed", "User");
        context.TransporterDeviceAssignments.Add(assignment);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        return assignment;
    }

    [Test]
    public async Task EndAssignment_PersistsEndedStatusAndReason()
    {
        using var context = NewContext(nameof(EndAssignment_PersistsEndedStatusAndReason));
        var accountId = Guid.NewGuid();
        var (transporter, device) = Seed(context, accountId);
        var assignment = SeedActiveAssignment(context, accountId, transporter.TransporterId, device.DeviceId);
        var writer = new TransporterDeviceAssignmentWriter(context, Principal(accountId));

        await writer.EndAssignmentAsync(assignment.TransporterDeviceAssignmentId, "unit retired", CancellationToken.None);

        var stored = await context.TransporterDeviceAssignments
            .SingleAsync(a => a.TransporterDeviceAssignmentId == assignment.TransporterDeviceAssignmentId);
        Assert.Multiple(() =>
        {
            Assert.That(stored.Status, Is.EqualTo((int)AssignmentStatus.Ended));
            Assert.That(stored.EffectiveTo, Is.Not.Null);
            Assert.That(stored.AssignmentReason, Is.EqualTo("unit retired"));
            Assert.That(context.AuditEvents.Count(x => x.Action == "EndDeviceTransporterAssignment"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task EndAssignment_AlreadyEnded_IsANoOp()
    {
        using var context = NewContext(nameof(EndAssignment_AlreadyEnded_IsANoOp));
        var accountId = Guid.NewGuid();
        var (transporter, device) = Seed(context, accountId);
        var assignment = SeedActiveAssignment(context, accountId, transporter.TransporterId, device.DeviceId);
        var writer = new TransporterDeviceAssignmentWriter(context, Principal(accountId));
        await writer.EndAssignmentAsync(assignment.TransporterDeviceAssignmentId, "first", CancellationToken.None);

        await writer.EndAssignmentAsync(assignment.TransporterDeviceAssignmentId, "second", CancellationToken.None);

        var stored = await context.TransporterDeviceAssignments
            .SingleAsync(a => a.TransporterDeviceAssignmentId == assignment.TransporterDeviceAssignmentId);
        Assert.That(stored.AssignmentReason, Is.EqualTo("first"));
    }

    [Test]
    public async Task Assign_SupersedesPriorActiveAssignment()
    {
        using var context = NewContext(nameof(Assign_SupersedesPriorActiveAssignment));
        var accountId = Guid.NewGuid();
        var (transporter, device) = Seed(context, accountId);
        var prior = SeedActiveAssignment(context, accountId, transporter.TransporterId, device.DeviceId);
        var writer = new TransporterDeviceAssignmentWriter(context, Principal(accountId));

        var vm = await writer.AssignAsync(new TransporterDeviceAssignmentDto(
            accountId, transporter.TransporterId, device.DeviceId, 0, true, "portal"), CancellationToken.None);

        var storedPrior = await context.TransporterDeviceAssignments
            .SingleAsync(a => a.TransporterDeviceAssignmentId == prior.TransporterDeviceAssignmentId);
        var storedDevice = await context.Devices.SingleAsync(d => d.DeviceId == device.DeviceId);
        Assert.Multiple(() =>
        {
            Assert.That(vm.Status, Is.EqualTo(AssignmentStatus.Active));
            Assert.That(storedPrior.Status, Is.EqualTo((int)AssignmentStatus.Superseded));
            Assert.That(storedPrior.EffectiveTo, Is.Not.Null);
            Assert.That(storedDevice.LastAssignedAt, Is.Not.Null);
            Assert.That(storedDevice.DetectedStatus, Is.EqualTo((int)DetectedStatus.Assigned));
        });
    }

    [Test]
    public async Task Assign_Primary_DemotesOtherPrimaryOnAnotherDevice()
    {
        using var context = NewContext(nameof(Assign_Primary_DemotesOtherPrimaryOnAnotherDevice));
        var accountId = Guid.NewGuid();
        var (transporter, device) = Seed(context, accountId);
        var otherDevice = new Device("Device 2", 2, "SER-2", 1, null, null, null, null,
            (int)DetectedStatus.Assigned, device.OperatorId, accountId);
        context.Devices.Add(otherDevice);
        context.SaveChanges();
        context.ChangeTracker.Clear();
        var otherPrimary = SeedActiveAssignment(context, accountId, transporter.TransporterId, otherDevice.DeviceId);
        var writer = new TransporterDeviceAssignmentWriter(context, Principal(accountId));

        await writer.AssignAsync(new TransporterDeviceAssignmentDto(
            accountId, transporter.TransporterId, device.DeviceId, 0, true, "portal"), CancellationToken.None);

        var storedOther = await context.TransporterDeviceAssignments
            .SingleAsync(a => a.TransporterDeviceAssignmentId == otherPrimary.TransporterDeviceAssignmentId);
        Assert.Multiple(() =>
        {
            // The other device's assignment stays active — only its primary flag moves.
            Assert.That(storedOther.Status, Is.EqualTo((int)AssignmentStatus.Active));
            Assert.That(storedOther.IsPrimary, Is.False);
        });
    }
}
