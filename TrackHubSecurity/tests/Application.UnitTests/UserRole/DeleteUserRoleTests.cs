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

using TrackHub.Security.Application.UserRole.Commands.Delete;

namespace Application.UnitTests.UserRole;

[TestFixture]
public class DeleteUserRoleCommandHandlerTests
{
    private Mock<IUserRoleWriter> _writerMock;

    [SetUp]
    public void SetUp()
    {
        _writerMock = new Mock<IUserRoleWriter>();
    }

    [Test]
    public async Task Handle_ValidCommand_DelegatesToWriter()
    {
        var userId = Guid.NewGuid();
        var roleId = 4;
        var handler = new DeleteUserRoleCommandHandler(_writerMock.Object, new Mock<Common.Mediator.IPublisher>().Object, new Mock<Common.Application.Interfaces.ICurrentPrincipal>().Object);

        await handler.Handle(new DeleteUserRoleCommand(userId, roleId), CancellationToken.None);

        _writerMock.Verify(w => w.DeleteUserRoleAsync(userId, roleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DifferentRoleId_PassesCorrectValues()
    {
        var userId = Guid.NewGuid();
        var handler = new DeleteUserRoleCommandHandler(_writerMock.Object, new Mock<Common.Mediator.IPublisher>().Object, new Mock<Common.Application.Interfaces.ICurrentPrincipal>().Object);

        await handler.Handle(new DeleteUserRoleCommand(userId, 9), CancellationToken.None);

        _writerMock.Verify(w => w.DeleteUserRoleAsync(userId, 9, It.IsAny<CancellationToken>()), Times.Once);
        _writerMock.Verify(w => w.DeleteUserRoleAsync(userId, 4, It.IsAny<CancellationToken>()), Times.Never);
    }
}
