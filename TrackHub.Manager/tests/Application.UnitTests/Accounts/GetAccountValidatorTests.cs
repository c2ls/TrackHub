// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using FluentValidation.TestHelper;
using TrackHub.Manager.Application.Accounts.Queries.Get;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class GetAccountValidatorTests
{
    private GetAccountValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new GetAccountValidator();
    }

    [Test]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var query = new GetAccountQuery(Guid.Empty);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Test]
    public void Should_Not_Have_Error_When_Id_Is_Present()
    {
        var query = new GetAccountQuery(Guid.NewGuid());
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
