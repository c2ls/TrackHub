// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using FluentValidation.TestHelper;
using TrackHub.Manager.Application.Credentials.Queries.Get;

namespace Application.UnitTests.Credentials.Queries.Get;

[TestFixture]
public class GetCredentialValidatorTests
{
    private GetCredentialValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new GetCredentialValidator();
    }

    [Test]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var q = new GetCredentialQuery(Guid.Empty);
        var result = _validator.TestValidate(q);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
