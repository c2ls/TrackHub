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

using Common.Domain.Extensions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using TrackHub.Manager.Application.Credentials.Command.Create;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace TrackHub.Manager.Application.UnitTests.Credentials.Command.Create;

[TestFixture]
public class CreateCredentialTests
{
    private Mock<ICredentialWriter> _mockCredentialWriter;
    private Mock<IConfiguration> _mockConfiguration;
    private CreateCredentialValidator _validator;
    private CreateCredentialCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockCredentialWriter = new Mock<ICredentialWriter>();
        _mockConfiguration = new Mock<IConfiguration>();
        _validator = new CreateCredentialValidator(_mockConfiguration.Object);

        _handler = new CreateCredentialCommandHandler(_mockCredentialWriter.Object, _mockConfiguration.Object);
    }

    [Test]
    public async Task Handle_WhenValidRequest_ReturnsCredentialVm()
    {
        // Arrange
        var command = new CreateCredentialCommand(new CredentialDto());

        var key = "encryptionKey";
        var salt = CryptographyExtensions.GenerateAesKey(256);

        _mockConfiguration.Setup(c => c["AppSettings:EncryptionKey"]).Returns(key);
        _mockCredentialWriter.Setup(w => w.CreateCredentialAsync(command.Credential, salt, key, CancellationToken.None))
            .ReturnsAsync(new CredentialVm());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.Default);
        Assert.That(result, Is.InstanceOf<CredentialVm>());
    }

    [Test]
    public void Should_Have_Error_When_Operator_Is_Empty()
    {
        var command = new CreateCredentialCommand
        {
            Credential = new CredentialDto
            {
                Uri = "https://example.com/",
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(v => v.Credential.OperatorId);
    }

    [Test]
    public void Should_Have_Error_When_Uri_Is_Empty()
    {
        var command = new CreateCredentialCommand
        {
            Credential = new CredentialDto
            {
                Uri = string.Empty
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(v => v.Credential.Uri);
    }

    [Test]
    public void Should_Have_Error_When_Uri_Does_Not_End_With_Slash()
    {
        var command = new CreateCredentialCommand
        {
            Credential = new CredentialDto
            {
                Uri = "https://example.com"
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(v => v.Credential.Uri)
            .WithErrorMessage("Credential Uri must end with '/'");
    }

    [Test]
    public void Should_Not_Have_Error_When_All_Validation_Passes()
    {
        var command = new CreateCredentialCommand
        {
            Credential = new CredentialDto
            {
                Username = "testuser",
                Password = "password",
                OperatorId = Guid.NewGuid(),
                Uri = "https://example.com/"
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
