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
using TrackHub.Security.Application.Users.Events;

namespace Application.UnitTests.Users;
[TestFixture]
public class CreateUserCommandHandlerTests
{
    private Mock<IUserWriter> _mockUserWriter;
    private Mock<IUserReader> _mockUserReader;
    private Mock<IPublisher> _mockPublisher;
    private Mock<IUser> _mockUser;

    [SetUp]
    public void SetUp()
    {
        _mockUserWriter = new Mock<IUserWriter>();
        _mockUserReader = new Mock<IUserReader>();
        _mockPublisher = new Mock<IPublisher>();
        _mockUser = new Mock<IUser>();
    }

    [Test]
    public async Task Handle_ValidCommand_ReturnsUserVm()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            // Set up the properties of the CreateUserDto object
        };
        var createUserCommand = new CreateUserCommand(createUserDto);
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = new UserVm
        {
            UserId = userId,
            Username = "username",
            AccountId = accountId
        };
        var shrankUser = new UserShrankDto(user.UserId, user.Username, user.AccountId);

        _mockUserReader.Setup(x => x.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserVm { AccountId = accountId });
        _mockUserWriter.Setup(x => x.CreateUserAsync(createUserDto, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPublisher.Setup(x => x.Publish(It.IsAny<UserCreated.Notification>(), It.IsAny<CancellationToken>()))
            .Verifiable();
        _mockUser.Setup(x => x.Id).Returns(userId.ToString());

        var handler = new CreateUserCommandHandler(_mockUserWriter.Object, _mockUserReader.Object, _mockUser.Object, _mockPublisher.Object, _mockUser.Object);

        // Act
        var result = await handler.Handle(createUserCommand, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(user));
        _mockPublisher.Verify(x => x.Publish(It.Is<UserCreated.Notification>(n => n.User == shrankUser), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_InvalidCommand_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            // Set up the properties of the CreateUserDto object
        };
        var createUserCommand = new CreateUserCommand(createUserDto);
        var user = new UserVm
        {
            UserId = Guid.NewGuid(),
            Username = "username",
            AccountId = Guid.NewGuid()
        };
        var shrankUser = new UserShrankDto(user.UserId, user.Username, user.AccountId);

        _mockUserReader.Setup(x => x.GetUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserVm { AccountId = Guid.NewGuid() });
        _mockUserWriter.Setup(x => x.CreateUserAsync(createUserDto, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockPublisher.Setup(x => x.Publish(It.IsAny<UserCreated.Notification>(), It.IsAny<CancellationToken>()))
            .Verifiable();

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() =>
            new CreateUserCommandHandler(
            _mockUserWriter.Object,
            _mockUserReader.Object,
            _mockUser.Object,
            _mockPublisher.Object,
            _mockUser.Object));
    }
}
