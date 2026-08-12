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
using TrackHub.Manager.Application.Operators.Commands.Update;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Operators;

[TestFixture]
public class UpdateOperatorValidatorTests
{
    private UpdateOperatorValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateOperatorValidator();
    }

    [Test]
    public void Should_Have_Error_When_OperatorId_Is_Empty()
    {
        var command = new UpdateOperatorCommand(new UpdateOperatorDto(Guid.Empty, "Name", null, null, null, null, null, 1, 60));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Operator.OperatorId);
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new UpdateOperatorCommand(new UpdateOperatorDto(Guid.NewGuid(), "", null, null, null, null, null, 1, 60));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Operator.Name);
    }

    [Test]
    public void Should_Have_Error_When_ProtocolTypeId_Is_Zero()
    {
        var command = new UpdateOperatorCommand(new UpdateOperatorDto(Guid.NewGuid(), "Name", null, null, null, null, null, 0, 60));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Operator.ProtocolTypeId);
    }

    [Test]
    public void Should_Have_Error_When_Operator_Is_Default()
    {
        var command = new UpdateOperatorCommand(default);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Operator);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Valid()
    {
        var command = new UpdateOperatorCommand(new UpdateOperatorDto(Guid.NewGuid(), "ValidName", "Desc", "+1", "e@e.com", "Addr", "Contact", 2, 60));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
