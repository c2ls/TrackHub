// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using Common.Application.Interfaces;
using Common.Domain.Enums;
using TrackHub.Manager.Application.Accounts.Commands.ChangeStatus;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class ChangeAccountStatusValidatorTests
{
    private Mock<IAccountOperationalStatusReader> _statusReaderMock;
    private ChangeAccountStatusValidator _validator;
    private readonly Guid _accountId = Guid.NewGuid();

    [SetUp]
    public void Setup()
    {
        _statusReaderMock = new Mock<IAccountOperationalStatusReader>();
        _validator = new ChangeAccountStatusValidator(_statusReaderMock.Object);
    }

    private void CurrentStatus(AccountStatus? status)
        => _statusReaderMock
            .Setup(r => r.GetAccountStatusAsync(_accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

    [Test]
    public async Task Validate_AllowedTransitionWithReason_IsValid()
    {
        CurrentStatus(AccountStatus.Active);
        var result = await _validator.ValidateAsync(
            new ChangeAccountStatusCommand(_accountId, AccountStatus.Suspended, "Non-payment."));
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_DisallowedTransition_IsInvalid()
    {
        // Archived is terminal: Archived -> Active is not allowed.
        CurrentStatus(AccountStatus.Archived);
        var result = await _validator.ValidateAsync(
            new ChangeAccountStatusCommand(_accountId, AccountStatus.Active, null));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public async Task Validate_SuspendWithoutReason_IsInvalid()
    {
        CurrentStatus(AccountStatus.Active);
        var result = await _validator.ValidateAsync(
            new ChangeAccountStatusCommand(_accountId, AccountStatus.Suspended, null));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public async Task Validate_CancelWithoutReason_IsInvalid()
    {
        CurrentStatus(AccountStatus.Trial);
        var result = await _validator.ValidateAsync(
            new ChangeAccountStatusCommand(_accountId, AccountStatus.Cancelled, "  "));
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public async Task Validate_ReactivateWithoutReason_IsValid()
    {
        // Suspended -> Active is allowed and needs no reason.
        CurrentStatus(AccountStatus.Suspended);
        var result = await _validator.ValidateAsync(
            new ChangeAccountStatusCommand(_accountId, AccountStatus.Active, null));
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_UnknownAccount_TransitionRulePasses()
    {
        // A missing account defers to the writer's 404 rather than a "not allowed" 400.
        CurrentStatus(null);
        var result = await _validator.ValidateAsync(
            new ChangeAccountStatusCommand(_accountId, AccountStatus.Active, null));
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task Validate_EmptyAccountId_IsInvalid()
    {
        var result = await _validator.ValidateAsync(
            new ChangeAccountStatusCommand(Guid.Empty, AccountStatus.Active, null));
        Assert.That(result.IsValid, Is.False);
    }
}
