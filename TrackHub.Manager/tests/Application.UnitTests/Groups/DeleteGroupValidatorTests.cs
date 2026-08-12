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
using TrackHub.Manager.Application.Groups.Commands.Delete;

namespace Application.UnitTests.Groups;

[TestFixture]
public class DeleteGroupValidatorTests
{
    private DeleteGroupValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new DeleteGroupValidator();
    }

    [Test]
    public void Should_Have_Error_When_Id_Is_Zero()
    {
        var command = new DeleteGroupCommand(0);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Id);
    }

    [Test]
    public void Should_Not_Have_Error_When_Id_Is_Valid()
    {
        var command = new DeleteGroupCommand(1L);
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
