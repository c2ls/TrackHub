// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Accounts.Commands.UpdateBranding;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class UpdateAccountBrandingValidatorTests
{
    private Mock<IAccountBrandingReader> _brandingReaderMock;
    private UpdateAccountBrandingValidator _validator;
    private readonly Guid _accountId = Guid.NewGuid();

    [SetUp]
    public void Setup()
    {
        _brandingReaderMock = new Mock<IAccountBrandingReader>();
        _brandingReaderMock
            .Setup(r => r.LogoDocumentBelongsToAccountAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _validator = new UpdateAccountBrandingValidator(_brandingReaderMock.Object);
    }

    private AccountBrandingDto Dto(string color = "#1A73E8", string displayName = "Acme", Guid? logo = null)
        => new(_accountId, displayName, logo, color, null);

    [Test]
    public async Task Validate_ValidBranding_IsValid()
    {
        var result = await _validator.ValidateAsync(new UpdateAccountBrandingCommand(Dto()));
        Assert.That(result.IsValid, Is.True);
    }

    [TestCase("1A73E8")]     // missing '#'
    [TestCase("#1A73E")]     // too short
    [TestCase("#1A73E8F")]   // too long
    [TestCase("#GGGGGG")]    // non-hex
    [TestCase("blue")]
    public async Task Validate_InvalidColor_IsInvalid(string color)
    {
        var result = await _validator.ValidateAsync(new UpdateAccountBrandingCommand(Dto(color: color)));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public async Task Validate_EmptyDisplayName_IsInvalid()
    {
        var result = await _validator.ValidateAsync(new UpdateAccountBrandingCommand(Dto(displayName: "")));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public async Task Validate_EmptyAccountId_IsInvalid()
    {
        var dto = new AccountBrandingDto(Guid.Empty, "Acme", null, "#1A73E8", null);
        var result = await _validator.ValidateAsync(new UpdateAccountBrandingCommand(dto));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public async Task Validate_LogoNotOwnedByAccount_IsInvalid()
    {
        _brandingReaderMock
            .Setup(r => r.LogoDocumentBelongsToAccountAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var result = await _validator.ValidateAsync(new UpdateAccountBrandingCommand(Dto(logo: Guid.NewGuid())));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public async Task Validate_NoLogo_SkipsOwnershipCheck()
    {
        var result = await _validator.ValidateAsync(new UpdateAccountBrandingCommand(Dto(logo: null)));
        Assert.That(result.IsValid, Is.True);
        _brandingReaderMock.Verify(
            r => r.LogoDocumentBelongsToAccountAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
