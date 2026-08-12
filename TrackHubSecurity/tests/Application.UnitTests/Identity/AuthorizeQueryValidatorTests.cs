// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using FluentValidation.TestHelper;
using TrackHub.Security.Application.Identity.Queries.Authorize;

namespace Application.UnitTests.Identity;

[TestFixture]
public class AuthorizeQueryValidatorTests
{
    private AuthorizeQueryValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new AuthorizeQueryValidator();
    }

    [Test]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var query = new AuthorizeQuery(Guid.Empty, "Resource", "Action");
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(v => v.UserId);
    }

    [Test]
    public void Should_Have_Error_When_Resource_Is_Empty()
    {
        var query = new AuthorizeQuery(Guid.NewGuid(), "", "Action");
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(v => v.Resource);
    }

    [Test]
    public void Should_Have_Error_When_Action_Is_Empty()
    {
        var query = new AuthorizeQuery(Guid.NewGuid(), "Resource", "");
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(v => v.Action);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Valid()
    {
        var query = new AuthorizeQuery(Guid.NewGuid(), "Operators", "Read");
        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_Have_All_Errors_When_All_Fields_Empty()
    {
        var query = new AuthorizeQuery(Guid.Empty, "", "");
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(v => v.UserId);
        result.ShouldHaveValidationErrorFor(v => v.Resource);
        result.ShouldHaveValidationErrorFor(v => v.Action);
    }
}
