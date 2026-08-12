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

using Common.Application.Exceptions;
using Common.Application.Interfaces;
using TrackHub.Security.Application.Identity.Queries.AuthorizeUser;

namespace Application.UnitTests.Identity;

[TestFixture]
public class AuthorizeUserQueryHandlerTests
{
    private Mock<IIdentityService> _serviceMock;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IIdentityService>();
    }

    [Test]
    public async Task Handle_SelfSubject_ReturnsCombinedDecision()
    {
        var userId = Guid.NewGuid();
        _serviceMock.Setup(s => s.AuthorizeUserAsync(userId, "Operators", "Read", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new AuthorizeUserQueryHandler(_serviceMock.Object, IdentityTestCallers.User(userId).Object);
        var result = await handler.Handle(new AuthorizeUserQuery(userId, "Operators", "Read"), CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Handle_ServiceCaller_MayQueryAnyUser()
    {
        var userId = Guid.NewGuid();
        _serviceMock.Setup(s => s.AuthorizeUserAsync(userId, "Operators", "Read", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new AuthorizeUserQueryHandler(_serviceMock.Object, IdentityTestCallers.Service().Object);
        var result = await handler.Handle(new AuthorizeUserQuery(userId, "Operators", "Read"), CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Handle_UserAskingAboutAnotherUser_ThrowsForbidden()
    {
        var handler = new AuthorizeUserQueryHandler(_serviceMock.Object, IdentityTestCallers.User(Guid.NewGuid()).Object);

        Assert.ThrowsAsync<ForbiddenAccessException>(
            () => handler.Handle(new AuthorizeUserQuery(Guid.NewGuid(), "Operators", "Read"), CancellationToken.None));
        _serviceMock.Verify(s => s.AuthorizeUserAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_AnonymousCaller_ThrowsUnauthorized()
    {
        var anonymous = new Mock<IUser>();
        anonymous.Setup(u => u.Id).Returns((string?)null);

        var handler = new AuthorizeUserQueryHandler(_serviceMock.Object, anonymous.Object);

        Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new AuthorizeUserQuery(Guid.NewGuid(), "Operators", "Read"), CancellationToken.None));
    }
}
