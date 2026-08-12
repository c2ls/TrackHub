// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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
using TrackHub.Manager.Application.Operators.Commands.Create;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Operators;

[TestFixture]
public class CreateOperatorValidatorTests
{
    private CreateOperatorValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateOperatorValidator();
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateOperatorCommand(new OperatorDto("", null, null, null, null, null, 1));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Operator.Name);
    }

    [Test]
    public void Should_Have_Error_When_ProtocolTypeId_Is_Zero()
    {
        var command = new CreateOperatorCommand(new OperatorDto("Valid", null, null, null, null, null, 0));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Operator.ProtocolTypeId);
    }

    [Test]
    public void Should_Have_Error_When_Operator_Is_Default()
    {
        var command = new CreateOperatorCommand(default);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Operator);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Valid()
    {
        var command = new CreateOperatorCommand(new OperatorDto("ValidOp", "Desc", "+123", "e@e.com", "Addr", "Contact", 1));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_Allow_Null_Optional_Fields()
    {
        var command = new CreateOperatorCommand(new OperatorDto("OpName", null, null, null, null, null, 3));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
