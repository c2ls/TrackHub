// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using FluentValidation.TestHelper;
using Microsoft.Extensions.Configuration;
using TrackHub.Manager.Application.Credentials.Command;
using TrackHub.Manager.Application.Credentials.Command.Create;
using TrackHub.Manager.Application.Credentials.Command.Update;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Credentials.Command;

[TestFixture]
public class CredentialUriValidationTests
{
    private Mock<IConfiguration> _mockConfiguration;

    [SetUp]
    public void Setup() => _mockConfiguration = new Mock<IConfiguration>();

    private CreateCredentialValidator CreateValidator() => new(_mockConfiguration.Object);

    private static CreateCredentialCommand CommandFor(string uri)
        => new(new CredentialDto(uri, "user", "pass", null, null, Guid.NewGuid()));

    [TestCase("https://gps.example.com/")]
    [TestCase("http://gps.example.com/api/")]
    [TestCase("https://203.0.113.10/")]
    public void Should_Accept_Public_Http_Uris(string uri)
        => CreateValidator().TestValidate(CommandFor(uri)).ShouldNotHaveAnyValidationErrors();

    [TestCase("file:///etc/passwd/")]
    [TestCase("ftp://gps.example.com/")]
    [TestCase("gps.example.com/")]
    [TestCase("/relative/path/")]
    public void Should_Reject_Non_Http_Or_Relative_Uris(string uri)
        => CreateValidator().TestValidate(CommandFor(uri)).ShouldHaveValidationErrorFor(x => x.Credential.Uri);

    [TestCase("http://169.254.169.254/")]
    [TestCase("http://localhost:8080/")]
    [TestCase("http://127.0.0.1/")]
    [TestCase("http://10.0.0.5/")]
    [TestCase("http://172.16.4.4/")]
    [TestCase("http://192.168.1.1/")]
    [TestCase("http://100.100.0.1/")]
    [TestCase("http://[::1]/")]
    [TestCase("http://[fd00::1]/")]
    [TestCase("http://metadata.google.internal/")]
    public void Should_Reject_Non_Routable_Hosts(string uri)
        => CreateValidator().TestValidate(CommandFor(uri)).ShouldHaveValidationErrorFor(x => x.Credential.Uri);

    [Test]
    public void Should_Allow_Non_Routable_Hosts_When_Configured()
    {
        _mockConfiguration.Setup(c => c[CredentialUriRules.AllowPrivateHostsKey]).Returns("true");
        CreateValidator().TestValidate(CommandFor("http://localhost:8080/")).ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_Still_Require_A_Trailing_Slash_On_Create()
        => CreateValidator().TestValidate(CommandFor("https://gps.example.com"))
            .ShouldHaveValidationErrorFor(x => x.Credential.Uri)
            .WithErrorMessage("Credential Uri must end with '/'");

    [Test]
    public void Update_Should_Reject_Non_Routable_Hosts()
    {
        var validator = new UpdateCredentialValidator(_mockConfiguration.Object);
        var command = new UpdateCredentialCommand(new UpdateCredentialDto(Guid.NewGuid(), "http://169.254.169.254/", "user", "pass", "key", "key2"));
        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Credential.Uri);
    }

    [Test]
    public void Update_Should_Accept_A_Public_Uri_Without_A_Trailing_Slash()
    {
        var validator = new UpdateCredentialValidator(_mockConfiguration.Object);
        var command = new UpdateCredentialCommand(new UpdateCredentialDto(Guid.NewGuid(), "https://gps.example.com/api", "user", "pass", "key", "key2"));
        validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void UpdateOperator_Should_Reject_Non_Routable_Hosts()
    {
        var validator = new UpdateOperatorCredentialValidator(_mockConfiguration.Object);
        var command = new UpdateOperatorCredentialCommand(new UpdateOperatorCredentialDto(Guid.NewGuid(), "http://192.168.0.10/", "user", "pass", "key", "key2"));
        validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Credential.Uri);
    }
}
