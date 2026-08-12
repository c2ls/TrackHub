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

using Common.Domain.Constants;
using TrackHub.Security.Application.Users.Events;

namespace Application.UnitTests.Events;

[TestFixture]
public class ManagerCreatedEventTests
{
    private Mock<IUserRoleWriter> _userRoleWriterMock;
    private Mock<IRoleReader> _roleReaderMock;

    [SetUp]
    public void SetUp()
    {
        _userRoleWriterMock = new Mock<IUserRoleWriter>();
        _roleReaderMock = new Mock<IRoleReader>();
    }

    [Test]
    public async Task Handle_ValidNotification_GetsManagerRoleAndCreatesUserRole()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = 42;
        var managerRole = new RoleVm(roleId, Roles.Manager);

        _roleReaderMock.Setup(r => r.GetRoleAsync(Roles.Manager, It.IsAny<CancellationToken>()))
            .ReturnsAsync(managerRole);

        var handler = new ManagerCreated.Notification.EventHandler(_userRoleWriterMock.Object, _roleReaderMock.Object);

        // Act
        await handler.Handle(new ManagerCreated.Notification(userId), CancellationToken.None);

        // Assert — must look up Manager role and create the assignment
        _roleReaderMock.Verify(r => r.GetRoleAsync(Roles.Manager, It.IsAny<CancellationToken>()), Times.Once);
        _userRoleWriterMock.Verify(w => w.CreateUserRoleAsync(
            It.Is<UserRoleDto>(dto => dto.UserId == userId && dto.RoleId == roleId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_UsesRoleIdFromReader_NotHardcoded()
    {
        // Arrange — roleId from DB differs from any hardcoded value
        var userId = Guid.NewGuid();
        var dynamicRoleId = 99;

        _roleReaderMock.Setup(r => r.GetRoleAsync(Roles.Manager, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RoleVm(dynamicRoleId, Roles.Manager));

        var handler = new ManagerCreated.Notification.EventHandler(_userRoleWriterMock.Object, _roleReaderMock.Object);

        // Act
        await handler.Handle(new ManagerCreated.Notification(userId), CancellationToken.None);

        // Assert
        _userRoleWriterMock.Verify(w => w.CreateUserRoleAsync(
            It.Is<UserRoleDto>(dto => dto.RoleId == dynamicRoleId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
