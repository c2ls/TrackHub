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
using TrackHub.Manager.Application.Transporters.Commands.Create;
using TrackHub.Manager.Domain.Interfaces;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Domain.Records;

namespace Application.UnitTests.Transporters;

[TestFixture]
public class CreateTransporterCommandHandlerTests
{
    private Mock<ITransporterWriter> _writerMock;
    private Mock<IUserReader> _userReaderMock;
    private Mock<IUser> _userMock;
    private Guid _userId;
    private Guid _accountId;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<ITransporterWriter>();
        _userReaderMock = new Mock<IUserReader>();
        _userMock = new Mock<IUser>();
        _userId = Guid.NewGuid();
        _accountId = Guid.NewGuid();

        _userMock.Setup(u => u.Id).Returns(_userId.ToString());
        _userReaderMock.Setup(r => r.GetUserAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserVm(_userId, "testuser", true, _accountId));
    }

    [Test]
    public async Task Handle_ValidCommand_DelegatesToWriterWithAccountId()
    {
        // Arrange
        var dto = new TransporterDto("Truck-001", 1, Guid.Empty);
        var expectedVm = new TransporterVm(Guid.NewGuid(), "Truck-001", TransporterType.Truck, 1);
        _writerMock.Setup(w => w.CreateTransporterAsync(
                It.Is<TransporterDto>(d => d.AccountId == _accountId && d.Name == "Truck-001"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVm);

        var handler = new CreateTransporterCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object);

        // Act
        var result = await handler.Handle(new CreateTransporterCommand(dto), CancellationToken.None);

        // Assert
        Assert.That(result, Is.EqualTo(expectedVm));
        _writerMock.Verify(w => w.CreateTransporterAsync(
            It.Is<TransporterDto>(d => d.AccountId == _accountId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_OverridesAccountIdFromUserContext()
    {
        // Arrange — DTO has Guid.Empty for AccountId (from GraphQL input)
        var dto = new TransporterDto("Asset-X", 3, Guid.Empty);
        var expectedVm = new TransporterVm(Guid.NewGuid(), "Asset-X", TransporterType.Bicycle, 3);
        _writerMock.Setup(w => w.CreateTransporterAsync(It.IsAny<TransporterDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVm);

        var handler = new CreateTransporterCommandHandler(_writerMock.Object, _userReaderMock.Object, _userMock.Object);

        // Act
        var result = await handler.Handle(new CreateTransporterCommand(dto), CancellationToken.None);

        // Assert
        Assert.That(result.TransporterTypeId, Is.EqualTo(3));
        Assert.That(result.Name, Is.EqualTo("Asset-X"));
        _writerMock.Verify(w => w.CreateTransporterAsync(
            It.Is<TransporterDto>(d => d.AccountId == _accountId && d.Name == "Asset-X" && d.TransporterTypeId == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_NullUserId_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var nullUserMock = new Mock<IUser>();
        nullUserMock.Setup(u => u.Id).Returns((string?)null);

        // Act & Assert
        Assert.Throws<UnauthorizedAccessException>(() =>
            new CreateTransporterCommandHandler(_writerMock.Object, _userReaderMock.Object, nullUserMock.Object));
    }
}
