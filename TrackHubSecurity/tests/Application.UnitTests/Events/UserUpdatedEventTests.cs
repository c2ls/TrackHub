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
public class UserUpdatedEventTests
{
    private Mock<IManagerWriter> _managerWriterMock;

    [SetUp]
    public void SetUp()
    {
        _managerWriterMock = new Mock<IManagerWriter>();
    }

    [Test]
    public async Task Handle_ValidNotification_CallsUpdateUserAsync()
    {
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserShrankDto(userId, "updated_user", true);
        var handler = new UserUpdated.Notification.EventHandler(_managerWriterMock.Object);

        await handler.Handle(new UserUpdated.Notification(userId, updateDto), CancellationToken.None);

        _managerWriterMock.Verify(w => w.UpdateUserAsync(userId, updateDto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_PassesBothIdAndDto()
    {
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserShrankDto(userId, "user", false);
        var handler = new UserUpdated.Notification.EventHandler(_managerWriterMock.Object);

        await handler.Handle(new UserUpdated.Notification(userId, updateDto), CancellationToken.None);

        _managerWriterMock.Verify(w => w.UpdateUserAsync(
            userId,
            It.Is<UpdateUserShrankDto>(d => d.Username == "user" && d.Active == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
