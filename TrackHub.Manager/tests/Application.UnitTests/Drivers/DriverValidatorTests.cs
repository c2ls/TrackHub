using TrackHub.Manager.Application.Drivers.Commands;
using TrackHub.Manager.Application.Drivers.Queries;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Drivers;

[TestFixture]
public class DriverValidatorTests
{
    private static DriverDto ValidDriver(string name = "Ana Perez", string? phone = "+57 300 1234567", DateOnly? licenseExpiresAt = null)
        => new(Guid.NewGuid(), name, phone, "CC", "123456", true, "EMP-1", "LIC-1", licenseExpiresAt, null);

    private static DriverQualificationDto ValidQualification(DateOnly? issuedAt = null, DateOnly? expiresAt = null, string? type = null, string? status = null)
        => new(Guid.NewGuid(), Guid.NewGuid(), type ?? DriverQualificationTypes.License, "C2", "LIC-1",
            issuedAt, expiresAt, "RUNT", status ?? DriverQualificationStatuses.Valid, null, null);

    // Before spec 09 these commands had NO validators — an empty name reached the writer (§2.2.4).
    [Test]
    public void CreateDriver_RejectsEmptyName()
    {
        var result = new CreateDriverCommandValidator().Validate(new CreateDriverCommand(ValidDriver(name: "")));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void CreateDriver_RejectsMalformedPhone_ButAllowsBlank()
    {
        var validator = new CreateDriverCommandValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new CreateDriverCommand(ValidDriver(phone: "not-a-phone"))).IsValid, Is.False);
            // Phone is optional — absent must stay valid.
            Assert.That(validator.Validate(new CreateDriverCommand(ValidDriver(phone: null))).IsValid, Is.True);
            Assert.That(validator.Validate(new CreateDriverCommand(ValidDriver())).IsValid, Is.True);
        });
    }

    [Test]
    public void CreateDriver_RejectsPastLicenseExpiry_ButUpdateAllowsIt()
    {
        var past = DateOnly.FromDateTime(DateTimeOffset.UtcNow.UtcDateTime).AddDays(-1);

        // Onboarding on an already-expired license is a data-entry error...
        Assert.That(new CreateDriverCommandValidator().Validate(new CreateDriverCommand(ValidDriver(licenseExpiresAt: past))).IsValid, Is.False);
        // ...but an existing record must remain editable while its license lapses.
        Assert.That(new UpdateDriverCommandValidator().Validate(new UpdateDriverCommand(Guid.NewGuid(), ValidDriver(licenseExpiresAt: past))).IsValid, Is.True);
    }

    [Test]
    public void UpdateDriver_RequiresDriverId()
    {
        var result = new UpdateDriverCommandValidator().Validate(new UpdateDriverCommand(Guid.Empty, ValidDriver()));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void DeactivateDriver_RequiresDriverId()
    {
        Assert.That(new DeactivateDriverCommandValidator().Validate(new DeactivateDriverCommand(Guid.Empty)).IsValid, Is.False);
        Assert.That(new DeactivateDriverCommandValidator().Validate(new DeactivateDriverCommand(Guid.NewGuid())).IsValid, Is.True);
    }

    [Test]
    public void CreateQualification_RejectsUnknownTypeAndStatus()
    {
        var validator = new CreateDriverQualificationCommandValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new CreateDriverQualificationCommand(ValidQualification(type: "Wizardry"))).IsValid, Is.False);
            Assert.That(validator.Validate(new CreateDriverQualificationCommand(ValidQualification(status: "Pending"))).IsValid, Is.False);
            Assert.That(validator.Validate(new CreateDriverQualificationCommand(ValidQualification())).IsValid, Is.True);
        });
    }

    [Test]
    public void CreateQualification_RejectsExpiryBeforeIssue()
    {
        var validator = new CreateDriverQualificationCommandValidator();
        var inverted = ValidQualification(issuedAt: new DateOnly(2027, 1, 1), expiresAt: new DateOnly(2026, 1, 1));

        Assert.That(validator.Validate(new CreateDriverQualificationCommand(inverted)).IsValid, Is.False);
    }

    [Test]
    public void AssignDriver_RejectsUnknownAssignmentType()
    {
        var validator = new AssignDriverToTransporterCommandValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new AssignDriverToTransporterCommand(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "Permanent")).IsValid, Is.False);
            Assert.That(validator.Validate(new AssignDriverToTransporterCommand(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, DriverAssignmentTypes.Temporary)).IsValid, Is.True);
            Assert.That(validator.Validate(new AssignDriverToTransporterCommand(Guid.Empty, Guid.NewGuid(), DateTimeOffset.UtcNow, DriverAssignmentTypes.Regular)).IsValid, Is.False);
        });
    }

    [Test]
    public void AssignmentHistoryQuery_RejectsInvertedWindow()
    {
        var validator = new GetDriverAssignmentHistoryQueryValidator();
        var accountId = Guid.NewGuid();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new GetDriverAssignmentHistoryQuery(accountId, From: DateTimeOffset.UtcNow, To: DateTimeOffset.UtcNow.AddDays(-1))).IsValid, Is.False);
            Assert.That(validator.Validate(new GetDriverAssignmentHistoryQuery(accountId)).IsValid, Is.True);
        });
    }

    [Test]
    public void QualificationsQuery_RejectsNegativeWindowAndEmptyAccount()
    {
        var validator = new GetDriverQualificationsQueryValidator();

        Assert.Multiple(() =>
        {
            Assert.That(validator.Validate(new GetDriverQualificationsQuery(Guid.NewGuid(), ExpiringWithinDays: -1)).IsValid, Is.False);
            Assert.That(validator.Validate(new GetDriverQualificationsQuery(Guid.Empty)).IsValid, Is.False);
            Assert.That(validator.Validate(new GetDriverQualificationsQuery(Guid.NewGuid(), ExpiringWithinDays: 30)).IsValid, Is.True);
        });
    }
}
