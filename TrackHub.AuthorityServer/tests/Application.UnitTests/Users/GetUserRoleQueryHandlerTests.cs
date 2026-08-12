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

using TrackHub.AuthorityServer.Application.Users.Queries.GetUserRole;
using TrackHub.AuthorityServer.Domain.Interfaces;

namespace TrackHub.AuthorityServer.Application.UnitTests.Users;

// The login flow resolves the user's role through this query so the
// access token can carry it (before, user tokens had no role claim and every IsPrivileged
// check on the resource services silently fell back to non-privileged).
[TestFixture]
public class GetUserRoleQueryHandlerTests
{
    [Test]
    public async Task Handle_UserWithRole_ReturnsTheRoleName()
    {
        var userId = Guid.NewGuid();
        var reader = new Mock<IUserReader>();
        reader.Setup(r => r.GetUserRoleAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync("Administrator");
        var handler = new GetUserRoleQueryHandler(reader.Object);

        var role = await handler.Handle(new GetUserRoleQuery(userId), CancellationToken.None);

        Assert.That(role, Is.EqualTo("Administrator"));
    }

    [Test]
    public async Task Handle_UserWithoutRole_ReturnsNull()
    {
        var reader = new Mock<IUserReader>();
        reader.Setup(r => r.GetUserRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);
        var handler = new GetUserRoleQueryHandler(reader.Object);

        var role = await handler.Handle(new GetUserRoleQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.That(role, Is.Null);
    }
}
