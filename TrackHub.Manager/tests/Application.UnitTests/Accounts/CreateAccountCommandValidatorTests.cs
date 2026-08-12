// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using FluentValidation.TestHelper;
using TrackHub.Manager.Application.Accounts.Commands.Create;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class CreateAccountCommandValidatorTests
{
    private CreateAccountValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateAccountValidator();
    }

    [Test]
    public void Should_Have_Error_When_Account_Is_Null()
    {
        var command = new CreateAccountCommand(default);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(v => v.Account);
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateAccountCommand(new AccountDto(string.Empty, null, 1, true, "p", "e", "f", "l"));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(v => v.Account.Name);
    }

    [Test]
    public void Should_Have_Error_When_TypeId_Is_Empty()
    {
        var command = new CreateAccountCommand(new AccountDto("Name", null, 0, true, "p", "e", "f", "l"));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(v => v.Account.TypeId);
    }

    [Test]
    public void Should_Not_Have_Error_When_All_Valid()
    {
        var command = new CreateAccountCommand(new AccountDto("Name", "d", 1, true, "p", "e", "f", "l"));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
