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
using TrackHub.Security.Application.Clients.Commands.Create;

namespace Application.UnitTests.Clients;

[TestFixture]
public class CreateClientCommandValidatorTests
{
    private CreateClientCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new CreateClientCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var command = new CreateClientCommand(new ClientDto(null, "", "Desc", "secret"));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Client.Name);
    }

    [Test]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        var command = new CreateClientCommand(new ClientDto(null, "Name", "", "secret"));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Client.Description);
    }

    [Test]
    public void Should_Have_Error_When_Secret_Is_Empty()
    {
        var command = new CreateClientCommand(new ClientDto(null, "Name", "Desc", ""));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Client.Secret);
    }

    [Test]
    public void Should_Have_Error_When_Client_Is_Default()
    {
        var command = new CreateClientCommand(default);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Client);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Valid()
    {
        var command = new CreateClientCommand(new ClientDto(Guid.NewGuid(), "ValidClient", "ValidDesc", "ValidSecret"));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_Allow_Null_UserId()
    {
        var command = new CreateClientCommand(new ClientDto(null, "Client", "Desc", "Secret"));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
