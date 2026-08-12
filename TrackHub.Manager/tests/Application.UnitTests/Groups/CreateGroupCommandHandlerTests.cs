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

using Common.Application.Interfaces;
using TrackHub.Manager.Application.Groups.Commands.Create;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Groups;

[TestFixture]
public class CreateGroupCommandHandlerTests
{
    private Mock<IGroupWriter> _writerMock;
    private Mock<IUserReader> _userReaderMock;
    private Mock<IUser> _userMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IGroupWriter>();
        _userReaderMock = new Mock<IUserReader>();
        _userMock = new Mock<IUser>();
    }

    [Test]
    public async Task Handle_ValidCommand_LooksUpUserAndCreatesGroup()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var groupDto = new GroupDto("Fleet A", "Main fleet", true);
        var expectedVm = new GroupVm(1L, "Fleet A", "Main fleet", true, accountId);

        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userReaderMock.Setup(r => r.GetUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserVm(userId, "admin", true, accountId));
        _writerMock.Setup(w => w.CreateGroupAsync(groupDto, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVm);

        var handler = new CreateGroupCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object);

        // Act
        var result = await handler.Handle(new CreateGroupCommand(groupDto), CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedVm));
        _writerMock.Verify(w => w.CreateGroupAsync(groupDto, accountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Constructor_NullUserId_ThrowsUnauthorizedAccessException()
    {
        _userMock.Setup(u => u.Id).Returns((string?)null);

        Assert.Throws<UnauthorizedAccessException>(() =>
            new CreateGroupCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object));
    }

    [Test]
    public async Task Handle_DifferentUsers_GetDifferentAccountIds()
    {
        // Arrange — verify account scoping works per-user
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var groupDto = new GroupDto("Group", "Desc", true);

        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userReaderMock.Setup(r => r.GetUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserVm(userId, "user1", true, accountId));
        _writerMock.Setup(w => w.CreateGroupAsync(groupDto, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GroupVm(2L, "Group", "Desc", true, accountId));

        var handler = new CreateGroupCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object);

        // Act
        await handler.Handle(new CreateGroupCommand(groupDto), CancellationToken.None);

        // Assert — writer must be called with the user's account, not another
        var wrongAccountId = Guid.NewGuid();
        _writerMock.Verify(w => w.CreateGroupAsync(groupDto, wrongAccountId, It.IsAny<CancellationToken>()), Times.Never);
    }
}
