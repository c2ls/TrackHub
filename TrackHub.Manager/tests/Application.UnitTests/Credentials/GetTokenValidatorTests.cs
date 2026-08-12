// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using FluentValidation.TestHelper;
using TrackHub.Manager.Application.CredentialToken.Queries.GetToken;

namespace Application.UnitTests.Credentials.Queries.GetToken;

[TestFixture]
public class GetTokenValidatorTests
{
    private GetTokenValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new GetTokenValidator();
    }

    [Test]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        var q = new GetTokenQuery(Guid.Empty);
        var result = _validator.TestValidate(q);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
