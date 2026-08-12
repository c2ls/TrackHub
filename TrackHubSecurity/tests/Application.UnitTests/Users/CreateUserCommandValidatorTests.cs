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

using TrackHub.Security.Application.Users.Commands.Create;
using FluentValidation.TestHelper;
using Common.Domain.Constants;

namespace Application.UnitTests.Users;
[TestFixture]
public class CreateUserCommandValidatorTests
{
    private CreateUserCommandValidator _validator;
    private Mock<IUserReader> _userReaderMock;

    [SetUp]
    public void SetUp()
    {
        _userReaderMock = new Mock<IUserReader>();
        _validator = new CreateUserCommandValidator(_userReaderMock.Object);
    }

    [Test]
    public async Task ShouldHaveError_WhenUsernameIsEmpty()
    {
        var command = new CreateUserCommand
        {
            User = new CreateUserDto
            {
                Username = "",
                Password = "password",
                EmailAddress = "test@example.com"
            }
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(v => v.User.Username);
    }

    [Test]
    public async Task ShouldHaveError_WhenUsernameExceedsMaxLength()
    {
        var command = new CreateUserCommand
        {
            User = new CreateUserDto
            {
                FirstName = "First",
                LastName = "Last",
                Username = new string('a', ColumnMetadata.DefaultUserNameLength + 1),
                Password = "password",
                EmailAddress = "test@example.com"
            }
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(v => v.User.Username);
    }

    [Test]
    public async Task ShouldHaveError_WhenPasswordIsEmpty()
    {
        var command = new CreateUserCommand
        {
            User = new CreateUserDto
            {
                Username = "username",
                Password = "",
                EmailAddress = "test@example.com"
            }
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(v => v.User.Password);
    }

    [Test]
    public async Task ShouldHaveError_WhenPasswordDoesNotMeetMinimumLength()
    {
        var command = new CreateUserCommand
        {
            User = new CreateUserDto
            {
                Username = "username",
                Password = "pass",
                EmailAddress = "test@example.com"
            }
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(v => v.User.Password);
    }

    [Test]
    public async Task ShouldHaveError_WhenEmailAddressIsEmpty()
    {
        var command = new CreateUserCommand
        {
            User = new CreateUserDto
            {
                Username = "username",
                Password = "password",
                EmailAddress = ""
            }
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(v => v.User.EmailAddress);
    }

    [Test]
    public async Task ShouldHaveError_WhenEmailAddressIsInvalid()
    {
        var command = new CreateUserCommand
        {
            User = new CreateUserDto
            {
                Username = "username",
                Password = "password",
                EmailAddress = "invalidemail"
            }
        };

        var result = await _validator.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(v => v.User.EmailAddress);
    }

    [Test]
    public async Task ShouldHaveError_WhenEmailAddressAlreadyExists()
    {
        var command = new CreateUserCommand
        {
            User = new CreateUserDto
            {
                FirstName = "First",
                LastName = "Last",
                Username = "username",
                Password = "Password1",
                EmailAddress = "existing@example.com"
            }
        };

        _userReaderMock.Setup(x => x.ValidateEmailAddressAsync(command.User.EmailAddress, CancellationToken.None))
            .ReturnsAsync(false);

        var result = await _validator.ValidateAsync(command);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors[0].ErrorMessage, Is.EqualTo("Email address already in use"));
        }
    }

    [Test]
    public async Task ShouldBeValid_WhenAllFieldsAreValid()
    {
        var command = new CreateUserCommand
        {
            User = new CreateUserDto
            {
                FirstName = "first",
                LastName = "last",
                Username = "username",
                Password = "Password1",
                EmailAddress = "test@example.com"
            }
        };

        _userReaderMock.Setup(x => x.ValidateEmailAddressAsync(command.User.EmailAddress, CancellationToken.None))
            .ReturnsAsync(true);

        var result = await _validator.ValidateAsync(command);

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public async Task ShouldHaveError_WhenAllFieldsAreInvalid()
    {
        var command = new CreateUserCommand
        {
            User = new CreateUserDto
            {
                Password = "pass",
                EmailAddress = "existing@example.com"
            }
        };

        _userReaderMock.Setup(x => x.ValidateEmailAddressAsync(command.User.EmailAddress, CancellationToken.None))
            .ReturnsAsync(false);

        var result = await _validator.ValidateAsync(command);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.EqualTo(7));
        }
    }
}
