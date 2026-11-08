using Common.Application.Interfaces;
using Moq;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class StaffWorkforceReaderTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

    private static ICurrentPrincipal Principal(Guid accountId, PrincipalType type = PrincipalType.User, Guid? staffId = null)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(x => x.AccountId).Returns(accountId);
        principal.SetupGet(x => x.PrincipalType).Returns(type);
        principal.SetupGet(x => x.UserId).Returns(type == PrincipalType.User ? Guid.NewGuid() : null);
        principal.SetupGet(x => x.StaffId).Returns(staffId);
        return principal.Object;
    }

    private static StaffTransporterAssignment ActiveAssignment(Guid accountId, Guid staffId, Guid transporterId)
        => new(accountId, staffId, transporterId, DateTimeOffset.UtcNow.AddHours(-1), null,
            StaffAssignmentTypes.Regular, StaffAssignmentStatuses.Active, "User:test");

    [Test]
    public async Task ValidateStaffAssignment_TrueForAssignmentRow()
    {
        using var context = NewContext(nameof(ValidateStaffAssignment_TrueForAssignmentRow));
        var accountId = Guid.NewGuid();
        var transporterId = Guid.NewGuid();
        // No DefaultTransporterId at all — only the assignment row can satisfy this (AC4).
        var staff = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, null);
        context.StaffMembers.Add(staff);
        context.StaffTransporterAssignments.Add(ActiveAssignment(accountId, staff.StaffId, transporterId));
        context.SaveChanges();

        var reader = new StaffMemberReader(context, Principal(accountId));
        var ok = await reader.ValidateStaffAssignmentAsync(staff.StaffId, "Transporter", transporterId.ToString(), CancellationToken.None);

        Assert.That(ok, Is.True);
    }

    [Test]
    public async Task ValidateStaffAssignment_TrueForDefaultTransporter_Unchanged()
    {
        using var context = NewContext(nameof(ValidateStaffAssignment_TrueForDefaultTransporter_Unchanged));
        var accountId = Guid.NewGuid();
        var transporter = new Transporter("Truck 1", 1, accountId);
        context.Transporters.Add(transporter);
        var staff = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, transporter.TransporterId);
        context.StaffMembers.Add(staff);
        context.SaveChanges();

        var reader = new StaffMemberReader(context, Principal(accountId));
        var ok = await reader.ValidateStaffAssignmentAsync(staff.StaffId, "Transporter", transporter.TransporterId.ToString(), CancellationToken.None);

        // The legacy path DocumentAccessPolicy relies on must keep working untouched (AC4).
        Assert.That(ok, Is.True);
    }

    // Spec §5: a cross-account transporter reference must never validate. Writers now reject one, but
    // rows written before that check existed would otherwise let a staff reach another tenant's
    // documents through DocumentAccessPolicy.
    [Test]
    public async Task ValidateStaffAssignment_FalseForCrossAccountDefaultTransporter()
    {
        using var context = NewContext(nameof(ValidateStaffAssignment_FalseForCrossAccountDefaultTransporter));
        var accountId = Guid.NewGuid();
        var foreignTransporter = new Transporter("Foreign", 1, Guid.NewGuid());
        context.Transporters.Add(foreignTransporter);
        var staff = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, foreignTransporter.TransporterId);
        context.StaffMembers.Add(staff);
        context.SaveChanges();

        var reader = new StaffMemberReader(context, Principal(accountId));
        var ok = await reader.ValidateStaffAssignmentAsync(staff.StaffId, "Transporter", foreignTransporter.TransporterId.ToString(), CancellationToken.None);

        Assert.That(ok, Is.False);
    }

    [Test]
    public async Task ValidateStaffAssignment_FalseForUnrelatedAndEndedAndInactive()
    {
        using var context = NewContext(nameof(ValidateStaffAssignment_FalseForUnrelatedAndEndedAndInactive));
        var accountId = Guid.NewGuid();
        var endedTransporterId = Guid.NewGuid();
        var staff = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, null);
        var inactive = new StaffMember(accountId, "Beto", null, null, null, false, null, null, null, Guid.NewGuid());
        context.StaffMembers.AddRange(staff, inactive);
        context.StaffTransporterAssignments.Add(new StaffTransporterAssignment(accountId, staff.StaffId, endedTransporterId,
            DateTimeOffset.UtcNow.AddDays(-5), DateTimeOffset.UtcNow.AddDays(-1), StaffAssignmentTypes.Regular,
            StaffAssignmentStatuses.Ended, "User:test"));
        context.SaveChanges();

        var reader = new StaffMemberReader(context, Principal(accountId));

        Assert.Multiple(async () =>
        {
            Assert.That(await reader.ValidateStaffAssignmentAsync(staff.StaffId, "Transporter", Guid.NewGuid().ToString(), CancellationToken.None), Is.False);
            // An ENDED assignment no longer grants access.
            Assert.That(await reader.ValidateStaffAssignmentAsync(staff.StaffId, "Transporter", endedTransporterId.ToString(), CancellationToken.None), Is.False);
            // An inactive staff is assigned to nothing, default transporter notwithstanding.
            Assert.That(await reader.ValidateStaffAssignmentAsync(inactive.StaffId, "Transporter", inactive.DefaultTransporterId!.Value.ToString(), CancellationToken.None), Is.False);
            // Resource types whose modules have not registered stay unknown.
            Assert.That(await reader.ValidateStaffAssignmentAsync(staff.StaffId, "Trip", Guid.NewGuid().ToString(), CancellationToken.None), Is.False);
        });
    }

    [Test]
    public async Task GetStaffAssignments_ReturnsRows_AndDoesNotDuplicateTheDefault()
    {
        using var context = NewContext(nameof(GetStaffAssignments_ReturnsRows_AndDoesNotDuplicateTheDefault));
        var accountId = Guid.NewGuid();
        var defaultTransporterId = Guid.NewGuid();
        var otherTransporterId = Guid.NewGuid();
        var staff = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, defaultTransporterId);
        context.StaffMembers.Add(staff);
        // One assignment covering the default transporter, one covering another.
        context.StaffTransporterAssignments.AddRange(
            ActiveAssignment(accountId, staff.StaffId, defaultTransporterId),
            ActiveAssignment(accountId, staff.StaffId, otherTransporterId));
        context.SaveChanges();

        var reader = new StaffMemberReader(context, Principal(accountId));
        var assignments = await reader.GetStaffAssignmentsAsync(staff.StaffId, CancellationToken.None);

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
    public async Task GetStaffAssignments_SynthesizesDefault_WhenNoRowCoversIt()
    {
        using var context = NewContext(nameof(GetStaffAssignments_SynthesizesDefault_WhenNoRowCoversIt));
        var accountId = Guid.NewGuid();
        var defaultTransporterId = Guid.NewGuid();
        var staff = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, defaultTransporterId);
        context.StaffMembers.Add(staff);
        context.SaveChanges();

        var reader = new StaffMemberReader(context, Principal(accountId));
        var assignments = await reader.GetStaffAssignmentsAsync(staff.StaffId, CancellationToken.None);

        var only = assignments.Single();
        Assert.Multiple(() =>
        {
            Assert.That(only.ResourceId, Is.EqualTo(defaultTransporterId.ToString()));
            Assert.That(only.StartsAt, Is.Null);
            Assert.That(only.AssignmentType, Is.Null);
        });
    }

    [Test]
    public async Task GetStaffQualifications_FiltersByExpiryWindow_AndOrdersSoonestFirst()
    {
        using var context = NewContext(nameof(GetStaffQualifications_FiltersByExpiryWindow_AndOrdersSoonestFirst));
        var accountId = Guid.NewGuid();
        var staff = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, null);
        context.StaffMembers.Add(staff);
        var today = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime);
        context.StaffQualifications.AddRange(
            new StaffQualification(accountId, staff.StaffId, StaffQualificationTypes.License, null, "soon", null, today.AddDays(5), null, StaffQualificationStatuses.Valid, null, null),
            new StaffQualification(accountId, staff.StaffId, StaffQualificationTypes.MedicalExam, null, "pastdue", null, today.AddDays(-3), null, StaffQualificationStatuses.Valid, null, null),
            new StaffQualification(accountId, staff.StaffId, StaffQualificationTypes.Training, null, "far", null, today.AddDays(200), null, StaffQualificationStatuses.Valid, null, null),
            new StaffQualification(accountId, staff.StaffId, StaffQualificationTypes.Other, null, "undated", null, null, null, StaffQualificationStatuses.Valid, null, null));
        context.SaveChanges();

        var reader = new StaffQualificationReader(context, Principal(accountId));
        var expiring = (await reader.GetStaffQualificationsAsync(accountId, null, 30, 0, 50, CancellationToken.None)).Items;

        Assert.Multiple(() =>
        {
            // Past-due rows always qualify; the far-future and undated ones do not.
            Assert.That(expiring.Select(x => x.Number), Is.EqualTo(new[] { "pastdue", "soon" }));
            Assert.That(expiring.First().StaffName, Is.EqualTo("Ana"));
        });

        var all = (await reader.GetStaffQualificationsAsync(accountId, null, null, 0, 50, CancellationToken.None)).Items;
        // Unfiltered: soonest expiry first, undated last.
        Assert.That(all.Select(x => x.Number), Is.EqualTo(new[] { "pastdue", "soon", "far", "undated" }));
    }

    [Test]
    public async Task GetStaffQualifications_ForAnotherAccount_IsForbidden()
    {
        using var context = NewContext(nameof(GetStaffQualifications_ForAnotherAccount_IsForbidden));
        var reader = new StaffQualificationReader(context, Principal(Guid.NewGuid()));

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            reader.GetStaffQualificationsAsync(Guid.NewGuid(), null, null, 0, 50, CancellationToken.None));
    }

    [Test]
    public async Task GetStaffAssignmentHistory_FiltersByWindowAndTransporter()
    {
        using var context = NewContext(nameof(GetStaffAssignmentHistory_FiltersByWindowAndTransporter));
        var accountId = Guid.NewGuid();
        var staff = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, null);
        var truckA = new Transporter("A", 1, accountId);
        var truckB = new Transporter("B", 1, accountId);
        context.StaffMembers.Add(staff);
        context.Transporters.AddRange(truckA, truckB);
        context.StaffTransporterAssignments.AddRange(
            new StaffTransporterAssignment(accountId, staff.StaffId, truckA.TransporterId, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow.AddDays(-20), StaffAssignmentTypes.Regular, StaffAssignmentStatuses.Ended, "User:test"),
            new StaffTransporterAssignment(accountId, staff.StaffId, truckB.TransporterId, DateTimeOffset.UtcNow.AddDays(-5), null, StaffAssignmentTypes.Temporary, StaffAssignmentStatuses.Active, "User:test"));
        context.SaveChanges();

        var reader = new StaffAssignmentReader(context, Principal(accountId));

        var recent = (await reader.GetStaffAssignmentHistoryAsync(accountId, null, null, DateTimeOffset.UtcNow.AddDays(-10), null, 0, 50, CancellationToken.None)).Items;
        // The 30→20-days-ago assignment closed before the window opened, so it is out.
        Assert.That(recent.Select(x => x.TransporterName), Is.EqualTo(new[] { "B" }));

        var byTransporter = (await reader.GetStaffAssignmentHistoryAsync(accountId, null, truckA.TransporterId, null, null, 0, 50, CancellationToken.None)).Items;
        Assert.That(byTransporter.Select(x => x.TransporterName), Is.EqualTo(new[] { "A" }));
    }

    [Test]
    public async Task GetMyStaffProfile_ReturnsOnlyOwnData()
    {
        using var context = NewContext(nameof(GetMyStaffProfile_ReturnsOnlyOwnData));
        var accountId = Guid.NewGuid();
        var me = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, null);
        var other = new StaffMember(accountId, "Beto", null, null, null, true, null, null, null, null);
        var truck = new Transporter("A", 1, accountId);
        context.StaffMembers.AddRange(me, other);
        context.Transporters.Add(truck);
        context.StaffQualifications.AddRange(
            new StaffQualification(accountId, me.StaffId, StaffQualificationTypes.License, null, "mine", null, null, null, StaffQualificationStatuses.Valid, null, null),
            new StaffQualification(accountId, other.StaffId, StaffQualificationTypes.License, null, "theirs", null, null, null, StaffQualificationStatuses.Valid, null, null));
        context.StaffTransporterAssignments.AddRange(
            ActiveAssignment(accountId, me.StaffId, truck.TransporterId),
            ActiveAssignment(accountId, other.StaffId, truck.TransporterId));
        context.SaveChanges();

        var reader = new StaffMemberReader(context, Principal(accountId, PrincipalType.Staff, me.StaffId));
        var profile = await reader.GetMyStaffProfileAsync(me.StaffId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(profile.Staff.Name, Is.EqualTo("Ana"));
            // Another staff's qualifications and assignments must never leak in (AC2).
            Assert.That(profile.Qualifications.Select(x => x.Number), Is.EqualTo(new[] { "mine" }));
            Assert.That(profile.ActiveAssignments, Has.Count.EqualTo(1));
            Assert.That(profile.ActiveAssignments.Single().StaffId, Is.EqualTo(me.StaffId));
        });
    }
}
