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
using TrackHub.Manager.Application.Groups.Commands.Update;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Groups;

[TestFixture]
public class UpdateGroupValidatorTests
{
    private UpdateGroupValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateGroupValidator();
    }

    [Test]
    public void Should_Have_Error_When_GroupId_Is_Zero()
    {
        var command = new UpdateGroupCommand(new UpdateGroupDto(0, "Name", "Desc", true));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Group.GroupId);
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new UpdateGroupCommand(new UpdateGroupDto(1L, "", "Desc", true));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Group.Name);
    }

    [Test]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        var command = new UpdateGroupCommand(new UpdateGroupDto(1L, "Name", "", true));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Group.Description);
    }

    [Test]
    public void Should_Have_Error_When_Group_Is_Default()
    {
        var command = new UpdateGroupCommand(default);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Group);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Valid()
    {
        var command = new UpdateGroupCommand(new UpdateGroupDto(1L, "Fleet", "Main fleet", true));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
