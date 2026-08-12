using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class DriverWorkforceReaderTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId, PrincipalType type = PrincipalType.User, Guid? driverId = null)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.AccountId).Returns(accountId);
        principal.SetupGet(x => x.PrincipalType).Returns(type);
        principal.SetupGet(x => x.UserId).Returns(type == PrincipalType.User ? Guid.NewGuid() : null);
        principal.SetupGet(x => x.DriverId).Returns(driverId);
        return principal.Object;
    }

    private static DriverTransporterAssignment ActiveAssignment(Guid accountId, Guid driverId, Guid transporterId)
        => new(accountId, driverId, transporterId, DateTimeOffset.UtcNow.AddHours(-1), null,
            DriverAssignmentTypes.Regular, DriverAssignmentStatuses.Active, "User:test");

    [Test]
    public async Task ValidateDriverAssignment_TrueForAssignmentRow()
    {
        using var context = NewContext(nameof(ValidateDriverAssignment_TrueForAssignmentRow));
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        // No DefaultTransporterId at all — only the assignment row can satisfy this (AC4).
        var driver = new Driver(accountId, "Ana", null, null, null, true, null, null, null, null);
        context.Drivers.Add(driver);
        context.DriverTransporterAssignments.Add(ActiveAssignment(accountId, driver.DriverId, transporterId));
        context.SaveChanges();

        var reader = new DriverReader(context, Principal(accountId));
        var ok = await reader.ValidateDriverAssignmentAsync(driver.DriverId, "Transporter", transporterId.ToString(), CancellationToken.None);

        Assert.That(ok, Is.True);
    }

    [Test]
    public async Task ValidateDriverAssignment_TrueForDefaultTransporter_Unchanged()
    {
        using var context = NewContext(nameof(ValidateDriverAssignment_TrueForDefaultTransporter_Unchanged));
        var accountId = Guid.NewGuid();
        var transporter = new Transporter("Truck 1", 1, accountId);
        context.Transporters.Add(transporter);
        var driver = new Driver(accountId, "Ana", null, null, null, true, null, null, null, transporter.TransporterId);
        context.Drivers.Add(driver);
        context.SaveChanges();

        var reader = new DriverReader(context, Principal(accountId));
        var ok = await reader.ValidateDriverAssignmentAsync(driver.DriverId, "Transporter", transporter.TransporterId.ToString(), CancellationToken.None);

        // The legacy path DocumentAccessPolicy relies on must keep working untouched (AC4).
        Assert.That(ok, Is.True);
    }

    // Spec §5: a cross-account transporter reference must never validate. Writers now reject one, but
    // rows written before that check existed would otherwise let a driver reach another tenant's
    // documents through DocumentAccessPolicy.
    [Test]
    public async Task ValidateDriverAssignment_FalseForCrossAccountDefaultTransporter()
    {
        using var context = NewContext(nameof(ValidateDriverAssignment_FalseForCrossAccountDefaultTransporter));
        var accountId = Guid.NewGuid();
        var foreignTransporter = new Transporter("Foreign", 1, Guid.NewGuid());
        context.Transporters.Add(foreignTransporter);
        var driver = new Driver(accountId, "Ana", null, null, null, true, null, null, null, foreignTransporter.TransporterId);
        context.Drivers.Add(driver);
        context.SaveChanges();

        var reader = new DriverReader(context, Principal(accountId));
        var ok = await reader.ValidateDriverAssignmentAsync(driver.DriverId, "Transporter", foreignTransporter.TransporterId.ToString(), CancellationToken.None);

        Assert.That(ok, Is.False);
    }

    [Test]
    public async Task ValidateDriverAssignment_FalseForUnrelatedAndEndedAndInactive()
    {
        using var context = NewContext(nameof(ValidateDriverAssignment_FalseForUnrelatedAndEndedAndInactive));
        var accountId = Guid.NewGuid();
        var endedTransporterId = Guid.NewGuid();
        var driver = new Driver(accountId, "Ana", null, null, null, true, null, null, null, null);
        var inactive = new Driver(accountId, "Beto", null, null, null, false, null, null, null, Guid.NewGuid());
        context.Drivers.AddRange(driver, inactive);
        context.DriverTransporterAssignments.Add(new DriverTransporterAssignment(accountId, driver.DriverId, endedTransporterId,
            DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddDays(-1), DriverAssignmentTypes.Regular,
            DriverAssignmentStatuses.Ended, "User:test"));
        context.SaveChanges();

        var reader = new DriverReader(context, Principal(accountId));

        Assert.Multiple(async () =>
        {
            Assert.That(await reader.ValidateDriverAssignmentAsync(driver.DriverId, "Transporter", Guid.NewGuid().ToString(), CancellationToken.None), Is.False);
            // An ENDED assignment no longer grants access.
            Assert.That(await reader.ValidateDriverAssignmentAsync(driver.DriverId, "Transporter", endedTransporterId.ToString(), CancellationToken.None), Is.False);
            // An inactive driver is assigned to nothing, default transporter notwithstanding.
            Assert.That(await reader.ValidateDriverAssignmentAsync(inactive.DriverId, "Transporter", inactive.DefaultTransporterId!.Value.ToString(), CancellationToken.None), Is.False);
            // Resource types whose modules have not registered stay unknown.
            Assert.That(await reader.ValidateDriverAssignmentAsync(driver.DriverId, "Trip", Guid.NewGuid().ToString(), CancellationToken.None), Is.False);
        });
    }

    [Test]
    public async Task GetDriverAssignments_ReturnsRows_AndDoesNotDuplicateTheDefault()
    {
        using var context = NewContext(nameof(GetDriverAssignments_ReturnsRows_AndDoesNotDuplicateTheDefault));
        var accountId = Guid.NewGuid();
        var defaultTransporterId = Guid.NewGuid();
        var otherTransporterId = Guid.NewGuid();
        var driver = new Driver(accountId, "Ana", null, null, null, true, null, null, null, defaultTransporterId);
        context.Drivers.Add(driver);
        // One assignment covering the default transporter, one covering another.
        context.DriverTransporterAssignments.AddRange(
            ActiveAssignment(accountId, driver.DriverId, defaultTransporterId),
            ActiveAssignment(accountId, driver.DriverId, otherTransporterId));
        context.SaveChanges();

        var reader = new DriverReader(context, Principal(accountId));
        var assignments = await reader.GetDriverAssignmentsAsync(driver.DriverId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            // The default transporter is already covered by a row, so it is NOT synthesized again.
            Assert.That(assignments, Has.Count.EqualTo(2));
            Assert.That(assignments.Count(x => x.ResourceId == defaultTransporterId.ToString()), Is.EqualTo(1));
            // Real rows carry the additive time-bound fields; the synthesized entry does not.
            Assert.That(assignments.All(x => x.StartsAt.HasValue), Is.True);
        });
    }

    [Test]
    public async Task GetDriverAssignments_SynthesizesDefault_WhenNoRowCoversIt()
    {
        using var context = NewContext(nameof(GetDriverAssignments_SynthesizesDefault_WhenNoRowCoversIt));
        var accountId = Guid.NewGuid();
        var defaultTransporterId = Guid.NewGuid();
        var driver = new Driver(accountId, "Ana", null, null, null, true, null, null, null, defaultTransporterId);
        context.Drivers.Add(driver);
        context.SaveChanges();

        var reader = new DriverReader(context, Principal(accountId));
        var assignments = await reader.GetDriverAssignmentsAsync(driver.DriverId, CancellationToken.None);

        var only = assignments.Single();
        Assert.Multiple(() =>
        {
            Assert.That(only.ResourceId, Is.EqualTo(defaultTransporterId.ToString()));
            Assert.That(only.StartsAt, Is.Null);
            Assert.That(only.AssignmentType, Is.Null);
        });
    }

    [Test]
    public async Task GetDriverQualifications_FiltersByExpiryWindow_AndOrdersSoonestFirst()
    {
        using var context = NewContext(nameof(GetDriverQualifications_FiltersByExpiryWindow_AndOrdersSoonestFirst));
        var accountId = Guid.NewGuid();
        var driver = new Driver(accountId, "Ana", null, null, null, true, null, null, null, null);
        context.Drivers.Add(driver);
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        context.DriverQualifications.AddRange(
            new DriverQualification(accountId, driver.DriverId, DriverQualificationTypes.License, null, "soon", null, today.AddDays(5), null, DriverQualificationStatuses.Valid, null, null),
            new DriverQualification(accountId, driver.DriverId, DriverQualificationTypes.MedicalExam, null, "pastdue", null, today.AddDays(-3), null, DriverQualificationStatuses.Valid, null, null),
            new DriverQualification(accountId, driver.DriverId, DriverQualificationTypes.Training, null, "far", null, today.AddDays(200), null, DriverQualificationStatuses.Valid, null, null),
            new DriverQualification(accountId, driver.DriverId, DriverQualificationTypes.Other, null, "undated", null, null, null, DriverQualificationStatuses.Valid, null, null));
        context.SaveChanges();

        var reader = new DriverQualificationReader(context, Principal(accountId));
        var expiring = await reader.GetDriverQualificationsAsync(accountId, null, 30, 0, 50, CancellationToken.None);

        Assert.Multiple(() =>
        {
            // Past-due rows always qualify; the far-future and undated ones do not.
            Assert.That(expiring.Select(x => x.Number), Is.EqualTo(new[] { "pastdue", "soon" }));
            Assert.That(expiring.First().DriverName, Is.EqualTo("Ana"));
        });

        var all = await reader.GetDriverQualificationsAsync(accountId, null, null, 0, 50, CancellationToken.None);
        // Unfiltered: soonest expiry first, undated last.
        Assert.That(all.Select(x => x.Number), Is.EqualTo(new[] { "pastdue", "soon", "far", "undated" }));
    }

    [Test]
    public void GetDriverQualifications_ForAnotherAccount_IsForbidden()
    {
        using var context = NewContext(nameof(GetDriverQualifications_ForAnotherAccount_IsForbidden));
        var reader = new DriverQualificationReader(context, Principal(Guid.NewGuid()));

        Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            reader.GetDriverQualificationsAsync(Guid.NewGuid(), null, null, 0, 50, CancellationToken.None));
    }

    [Test]
    public async Task GetDriverAssignmentHistory_FiltersByWindowAndTransporter()
    {
        using var context = NewContext(nameof(GetDriverAssignmentHistory_FiltersByWindowAndTransporter));
        var accountId = Guid.NewGuid();
        var driver = new Driver(accountId, "Ana", null, null, null, true, null, null, null, null);
        var truckA = new Transporter("A", 1, accountId);
        var truckB = new Transporter("B", 1, accountId);
        context.Drivers.Add(driver);
        context.Transporters.AddRange(truckA, truckB);
        context.DriverTransporterAssignments.AddRange(
            new DriverTransporterAssignment(accountId, driver.DriverId, truckA.TransporterId, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow.AddDays(-20), DriverAssignmentTypes.Regular, DriverAssignmentStatuses.Ended, "User:test"),
            new DriverTransporterAssignment(accountId, driver.DriverId, truckB.TransporterId, DateTimeOffset.UtcNow.AddDays(-5), null, DriverAssignmentTypes.Temporary, DriverAssignmentStatuses.Active, "User:test"));
        context.SaveChanges();

        var reader = new DriverAssignmentReader(context, Principal(accountId));

        var recent = await reader.GetDriverAssignmentHistoryAsync(accountId, null, null, DateTimeOffset.UtcNow.AddDays(-10), null, 0, 50, CancellationToken.None);
        // The 30→20-days-ago assignment closed before the window opened, so it is out.
        Assert.That(recent.Select(x => x.TransporterName), Is.EqualTo(new[] { "B" }));

        var byTransporter = await reader.GetDriverAssignmentHistoryAsync(accountId, null, truckA.TransporterId, null, null, 0, 50, CancellationToken.None);
        Assert.That(byTransporter.Select(x => x.TransporterName), Is.EqualTo(new[] { "A" }));
    }

    [Test]
    public async Task GetMyDriverProfile_ReturnsOnlyOwnData()
    {
        using var context = NewContext(nameof(GetMyDriverProfile_ReturnsOnlyOwnData));
        var accountId = Guid.NewGuid();
        var me = new Driver(accountId, "Ana", null, null, null, true, null, null, null, null);
        var other = new Driver(accountId, "Beto", null, null, null, true, null, null, null, null);
        var truck = new Transporter("A", 1, accountId);
        context.Drivers.AddRange(me, other);
        context.Transporters.Add(truck);
        context.DriverQualifications.AddRange(
            new DriverQualification(accountId, me.DriverId, DriverQualificationTypes.License, null, "mine", null, null, null, DriverQualificationStatuses.Valid, null, null),
            new DriverQualification(accountId, other.DriverId, DriverQualificationTypes.License, null, "theirs", null, null, null, DriverQualificationStatuses.Valid, null, null));
        context.DriverTransporterAssignments.AddRange(
            ActiveAssignment(accountId, me.DriverId, truck.TransporterId),
            ActiveAssignment(accountId, other.DriverId, truck.TransporterId));
        context.SaveChanges();

        var reader = new DriverReader(context, Principal(accountId, PrincipalType.Driver, me.DriverId));
        var profile = await reader.GetMyDriverProfileAsync(me.DriverId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(profile.Driver.Name, Is.EqualTo("Ana"));
            // Another driver's qualifications and assignments must never leak in (AC2).
            Assert.That(profile.Qualifications.Select(x => x.Number), Is.EqualTo(new[] { "mine" }));
            Assert.That(profile.ActiveAssignments, Has.Count.EqualTo(1));
            Assert.That(profile.ActiveAssignments.Single().DriverId, Is.EqualTo(me.DriverId));
        });
    }
}
