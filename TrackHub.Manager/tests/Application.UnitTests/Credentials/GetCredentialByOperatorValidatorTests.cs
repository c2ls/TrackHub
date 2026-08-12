// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using FluentValidation.TestHelper;
using TrackHub.Manager.Application.Credentials.Queries.GetByOperator;

namespace Application.UnitTests.Credentials.Queries.GetByOperator;

[TestFixture]
public class GetCredentialByOperatorValidatorTests
{
    private GetCredentialByOperatorValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new GetCredentialByOperatorValidator();
    }

    [Test]
    public void Should_Have_Error_When_OperatorId_Is_Empty()
    {
        var q = new GetCredentialByOperatorQuery(Guid.Empty);
        var result = _validator.TestValidate(q);
        result.ShouldHaveValidationErrorFor(x => x.OperatorId);
    }
}
