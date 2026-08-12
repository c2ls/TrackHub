// Copyright (c) 2026 Sergio Hernandez. All rights reserved.

using TrackHub.Manager.Application.Accounts.Commands.Update;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Domain.Interfaces;

namespace Application.UnitTests.Accounts;

[TestFixture]
public class UpdateAccountCommandHandlerTests
{
    private Mock<IAccountWriter> _accountWriterMock;
    private UpdateAccountCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _accountWriterMock = new Mock<IAccountWriter>();
        _handler = new UpdateAccountCommandHandler(_accountWriterMock.Object);
    }

    [Test]
    public async Task Handle_WhenCalled_CallsUpdate()
    {
        // Arrange
        var dto = new UpdateAccountDto(Guid.NewGuid(), "Name", "Desc", 1, true);
        var command = new UpdateAccountCommand(dto);

        _accountWriterMock.Setup(w => w.UpdateAccountAsync(dto, CancellationToken.None)).Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _accountWriterMock.Verify(w => w.UpdateAccountAsync(dto, CancellationToken.None), Times.Once);
    }
}
