// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using FluentValidation.TestHelper;
using TrackHub.Manager.Application.Accounts.Commands.Update;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class UpdateAccountCommandValidatorTests
{
    private UpdateAccountValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateAccountValidator();
    }

    [Test]
    public void Should_Have_Error_When_Account_Is_Null()
    {
        var command = new UpdateAccountCommand(default);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Account);
    }

    [Test]
    public void Should_Have_Error_When_AccountId_Is_Empty()
    {
        var command = new UpdateAccountCommand(new UpdateAccountDto(Guid.Empty, "Name", null, 1, true));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Account.AccountId);
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new UpdateAccountCommand(new UpdateAccountDto(Guid.NewGuid(), string.Empty, null, 1, true));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Account.Name);
    }

    [Test]
    public void Should_Have_Error_When_TypeId_Is_Empty()
    {
        var command = new UpdateAccountCommand(new UpdateAccountDto(Guid.NewGuid(), "Name", null, 0, true));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Account.TypeId);
    }

    [Test]
    public void Should_Not_Have_Error_When_All_Valid()
    {
        var command = new UpdateAccountCommand(new UpdateAccountDto(Guid.NewGuid(), "Name", "d", 1, true));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
