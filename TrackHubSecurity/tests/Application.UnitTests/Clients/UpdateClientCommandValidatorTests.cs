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
using TrackHub.Security.Application.Clients.Commands.Update;

namespace Application.UnitTests.Clients;

[TestFixture]
public class UpdateClientCommandValidatorTests
{
    private UpdateClientCommandValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new UpdateClientCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_UserId_Is_Empty()
    {
        var command = new UpdateClientCommand(new ClientUserDto(Guid.NewGuid(), Guid.Empty));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Client.UserId);
    }

    [Test]
    public void Should_Have_Error_When_ClientId_Is_Empty()
    {
        var command = new UpdateClientCommand(new ClientUserDto(Guid.Empty, Guid.NewGuid()));
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Client.ClientId);
    }

    [Test]
    public void Should_Have_Error_When_Client_Is_Default()
    {
        var command = new UpdateClientCommand(default);
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(v => v.Client);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Valid()
    {
        var command = new UpdateClientCommand(new ClientUserDto(Guid.NewGuid(), Guid.NewGuid()));
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
