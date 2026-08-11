using Common.Domain.Enums;
using FluentValidation.TestHelper;
using TrackHub.Manager.Application.GpsIntegration.Commands;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.GpsIntegration;

[TestFixture]
public class RegisterManualDeviceValidatorTests
{
    private RegisterManualDeviceValidator _validator = null!;

    [SetUp]
    public void SetUp() => _validator = new RegisterManualDeviceValidator();

    private static DeviceDto ValidDto() => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "SER-1",
        "ABC123",
        0,
        null,
        (short)DeviceType.Cellular,
        null,
        null,
        null);

    [Test]
    public void Validate_ValidCommand_Passes()
        => _validator.TestValidate(new RegisterManualDeviceCommand(ValidDto()))
            .ShouldNotHaveAnyValidationErrors();

    [Test]
    public void Validate_DefaultDevice_Fails()
        => _validator.TestValidate(new RegisterManualDeviceCommand(default))
            .ShouldHaveValidationErrorFor(v => v.Device);

    [Test]
    public void Validate_EmptyName_Fails()
        => _validator.TestValidate(new RegisterManualDeviceCommand(ValidDto() with { Name = "" }))
            .ShouldHaveValidationErrorFor(v => v.Device.Name);

    [Test]
    public void Validate_EmptySerial_Fails()
        => _validator.TestValidate(new RegisterManualDeviceCommand(ValidDto() with { Serial = "" }))
            .ShouldHaveValidationErrorFor(v => v.Device.Serial);

    [Test]
    public void Validate_NegativeIdentifier_Fails()
        => _validator.TestValidate(new RegisterManualDeviceCommand(ValidDto() with { Identifier = -1 }))
            .ShouldHaveValidationErrorFor(v => v.Device.Identifier);

    [Test]
    public void Validate_UnknownDeviceType_Fails()
        => _validator.TestValidate(new RegisterManualDeviceCommand(ValidDto() with { DeviceTypeId = 99 }))
            .ShouldHaveValidationErrorFor(v => v.Device.DeviceTypeId);

    [Test]
    public void Validate_ExplicitIdentifier_Passes()
        => _validator.TestValidate(new RegisterManualDeviceCommand(ValidDto() with { Identifier = 12 }))
            .ShouldNotHaveAnyValidationErrors();
}
