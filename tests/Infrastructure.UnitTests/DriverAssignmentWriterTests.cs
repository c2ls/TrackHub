using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;
using CommonConstants = Common.Domain.Constants;

namespace Infrastructure.UnitTests;

[TestFixture]
public class DriverAssignmentWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.AccountId).Returns(accountId);
        principal.SetupGet(x => x.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        return principal.Object;
    }

    private static (Driver Driver, Transporter Transporter) Seed(ApplicationDbContext context, Guid accountId, bool driverActive = true)
    {
        var driver = new Driver(accountId, "Ana", null, null, null, driverActive, null, null, null, null);
        var transporter = new Transporter("Truck 1", 1, accountId);
        context.Drivers.Add(driver);
        context.Transporters.Add(transporter);
        context.SaveChanges();
        return (driver, transporter);
    }

    private static void EnableWorkforce(ApplicationDbContext context, Guid accountId, string? configurationJson)
    {
        context.AccountFeatures.Add(new AccountFeature(accountId, CommonConstants.FeatureKeys.Workforce, true, "Standard", "Manual", null, null, configurationJson));
        context.SaveChanges();
    }

    [Test]
    public async Task Assign_CreatesActiveRow_AndAudits()
    {
        using var context = NewContext(nameof(Assign_CreatesActiveRow_AndAudits));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        var writer = new DriverAssignmentWriter(context, Principal(accountId));

        var vm = await writer.AssignDriverToTransporterAsync(driver.DriverId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddMinutes(-1), DriverAssignmentTypes.Regular, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Status, Is.EqualTo(DriverAssignmentStatuses.Active));
            Assert.That(vm.TransporterName, Is.EqualTo("Truck 1"));
            Assert.That(vm.EndsAt, Is.Null);
            Assert.That(context.AuditEvents.Count(x => x.Action == "AssignDriverToTransporter"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Assign_SamePairTwice_Conflicts()
    {
        using var context = NewContext(nameof(Assign_SamePairTwice_Conflicts));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        var writer = new DriverAssignmentWriter(context, Principal(accountId));
        await writer.AssignDriverToTransporterAsync(driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None);

        // One OPEN assignment per (driver, transporter) pair → 409 (AC3).
        Assert.ThrowsAsync<ConflictException>(() => writer.AssignDriverToTransporterAsync(
            driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Temporary, CancellationToken.None));
    }

    [Test]
    public async Task Assign_ToSecondTransporter_IsAllowed()
    {
        using var context = NewContext(nameof(Assign_ToSecondTransporter_IsAllowed));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        var second = new Transporter("Truck 2", 1, accountId);
        context.Transporters.Add(second);
        context.SaveChanges();
        var writer = new DriverAssignmentWriter(context, Principal(accountId));

        await writer.AssignDriverToTransporterAsync(driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None);
        await writer.AssignDriverToTransporterAsync(driver.DriverId, second.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None);

        // Concurrent assignments to DIFFERENT transporters are legal by design.
        Assert.That(context.DriverTransporterAssignments.Count(), Is.EqualTo(2));
    }

    [Test]
    public void Assign_ForeignTransporter_IsNotFound()
    {
        using var context = NewContext(nameof(Assign_ForeignTransporter_IsNotFound));
        var accountId = Guid.NewGuid();
        var (driver, _) = Seed(context, accountId);
        var foreign = new Transporter("Foreign", 1, Guid.NewGuid());
        context.Transporters.Add(foreign);
        context.SaveChanges();
        var writer = new DriverAssignmentWriter(context, Principal(accountId));

        Assert.ThrowsAsync<NotFoundException>(() => writer.AssignDriverToTransporterAsync(
            driver.DriverId, foreign.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None));
    }

    [Test]
    public void Assign_InactiveDriver_FailsValidation()
    {
        using var context = NewContext(nameof(Assign_InactiveDriver_FailsValidation));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId, driverActive: false);
        var writer = new DriverAssignmentWriter(context, Principal(accountId));

        Assert.ThrowsAsync<ValidationException>(() => writer.AssignDriverToTransporterAsync(
            driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None));
    }

    [Test]
    public void Assign_ExpiredLicense_BlockedOnlyWhenAccountOptsIn()
    {
        var accountId = Guid.NewGuid();
        var yesterday = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime).AddDays(-1);

        // Enforcement OFF (the default): an expired license does not stand in the way.
        using (var context = NewContext(nameof(Assign_ExpiredLicense_BlockedOnlyWhenAccountOptsIn) + "-off"))
        {
            var (driver, transporter) = Seed(context, accountId);
            EnableWorkforce(context, accountId, """{"blockAssignmentOnExpiredLicense":false}""");
            context.DriverQualifications.Add(new DriverQualification(accountId, driver.DriverId, DriverQualificationTypes.License,
                null, "L-1", null, yesterday, null, DriverQualificationStatuses.Valid, null, null));
            context.SaveChanges();

            var writer = new DriverAssignmentWriter(context, Principal(accountId));
            Assert.DoesNotThrowAsync(() => writer.AssignDriverToTransporterAsync(
                driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None));
        }

        // Enforcement ON: the same data is rejected with a clear validation error (AC5).
        using (var context = NewContext(nameof(Assign_ExpiredLicense_BlockedOnlyWhenAccountOptsIn) + "-on"))
        {
            var (driver, transporter) = Seed(context, accountId);
            EnableWorkforce(context, accountId, """{"blockAssignmentOnExpiredLicense":true}""");
            context.DriverQualifications.Add(new DriverQualification(accountId, driver.DriverId, DriverQualificationTypes.License,
                null, "L-1", null, yesterday, null, DriverQualificationStatuses.Valid, null, null));
            context.SaveChanges();

            var writer = new DriverAssignmentWriter(context, Principal(accountId));
            Assert.ThrowsAsync<ValidationException>(() => writer.AssignDriverToTransporterAsync(
                driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None));
        }
    }

    // A renewed licence supersedes an expired one. Asking "does an expired licence exist?" instead of
    // "does a valid one exist?" would permanently block every driver who has ever renewed.
    [Test]
    public void Assign_WithRenewedLicense_IsAllowedEvenWhenEnforcementIsOn()
    {
        using var context = NewContext(nameof(Assign_WithRenewedLicense_IsAllowedEvenWhenEnforcementIsOn));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        EnableWorkforce(context, accountId, """{"blockAssignmentOnExpiredLicense":true}""");
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        context.DriverQualifications.AddRange(
            // The superseded record is kept deliberately — §6 is a history model.
            new DriverQualification(accountId, driver.DriverId, DriverQualificationTypes.License, null, "old", null, today.AddYears(-2), null, DriverQualificationStatuses.Valid, null, null),
            new DriverQualification(accountId, driver.DriverId, DriverQualificationTypes.License, null, "current", null, today.AddYears(3), null, DriverQualificationStatuses.Valid, null, null));
        context.SaveChanges();

        var writer = new DriverAssignmentWriter(context, Principal(accountId));
        Assert.DoesNotThrowAsync(() => writer.AssignDriverToTransporterAsync(
            driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None));
    }

    // Enforcement targets drivers whose licence has lapsed, not accounts that have not entered
    // qualifications yet — otherwise enabling workforce would lock out every existing driver.
    [Test]
    public void Assign_WithNoLicenseRecordAtAll_IsAllowedEvenWhenEnforcementIsOn()
    {
        using var context = NewContext(nameof(Assign_WithNoLicenseRecordAtAll_IsAllowedEvenWhenEnforcementIsOn));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        EnableWorkforce(context, accountId, """{"blockAssignmentOnExpiredLicense":true}""");

        var writer = new DriverAssignmentWriter(context, Principal(accountId));
        Assert.DoesNotThrowAsync(() => writer.AssignDriverToTransporterAsync(
            driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None));
    }

    // §6 defines an active assignment as "EndsAt null / future", so scheduling a future close must not
    // retire the assignment — and revoke the driver's transporter/document access — today.
    [Test]
    public async Task End_WithAFutureDate_SchedulesTheCloseWithoutRetiringItNow()
    {
        using var context = NewContext(nameof(End_WithAFutureDate_SchedulesTheCloseWithoutRetiringItNow));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        var writer = new DriverAssignmentWriter(context, Principal(accountId));
        var created = await writer.AssignDriverToTransporterAsync(driver.DriverId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddHours(-1), DriverAssignmentTypes.Regular, CancellationToken.None);

        await writer.EndDriverAssignmentAsync(created.DriverTransporterAssignmentId, DateTimeOffset.UtcNow.AddDays(7), CancellationToken.None);

        var stored = context.DriverTransporterAssignments.Single();
        Assert.Multiple(() =>
        {
            Assert.That(stored.Status, Is.EqualTo(DriverAssignmentStatuses.Active), "A future end date must not retire the assignment today.");
            Assert.That(stored.EndsAt, Is.Not.Null);
        });

        var reader = new DriverReader(context, Principal(accountId));
        Assert.That(await reader.ValidateDriverAssignmentAsync(driver.DriverId, "Transporter", transporter.TransporterId.ToString(), CancellationToken.None),
            Is.True, "The driver keeps access until the scheduled end date passes.");
    }

    [Test]
    public void Assign_MalformedFeatureConfiguration_DoesNotBlock()
    {
        using var context = NewContext(nameof(Assign_MalformedFeatureConfiguration_DoesNotBlock));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        EnableWorkforce(context, accountId, "{not json");
        context.DriverQualifications.Add(new DriverQualification(accountId, driver.DriverId, DriverQualificationTypes.License,
            null, "L-1", null, DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime).AddDays(-1), null, DriverQualificationStatuses.Valid, null, null));
        context.SaveChanges();
        var writer = new DriverAssignmentWriter(context, Principal(accountId));

        // The strict behavior is the opt-in, so unreadable configuration must fall back to "not enforced".
        Assert.DoesNotThrowAsync(() => writer.AssignDriverToTransporterAsync(
            driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None));
    }

    [Test]
    public async Task End_ClosesAssignment_AndIsNotRepeatable()
    {
        using var context = NewContext(nameof(End_ClosesAssignment_AndIsNotRepeatable));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        var writer = new DriverAssignmentWriter(context, Principal(accountId));
        var created = await writer.AssignDriverToTransporterAsync(driver.DriverId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddHours(-2), DriverAssignmentTypes.Regular, CancellationToken.None);

        await writer.EndDriverAssignmentAsync(created.DriverTransporterAssignmentId, null, CancellationToken.None);

        var stored = context.DriverTransporterAssignments.Single();
        Assert.Multiple(() =>
        {
            Assert.That(stored.Status, Is.EqualTo(DriverAssignmentStatuses.Ended));
            Assert.That(stored.EndsAt, Is.Not.Null);
        });

        // Ended assignments are immutable (AC3).
        Assert.ThrowsAsync<ConflictException>(() =>
            writer.EndDriverAssignmentAsync(created.DriverTransporterAssignmentId, null, CancellationToken.None));
    }

    [Test]
    public async Task End_BeforeStart_FailsValidation()
    {
        using var context = NewContext(nameof(End_BeforeStart_FailsValidation));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        var writer = new DriverAssignmentWriter(context, Principal(accountId));
        var created = await writer.AssignDriverToTransporterAsync(driver.DriverId, transporter.TransporterId,
            DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None);

        Assert.ThrowsAsync<ValidationException>(() => writer.EndDriverAssignmentAsync(
            created.DriverTransporterAssignmentId, DateTimeOffset.UtcNow.AddDays(-5), CancellationToken.None));
    }

    [Test]
    public async Task Assign_AfterPreviousEnded_IsAllowed()
    {
        using var context = NewContext(nameof(Assign_AfterPreviousEnded_IsAllowed));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        var writer = new DriverAssignmentWriter(context, Principal(accountId));
        var first = await writer.AssignDriverToTransporterAsync(driver.DriverId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddHours(-4), DriverAssignmentTypes.Regular, CancellationToken.None);
        var endedAt = DateTimeOffset.UtcNow.AddHours(-2);
        await writer.EndDriverAssignmentAsync(first.DriverTransporterAssignmentId, endedAt, CancellationToken.None);

        // The pair is free again once the previous assignment closed — history accumulates.
        await writer.AssignDriverToTransporterAsync(driver.DriverId, transporter.TransporterId,
            endedAt.AddMinutes(1), DriverAssignmentTypes.Temporary, CancellationToken.None);

        Assert.That(context.DriverTransporterAssignments.Count(), Is.EqualTo(2));
    }

    // Regression: the overlap check originally compared the existing EndsAt against *now* and only
    // looked at Active rows, so a BACKDATED assignment could silently overlap a closed one for the
    // same pair — corrupting the history that hour-producing modules will later join against (§18.7).
    [Test]
    public async Task Assign_BackdatedIntoAClosedAssignment_Conflicts()
    {
        using var context = NewContext(nameof(Assign_BackdatedIntoAClosedAssignment_Conflicts));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        var writer = new DriverAssignmentWriter(context, Principal(accountId));

        // A ran from 30 days ago to 20 days ago and is closed.
        var first = await writer.AssignDriverToTransporterAsync(driver.DriverId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddDays(-30), DriverAssignmentTypes.Regular, CancellationToken.None);
        await writer.EndDriverAssignmentAsync(first.DriverTransporterAssignmentId, DateTimeOffset.UtcNow.AddDays(-20), CancellationToken.None);

        // B backdated to 25 days ago would cover 25→20 days ago twice.
        Assert.ThrowsAsync<ConflictException>(() => writer.AssignDriverToTransporterAsync(
            driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow.AddDays(-25), DriverAssignmentTypes.Regular, CancellationToken.None));

        // Starting exactly where the previous one ended is contiguous, not overlapping — allowed.
        Assert.DoesNotThrowAsync(() => writer.AssignDriverToTransporterAsync(
            driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow.AddDays(-20), DriverAssignmentTypes.Regular, CancellationToken.None));
    }

    [Test]
    public async Task Assign_CancelledAssignmentDoesNotBlock()
    {
        using var context = NewContext(nameof(Assign_CancelledAssignmentDoesNotBlock));
        var accountId = Guid.NewGuid();
        var (driver, transporter) = Seed(context, accountId);
        // A cancelled assignment never happened, so it must not reserve the pair's timeline.
        context.DriverTransporterAssignments.Add(new DriverTransporterAssignment(accountId, driver.DriverId,
            transporter.TransporterId, DateTimeOffset.UtcNow.AddDays(-1), null, DriverAssignmentTypes.Regular,
            DriverAssignmentStatuses.Cancelled, "User:test"));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new DriverAssignmentWriter(context, Principal(accountId));
        Assert.DoesNotThrowAsync(() => writer.AssignDriverToTransporterAsync(
            driver.DriverId, transporter.TransporterId, DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular, CancellationToken.None));
    }
}
