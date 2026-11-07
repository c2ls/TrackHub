using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;
using CommonConstants = Common.Domain.Constants;

namespace Infrastructure.UnitTests;

[TestFixture]
public class StaffAssignmentWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.AccountId).Returns(accountId);
        principal.SetupGet(x => x.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        return principal.Object;
    }

    private static (StaffMember Staff, Transporter Transporter) Seed(ApplicationDbContext context, Guid accountId, bool staffActive = true)
    {
        var staff = new StaffMember(accountId, "Ana", null, null, null, staffActive, null, null, null, null);
        var transporter = new Transporter("Truck 1", 1, accountId);
        context.StaffMembers.Add(staff);
        context.Transporters.Add(transporter);
        context.SaveChanges();
        return (staff, transporter);
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
        var (staff, transporter) = Seed(context, accountId);
        var writer = new StaffAssignmentWriter(context, Principal(accountId));

        var vm = await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddMinutes(-1), StaffAssignmentTypes.Regular, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Status, Is.EqualTo(StaffAssignmentStatuses.Active));
            Assert.That(vm.TransporterName, Is.EqualTo("Truck 1"));
            Assert.That(vm.EndsAt, Is.Null);
            Assert.That(context.AuditEvents.Count(x => x.Action == "AssignStaffToTransporter"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task Assign_SamePairTwice_Conflicts()
    {
        using var context = NewContext(nameof(Assign_SamePairTwice_Conflicts));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        var writer = new StaffAssignmentWriter(context, Principal(accountId));
        await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None);

        // One OPEN assignment per (staff, transporter) pair → 409 (AC3).
        await Assert.ThrowsAsync<ConflictException>(() => writer.AssignStaffToTransporterAsync(
            staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Temporary, CancellationToken.None));
    }

    [Test]
    public async Task Assign_ToSecondTransporter_IsAllowed()
    {
        using var context = NewContext(nameof(Assign_ToSecondTransporter_IsAllowed));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        var second = new Transporter("Truck 2", 1, accountId);
        context.Transporters.Add(second);
        context.SaveChanges();
        var writer = new StaffAssignmentWriter(context, Principal(accountId));

        await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None);
        await writer.AssignStaffToTransporterAsync(staff.StaffId, second.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None);

        // Concurrent assignments to DIFFERENT transporters are legal by design.
        Assert.That(context.StaffTransporterAssignments.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Assign_ForeignTransporter_IsNotFound()
    {
        using var context = NewContext(nameof(Assign_ForeignTransporter_IsNotFound));
        var accountId = Guid.NewGuid();
        var (staff, _) = Seed(context, accountId);
        var foreign = new Transporter("Foreign", 1, Guid.NewGuid());
        context.Transporters.Add(foreign);
        context.SaveChanges();
        var writer = new StaffAssignmentWriter(context, Principal(accountId));

        await Assert.ThrowsAsync<NotFoundException>(() => writer.AssignStaffToTransporterAsync(
            staff.StaffId, foreign.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None));
    }

    [Test]
    public async Task Assign_InactiveStaff_FailsValidation()
    {
        using var context = NewContext(nameof(Assign_InactiveStaff_FailsValidation));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId, staffActive: false);
        var writer = new StaffAssignmentWriter(context, Principal(accountId));

        await Assert.ThrowsAsync<ValidationException>(() => writer.AssignStaffToTransporterAsync(
            staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None));
    }

    [Test]
    public async Task Assign_ExpiredLicense_BlockedOnlyWhenAccountOptsIn()
    {
        var accountId = Guid.NewGuid();
        var yesterday = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime).AddDays(-1);

        // Enforcement OFF (the default): an expired license does not stand in the way.
        using (var context = NewContext(nameof(Assign_ExpiredLicense_BlockedOnlyWhenAccountOptsIn) + "-off"))
        {
            var (staff, transporter) = Seed(context, accountId);
            EnableWorkforce(context, accountId, """{"blockAssignmentOnExpiredLicense":false}""");
            context.StaffQualifications.Add(new StaffQualification(accountId, staff.StaffId, StaffQualificationTypes.License,
                null, "L-1", null, yesterday, null, StaffQualificationStatuses.Valid, null, null));
            context.SaveChanges();

            var writer = new StaffAssignmentWriter(context, Principal(accountId));
            await Assert.DoesNotThrowAsync(() => writer.AssignStaffToTransporterAsync(
                staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None));
        }

        // Enforcement ON: the same data is rejected with a clear validation error (AC5).
        using (var context = NewContext(nameof(Assign_ExpiredLicense_BlockedOnlyWhenAccountOptsIn) + "-on"))
        {
            var (staff, transporter) = Seed(context, accountId);
            EnableWorkforce(context, accountId, """{"blockAssignmentOnExpiredLicense":true}""");
            context.StaffQualifications.Add(new StaffQualification(accountId, staff.StaffId, StaffQualificationTypes.License,
                null, "L-1", null, yesterday, null, StaffQualificationStatuses.Valid, null, null));
            context.SaveChanges();

            var writer = new StaffAssignmentWriter(context, Principal(accountId));
            await Assert.ThrowsAsync<ValidationException>(() => writer.AssignStaffToTransporterAsync(
                staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None));
        }
    }

    // A renewed licence supersedes an expired one. Asking "does an expired licence exist?" instead of
    // "does a valid one exist?" would permanently block every staff who has ever renewed.
    [Test]
    public async Task Assign_WithRenewedLicense_IsAllowedEvenWhenEnforcementIsOn()
    {
        using var context = NewContext(nameof(Assign_WithRenewedLicense_IsAllowedEvenWhenEnforcementIsOn));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        EnableWorkforce(context, accountId, """{"blockAssignmentOnExpiredLicense":true}""");
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        context.StaffQualifications.AddRange(
            // The superseded record is kept deliberately — §6 is a history model.
            new StaffQualification(accountId, staff.StaffId, StaffQualificationTypes.License, null, "old", null, today.AddYears(-2), null, StaffQualificationStatuses.Valid, null, null),
            new StaffQualification(accountId, staff.StaffId, StaffQualificationTypes.License, null, "current", null, today.AddYears(3), null, StaffQualificationStatuses.Valid, null, null));
        context.SaveChanges();

        var writer = new StaffAssignmentWriter(context, Principal(accountId));
        await Assert.DoesNotThrowAsync(() => writer.AssignStaffToTransporterAsync(
            staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None));
    }

    // Enforcement targets staff whose licence has lapsed, not accounts that have not entered
    // qualifications yet — otherwise enabling workforce would lock out every existing staff.
    [Test]
    public async Task Assign_WithNoLicenseRecordAtAll_IsAllowedEvenWhenEnforcementIsOn()
    {
        using var context = NewContext(nameof(Assign_WithNoLicenseRecordAtAll_IsAllowedEvenWhenEnforcementIsOn));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        EnableWorkforce(context, accountId, """{"blockAssignmentOnExpiredLicense":true}""");

        var writer = new StaffAssignmentWriter(context, Principal(accountId));
        await Assert.DoesNotThrowAsync(() => writer.AssignStaffToTransporterAsync(
            staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None));
    }

    // §6 defines an active assignment as "EndsAt null / future", so scheduling a future close must not
    // retire the assignment — and revoke the staff's transporter/document access — today.
    [Test]
    public async Task End_WithAFutureDate_SchedulesTheCloseWithoutRetiringItNow()
    {
        using var context = NewContext(nameof(End_WithAFutureDate_SchedulesTheCloseWithoutRetiringItNow));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        var writer = new StaffAssignmentWriter(context, Principal(accountId));
        var created = await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddHours(-1), StaffAssignmentTypes.Regular, CancellationToken.None);

        await writer.EndStaffAssignmentAsync(created.StaffTransporterAssignmentId, DateTimeOffset.UtcNow.AddDays(7), CancellationToken.None);

        var stored = context.StaffTransporterAssignments.Single();
        Assert.Multiple(() =>
        {
            Assert.That(stored.Status, Is.EqualTo(StaffAssignmentStatuses.Active), "A future end date must not retire the assignment today.");
            Assert.That(stored.EndsAt, Is.Not.Null);
        });

        var reader = new StaffMemberReader(context, Principal(accountId));
        Assert.That(await reader.ValidateStaffAssignmentAsync(staff.StaffId, "Transporter", transporter.TransporterId.ToString(), CancellationToken.None),
            Is.True, "The staff keeps access until the scheduled end date passes.");
    }

    [Test]
    public async Task Assign_MalformedFeatureConfiguration_DoesNotBlock()
    {
        using var context = NewContext(nameof(Assign_MalformedFeatureConfiguration_DoesNotBlock));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        EnableWorkforce(context, accountId, "{not json");
        context.StaffQualifications.Add(new StaffQualification(accountId, staff.StaffId, StaffQualificationTypes.License,
            null, "L-1", null, DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime).AddDays(-1), null, StaffQualificationStatuses.Valid, null, null));
        context.SaveChanges();
        var writer = new StaffAssignmentWriter(context, Principal(accountId));

        // The strict behavior is the opt-in, so unreadable configuration must fall back to "not enforced".
        await Assert.DoesNotThrowAsync(() => writer.AssignStaffToTransporterAsync(
            staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None));
    }

    [Test]
    public async Task End_ClosesAssignment_AndIsNotRepeatable()
    {
        using var context = NewContext(nameof(End_ClosesAssignment_AndIsNotRepeatable));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        var writer = new StaffAssignmentWriter(context, Principal(accountId));
        var created = await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddHours(-2), StaffAssignmentTypes.Regular, CancellationToken.None);

        await writer.EndStaffAssignmentAsync(created.StaffTransporterAssignmentId, null, CancellationToken.None);

        var stored = context.StaffTransporterAssignments.Single();
        Assert.Multiple(() =>
        {
            Assert.That(stored.Status, Is.EqualTo(StaffAssignmentStatuses.Ended));
            Assert.That(stored.EndsAt, Is.Not.Null);
        });

        // Ended assignments are immutable (AC3).
        await Assert.ThrowsAsync<ConflictException>(() =>
            writer.EndStaffAssignmentAsync(created.StaffTransporterAssignmentId, null, CancellationToken.None));
    }

    [Test]
    public async Task End_AfterAScheduledEndHasPassed_IsRefused()
    {
        using var context = NewContext(nameof(End_AfterAScheduledEndHasPassed_IsRefused));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        var writer = new StaffAssignmentWriter(context, Principal(accountId));
        var created = await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddDays(-3), StaffAssignmentTypes.Regular, CancellationToken.None);
        await writer.EndStaffAssignmentAsync(created.StaffTransporterAssignmentId, DateTimeOffset.UtcNow.AddDays(1), CancellationToken.None);
        context.ChangeTracker.Clear();
        var lapsed = context.StaffTransporterAssignments.AsTracking().Single();
        lapsed.EndsAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await Assert.ThrowsAsync<ConflictException>(() =>
            writer.EndStaffAssignmentAsync(created.StaffTransporterAssignmentId, DateTimeOffset.UtcNow.AddDays(5), CancellationToken.None));
    }

    [Test]
    public async Task End_BeforeStart_FailsValidation()
    {
        using var context = NewContext(nameof(End_BeforeStart_FailsValidation));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        var writer = new StaffAssignmentWriter(context, Principal(accountId));
        var created = await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId,
            DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None);

        await Assert.ThrowsAsync<ValidationException>(() => writer.EndStaffAssignmentAsync(
            created.StaffTransporterAssignmentId, DateTimeOffset.UtcNow.AddDays(-5), CancellationToken.None));
    }

    [Test]
    public async Task Assign_AfterPreviousEnded_IsAllowed()
    {
        using var context = NewContext(nameof(Assign_AfterPreviousEnded_IsAllowed));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        var writer = new StaffAssignmentWriter(context, Principal(accountId));
        var first = await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddHours(-4), StaffAssignmentTypes.Regular, CancellationToken.None);
        var endedAt = DateTimeOffset.UtcNow.AddHours(-2);
        await writer.EndStaffAssignmentAsync(first.StaffTransporterAssignmentId, endedAt, CancellationToken.None);

        // The pair is free again once the previous assignment closed — history accumulates.
        await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId,
            endedAt.AddMinutes(1), StaffAssignmentTypes.Temporary, CancellationToken.None);

        Assert.That(context.StaffTransporterAssignments.Count(), Is.EqualTo(2));
    }

    // Regression: the overlap check originally compared the existing EndsAt against *now* and only
    // looked at Active rows, so a BACKDATED assignment could silently overlap a closed one for the
    // same pair — corrupting the history that hour-producing modules will later join against (§18.7).
    [Test]
    public async Task Assign_BackdatedIntoAClosedAssignment_Conflicts()
    {
        using var context = NewContext(nameof(Assign_BackdatedIntoAClosedAssignment_Conflicts));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        var writer = new StaffAssignmentWriter(context, Principal(accountId));

        // A ran from 30 days ago to 20 days ago and is closed.
        var first = await writer.AssignStaffToTransporterAsync(staff.StaffId, transporter.TransporterId,
            DateTimeOffset.UtcNow.AddDays(-30), StaffAssignmentTypes.Regular, CancellationToken.None);
        await writer.EndStaffAssignmentAsync(first.StaffTransporterAssignmentId, DateTimeOffset.UtcNow.AddDays(-20), CancellationToken.None);

        // B backdated to 25 days ago would cover 25→20 days ago twice.
        await Assert.ThrowsAsync<ConflictException>(() => writer.AssignStaffToTransporterAsync(
            staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow.AddDays(-25), StaffAssignmentTypes.Regular, CancellationToken.None));

        // Starting exactly where the previous one ended is contiguous, not overlapping — allowed.
        await Assert.DoesNotThrowAsync(() => writer.AssignStaffToTransporterAsync(
            staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow.AddDays(-20), StaffAssignmentTypes.Regular, CancellationToken.None));
    }

    [Test]
    public async Task Assign_CancelledAssignmentDoesNotBlock()
    {
        using var context = NewContext(nameof(Assign_CancelledAssignmentDoesNotBlock));
        var accountId = Guid.NewGuid();
        var (staff, transporter) = Seed(context, accountId);
        // A cancelled assignment never happened, so it must not reserve the pair's timeline.
        context.StaffTransporterAssignments.Add(new StaffTransporterAssignment(accountId, staff.StaffId,
            transporter.TransporterId, DateTimeOffset.UtcNow.AddDays(-1), null, StaffAssignmentTypes.Regular,
            StaffAssignmentStatuses.Cancelled, "User:test"));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new StaffAssignmentWriter(context, Principal(accountId));
        await Assert.DoesNotThrowAsync(() => writer.AssignStaffToTransporterAsync(
            staff.StaffId, transporter.TransporterId, DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular, CancellationToken.None));
    }
}
