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
using TrackHub.Security.Application.Identity.Queries.Authorize;

namespace Application.UnitTests.Identity;

[TestFixture]
public class AuthorizeQueryHandlerTests
{
    private Mock<IIdentityService> _serviceMock;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IIdentityService>();
    }

    [Test]
    public async Task Handle_UserIsAuthorized_ReturnsTrue()
    {
        var userId = Guid.NewGuid();
        _serviceMock.Setup(s => s.AuthorizeAsync(userId, "Operators", "Read", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.User(userId).Object);
        var result = await handler.Handle(new AuthorizeQuery(userId, "Operators", "Read"), CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Handle_UserIsNotAuthorized_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        _serviceMock.Setup(s => s.AuthorizeAsync(userId, "Admin", "Write", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.User(userId).Object);
        var result = await handler.Handle(new AuthorizeQuery(userId, "Admin", "Write"), CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Handle_PassesCorrectParametersToService()
    {
        var userId = Guid.NewGuid();
        var handler = new GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.Service().Object);

        await handler.Handle(new AuthorizeQuery(userId, "Resource", "Action"), CancellationToken.None);

        _serviceMock.Verify(s => s.AuthorizeAsync(userId, "Resource", "Action", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Handle_UserAskingAboutAnotherUser_ThrowsForbidden()
    {
        var handler = new GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.User(Guid.NewGuid()).Object);

        Assert.ThrowsAsync<Common.Application.Exceptions.ForbiddenAccessException>(
            () => handler.Handle(new AuthorizeQuery(Guid.NewGuid(), "Resource", "Action"), CancellationToken.None));
        _serviceMock.Verify(s => s.AuthorizeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
