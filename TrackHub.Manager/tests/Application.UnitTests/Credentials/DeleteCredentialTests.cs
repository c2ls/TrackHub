// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using FluentValidation.TestHelper;
using TrackHub.Manager.Application.Credentials.Command.Delete;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Credentials.Command.Delete;

[TestFixture]
public class DeleteCredentialTests
{
    private Mock<ICredentialWriter> _mockWriter;
    private DeleteCredentialCommandHandler _handler;
    private DeleteCredentialValidator _validator;

    [SetUp]
    public void Setup()
    {
        _mockWriter = new Mock<ICredentialWriter>();
        _handler = new DeleteCredentialCommandHandler(_mockWriter.Object);
        _validator = new DeleteCredentialValidator();
    }

    [Test]
    public async Task Handle_Calls_Delete()
    {
        var id = Guid.NewGuid();
        var cmd = new DeleteCredentialCommand(id);
        _mockWriter.Setup(w => w.DeleteCredentialAsync(id, CancellationToken.None)).Returns(Task.CompletedTask);

        await _handler.Handle(cmd, CancellationToken.None);

        _mockWriter.Verify(w => w.DeleteCredentialAsync(id, CancellationToken.None), Times.Once);
    }

    [Test]
    public void Validator_Should_Have_Error_When_Id_Is_Empty()
    {
        var cmd = new DeleteCredentialCommand(Guid.Empty);
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
