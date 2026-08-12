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
using TrackHub.Security.Application.Identity.Queries.IsValidService;

namespace Application.UnitTests.Identity;

[TestFixture]
public class IsValidServiceQueryHandlerTests
{
    private Mock<IIdentityService> _serviceMock;

    [SetUp]
    public void SetUp()
    {
        _serviceMock = new Mock<IIdentityService>();
    }

    [Test]
    public async Task Handle_ValidClient_ReturnsTrue()
    {
        _serviceMock.Setup(s => s.IsValidServiceAsync("my-client", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new TrackHub.Security.Application.Identity.Queries.IsValidService.GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.Service("my-client").Object);
        var result = await handler.Handle(new IsValidServiceQuery("my-client"), CancellationToken.None);

        Assert.That(result, Is.True);
    }

    [Test]
    public async Task Handle_UnknownClient_ReturnsFalse()
    {
        _serviceMock.Setup(s => s.IsValidServiceAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new TrackHub.Security.Application.Identity.Queries.IsValidService.GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.Service("unknown").Object);
        var result = await handler.Handle(new IsValidServiceQuery("unknown"), CancellationToken.None);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Handle_ServiceAskingAboutAnotherClient_ThrowsForbidden()
    {
        var handler = new TrackHub.Security.Application.Identity.Queries.IsValidService.GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.Service("my-client").Object);

        Assert.ThrowsAsync<Common.Application.Exceptions.ForbiddenAccessException>(
            () => handler.Handle(new IsValidServiceQuery("other-client"), CancellationToken.None));
        _serviceMock.Verify(s => s.IsValidServiceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public void Handle_UserPrincipal_ThrowsForbidden()
    {
        var handler = new TrackHub.Security.Application.Identity.Queries.IsValidService.GetUsersQueryHandler(_serviceMock.Object, IdentityTestCallers.User(Guid.NewGuid()).Object);

        Assert.ThrowsAsync<Common.Application.Exceptions.ForbiddenAccessException>(
            () => handler.Handle(new IsValidServiceQuery("my-client"), CancellationToken.None));
    }
}
