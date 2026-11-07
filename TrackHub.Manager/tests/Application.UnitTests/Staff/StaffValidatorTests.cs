using Common.Domain.Constants;
using TrackHub.Manager.Application.Staff.Commands;
using TrackHub.Manager.Application.Staff.Queries;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Staff;

[TestFixture]
public class StaffValidatorTests
{
    private static StaffMemberDto ValidStaff(string name = "Ana Perez", string? phone = "+57 300 1234567", DateOnly? licenseExpiresAt = null, string staffType = StaffTypes.Driver)
        => new(Guid.NewGuid(), name, phone, "CC", "123456", true, "EMP-1", "LIC-1", licenseExpiresAt, null, staffType);

    private static StaffQualificationDto ValidQualification(DateOnly? issuedAt = null, DateOnly? expiresAt = null, string? type = null, string? status = null)
        => new(Guid.NewGuid(), Guid.NewGuid(), type ?? StaffQualificationTypes.License, "C2", "LIC-1",
            issuedAt, expiresAt, "RUNT", status ?? StaffQualificationStatuses.Valid, null, null);

    // Before spec 09 these commands had NO validators — an empty name reached the writer (§2.2.4).
    [Test]
    public void CreateStaffMember_RejectsUnknownStaffType()
    {
        var validator = new CreateStaffMemberCommandValidator();
        Assert.That(validator.Validate(new CreateStaffMemberCommand(ValidStaff(staffType: "Pilot"))).IsValid, Is.False);
        Assert.That(validator.Validate(new CreateStaffMemberCommand(ValidStaff(staffType: StaffTypes.Mechanic))).IsValid, Is.True);
    }

    [Test]
    public void CreateStaffMember_RejectsEmptyName()
    {
        var result = new CreateStaffMemberCommandValidator().Validate(new CreateStaffMemberCommand(ValidStaff(name: "")));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void CreateStaffMember_RejectsMalformedPhone_ButAllowsBlank()
    {
        var validator = new CreateStaffMemberCommandValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new CreateStaffMemberCommand(ValidStaff(phone: "not-a-phone"))).IsValid, Is.False);
            // Phone is optional — absent must stay valid.
            Assert.That(validator.Validate(new CreateStaffMemberCommand(ValidStaff(phone: null))).IsValid, Is.True);
            Assert.That(validator.Validate(new CreateStaffMemberCommand(ValidStaff())).IsValid, Is.True);
        });
    }

    [Test]
    public async Task CreateStaffMember_RejectsPastLicenseExpiry_ButUpdateAllowsIt()
    {
        var past = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime).AddDays(-1);

        // Onboarding on an already-expired license is a data-entry error... (the rule asks the account
        // calendar, so it is asynchronous; without a principal it judges on the UTC day)
        Assert.That((await new CreateStaffMemberCommandValidator().ValidateAsync(new CreateStaffMemberCommand(ValidStaff(licenseExpiresAt: past)))).IsValid, Is.False);
        // ...but an existing record must remain editable while its license lapses.
        Assert.That(new UpdateStaffMemberCommandValidator().Validate(new UpdateStaffMemberCommand(Guid.NewGuid(), ValidStaff(licenseExpiresAt: past))).IsValid, Is.True);
    }

    [Test]
    public void UpdateStaffMember_RequiresStaffId()
    {
        var result = new UpdateStaffMemberCommandValidator().Validate(new UpdateStaffMemberCommand(Guid.Empty, ValidStaff()));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void DeactivateStaffMember_RequiresStaffId()
    {
        Assert.That(new DeactivateStaffMemberCommandValidator().Validate(new DeactivateStaffMemberCommand(Guid.Empty)).IsValid, Is.False);
        Assert.That(new DeactivateStaffMemberCommandValidator().Validate(new DeactivateStaffMemberCommand(Guid.NewGuid())).IsValid, Is.True);
    }

    [Test]
    public void CreateQualification_RejectsUnknownTypeAndStatus()
    {
        var validator = new CreateStaffQualificationCommandValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new CreateStaffQualificationCommand(ValidQualification(type: "Wizardry"))).IsValid, Is.False);
            Assert.That(validator.Validate(new CreateStaffQualificationCommand(ValidQualification(status: "Pending"))).IsValid, Is.False);
            Assert.That(validator.Validate(new CreateStaffQualificationCommand(ValidQualification())).IsValid, Is.True);
        });
    }

    [Test]
    public void CreateQualification_RejectsExpiryBeforeIssue()
    {
        var validator = new CreateStaffQualificationCommandValidator();
        var inverted = ValidQualification(issuedAt: new DateOnly(2027, 1, 1), expiresAt: new DateOnly(2026, 1, 1));

        Assert.That(validator.Validate(new CreateStaffQualificationCommand(inverted)).IsValid, Is.False);
    }

    [Test]
    public void AssignStaff_RejectsUnknownAssignmentType()
    {
        var validator = new AssignStaffToTransporterCommandValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new AssignStaffToTransporterCommand(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "Permanent")).IsValid, Is.False);
            Assert.That(validator.Validate(new AssignStaffToTransporterCommand(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, StaffAssignmentTypes.Temporary)).IsValid, Is.True);
            Assert.That(validator.Validate(new AssignStaffToTransporterCommand(Guid.Empty, Guid.NewGuid(), DateTimeOffset.UtcNow, StaffAssignmentTypes.Regular)).IsValid, Is.False);
        });
    }

    [Test]
    public void AssignmentHistoryQuery_RejectsInvertedWindow()
    {
        var validator = new GetStaffAssignmentHistoryQueryValidator();
        var accountId = Guid.NewGuid();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new GetStaffAssignmentHistoryQuery(accountId, From: DateTimeOffset.UtcNow, To: DateTimeOffset.UtcNow.AddDays(-1))).IsValid, Is.False);
            Assert.That(validator.Validate(new GetStaffAssignmentHistoryQuery(accountId)).IsValid, Is.True);
        });
    }

    [Test]
    public void QualificationsQuery_RejectsNegativeWindowAndEmptyAccount()
    {
        var validator = new GetStaffQualificationsQueryValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new GetStaffQualificationsQuery(Guid.NewGuid(), ExpiringWithinDays: -1)).IsValid, Is.False);
            Assert.That(validator.Validate(new GetStaffQualificationsQuery(Guid.Empty)).IsValid, Is.False);
            Assert.That(validator.Validate(new GetStaffQualificationsQuery(Guid.NewGuid(), ExpiringWithinDays: 30)).IsValid, Is.True);
        });
    }
}
