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

using TrackHub.Security.Application.Users.Events;

namespace Application.UnitTests.Events;

[TestFixture]
public class UserCreatedEventTests
{
    private Mock<IManagerWriter> _managerWriterMock;

    [SetUp]
    public void SetUp()
    {
        _managerWriterMock = new Mock<IManagerWriter>();
    }

    [Test]
    public async Task Handle_ValidNotification_CallsCreateUserAsync()
    {
        var user = new UserShrankDto(Guid.NewGuid(), "newuser", Guid.NewGuid());
        var notification = new UserCreated.Notification(user);
        var handler = new UserCreated.Notification.EventHandler(_managerWriterMock.Object);

        await handler.Handle(notification, CancellationToken.None);

        _managerWriterMock.Verify(w => w.CreateUserAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_PassesExactUserDto()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var user = new UserShrankDto(userId, "testuser", accountId);
        var handler = new UserCreated.Notification.EventHandler(_managerWriterMock.Object);

        await handler.Handle(new UserCreated.Notification(user), CancellationToken.None);

        _managerWriterMock.Verify(w => w.CreateUserAsync(
            It.Is<UserShrankDto>(u => u.UserId == userId && u.Username == "testuser" && u.AccountId == accountId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
