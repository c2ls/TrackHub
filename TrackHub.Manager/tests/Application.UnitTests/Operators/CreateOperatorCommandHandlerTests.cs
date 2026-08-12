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
using Common.Domain.Enums;
using TrackHub.Manager.Application.Operators.Commands.Create;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Operators;

[TestFixture]
public class CreateOperatorCommandHandlerTests
{
    private Mock<IOperatorWriter> _writerMock;
    private Mock<IUserReader> _userReaderMock;
    private Mock<IUser> _userMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IOperatorWriter>();
        _userReaderMock = new Mock<IUserReader>();
        _userMock = new Mock<IUser>();
    }

    [Test]
    public async Task Handle_ValidCommand_LooksUpUserAccountAndCreatesOperator()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var operatorDto = new OperatorDto("TestOperator", "Desc", "+1234", "test@test.com", "Addr", "Contact", 1);
        var expectedVm = new OperatorVm(Guid.NewGuid(), "TestOperator", "Desc", "+1234", "test@test.com", "Addr", "Contact",
            ProtocolType.CommandTrack, 1, accountId, DateTimeOffset.UtcNow, null,
            true, 60, OperatorHealthStatus.Unknown, null, null, null, null, null, null, null, null);

        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userReaderMock.Setup(r => r.GetUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserVm(userId, "testuser", true, accountId));
        _writerMock.Setup(w => w.CreateOperatorAsync(operatorDto, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVm);

        var handler = new CreateOperatorCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object);
        var command = new CreateOperatorCommand(operatorDto);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedVm));
        _userReaderMock.Verify(r => r.GetUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.CreateOperatorAsync(operatorDto, accountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidCommand_PropagatesCorrectAccountId()
    {
        // Arrange — verify the accountId from user lookup is forwarded, not some other ID
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var differentAccountId = Guid.NewGuid();
        var operatorDto = new OperatorDto("Op", null, null, null, null, null, 2);

        _userMock.Setup(u => u.Id).Returns(userId.ToString());
        _userReaderMock.Setup(r => r.GetUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserVm(userId, "user", true, accountId));
        _writerMock.Setup(w => w.CreateOperatorAsync(operatorDto, accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperatorVm(Guid.NewGuid(), "Op", null, null, null, null, null, ProtocolType.Traccar, 2, accountId, DateTimeOffset.UtcNow, null, true, 60, OperatorHealthStatus.Unknown, null, null, null, null, null, null, null, null));

        var handler = new CreateOperatorCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object);

        // Act
        await handler.Handle(new CreateOperatorCommand(operatorDto), CancellationToken.None);

        // Assert — writer must NOT be called with differentAccountId
        _writerMock.Verify(w => w.CreateOperatorAsync(operatorDto, differentAccountId, It.IsAny<CancellationToken>()), Times.Never);
        _writerMock.Verify(w => w.CreateOperatorAsync(operatorDto, accountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Constructor_NullUserId_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _userMock.Setup(u => u.Id).Returns((string?)null);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() =>
            new CreateOperatorCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object));
    }
}
