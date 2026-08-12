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

using System.Reflection.Metadata;
using Common.Application.Interfaces;
using TrackHub.Security.Application.Users.Commands.Create;
using TrackHub.Security.Application.Users.Commands.Delete;
using TrackHub.Security.Application.Users.Events;

namespace Application.UnitTests.Users;

[TestFixture]
internal class DeleteUserTests
{
    private Mock<IUserWriter> _writerMock;
    private Mock<IPublisher> _publisherMock;
    private Mock<IUser> _mockUser;

    [SetUp]
    public void Setup()
    {
        // Initialize the mock and the object under test before each test
        _writerMock = new Mock<IUserWriter>();
        _publisherMock = new Mock<IPublisher>();
        _mockUser = new Mock<IUser>();
    }

    [Test]
    public async Task Handle_ValidCommand_DeleteUserAndPublishesNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        _writerMock.Setup(m => m.DeleteUserAsync(userId, cancellationToken))
                  .Returns(Task.CompletedTask); // DeleteUserAsync returns a completed task
        _mockUser.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var handler = new DeleteUserCommandHandler(_writerMock.Object, _mockUser.Object, _publisherMock.Object, _mockUser.Object);
        var command = new DeleteUserCommand(userId);

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        // Verify that DeleteUserAsync was called with the correct arguments
        _writerMock.Verify(m => m.DeleteUserAsync(userId, cancellationToken), Times.Once);

        // Verify that Publish was called with the correct arguments
        _publisherMock.Verify(m => m.Publish(It.IsAny<UserDeleted.Notification>(), cancellationToken), Times.Once);
    }

    [Test]
    public void Handle_InvalidCommand_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        _writerMock.Setup(m => m.DeleteUserAsync(userId, cancellationToken))
                  .Returns(Task.CompletedTask); // DeleteUserAsync returns a completed task
        _mockUser.Setup(x => x.Id).Returns(userId.ToString());

        var handler = new DeleteUserCommandHandler(_writerMock.Object, _mockUser.Object, _publisherMock.Object, _mockUser.Object);
        var command = new DeleteUserCommand(userId);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await handler.Handle(command, CancellationToken.None));

        // Assert
        // Verify that DeleteUserAsync was called with the correct arguments
        _writerMock.Verify(m => m.DeleteUserAsync(userId, cancellationToken), Times.Never);

        // Verify that Publish was called with the correct arguments
        _publisherMock.Verify(m => m.Publish(It.IsAny<UserDeleted.Notification>(), cancellationToken), Times.Never);
    }
}
