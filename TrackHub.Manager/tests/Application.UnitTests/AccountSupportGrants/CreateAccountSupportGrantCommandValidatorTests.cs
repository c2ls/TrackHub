// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using FluentValidation.TestHelper;
using TrackHub.Manager.Application.AccountSupportGrants.Commands;
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.AccountSupportGrants;

[TestFixture]
public class CreateAccountSupportGrantCommandValidatorTests
{
    private CreateAccountSupportGrantCommandValidator _validator;

    [SetUp]
    public void Setup() => _validator = new CreateAccountSupportGrantCommandValidator();

    private static AccountSupportGrantDto Dto(string accessLevel, DateTimeOffset startsAt, DateTimeOffset endsAt)
        => new(Guid.NewGuid(), Guid.NewGuid(), "reason", "TICKET-1", accessLevel, startsAt, endsAt);

    [Test]
    public void Valid_ReadOnly_Grant_Within_24h_Passes()
    {
        var now = DateTimeOffset.UtcNow;
        var command = new CreateAccountSupportGrantCommand(Dto(SupportAccessLevels.ReadOnly, now, now.AddHours(8)));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Window_Longer_Than_24h_Fails()
    {
        var now = DateTimeOffset.UtcNow;
        var command = new CreateAccountSupportGrantCommand(Dto(SupportAccessLevels.Full, now, now.AddHours(25)));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(v => v.AccountSupportGrant);
    }

    [Test]
    public void EndsAt_Before_StartsAt_Fails()
    {
        var now = DateTimeOffset.UtcNow;
        var command = new CreateAccountSupportGrantCommand(Dto(SupportAccessLevels.Full, now, now.AddHours(-1)));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(v => v.AccountSupportGrant.EndsAt);
    }

    [Test]
    public void Invalid_AccessLevel_Fails()
    {
        var now = DateTimeOffset.UtcNow;
        var command = new CreateAccountSupportGrantCommand(Dto("Elevated", now, now.AddHours(1)));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(v => v.AccountSupportGrant.AccessLevel);
    }
}
