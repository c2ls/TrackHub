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

using Common.Application.Interfaces;
using TrackHub.Security.Application.Identity.Queries.GetUsername;

namespace Application.UnitTests.Identity;

[TestFixture]
public class GetUserNameQueryHandlerTests
{
    private Mock<IIdentityService> _serviceMock;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IIdentityService>();
    }

    [Test]
    public async Task Handle_ValidUserId_ReturnsUsername()
    {
        var userId = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetUserNameAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("john.doe");

        var handler = new TrackHub.Security.Application.Identity.Queries.GetUsername.GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.User(userId).Object);
        var result = await handler.Handle(new GetUserNameQuery(userId), CancellationToken.None);

        Assert.That(result, Is.EqualTo("john.doe"));
    }

    [Test]
    public async Task Handle_PassesCorrectUserId()
    {
        var userId = Guid.NewGuid();
        var handler = new TrackHub.Security.Application.Identity.Queries.GetUsername.GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.Service().Object);

        await handler.Handle(new GetUserNameQuery(userId), CancellationToken.None);

        _serviceMock.Verify(s => s.GetUserNameAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_UserAskingAboutAnotherUser_ThrowsForbidden()
    {
        var handler = new TrackHub.Security.Application.Identity.Queries.GetUsername.GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.User(Guid.NewGuid()).Object);

        Assert.ThrowsAsync<Common.Application.Exceptions.ForbiddenAccessException>(
            () => handler.Handle(new GetUserNameQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
