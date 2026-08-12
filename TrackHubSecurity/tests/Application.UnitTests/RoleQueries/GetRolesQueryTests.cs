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
using Moq;
using TrackHub.Security.Application.Roles.GetAll;
using TrackHub.Security.Domain.Interfaces;
using TrackHub.Security.Domain.Models;

namespace Application.UnitTests.RoleQueries;

/// <summary>
/// The Administrator exclusion used to run AFTER the fetch, over a materialized list. That is a
/// latent ragged-result bug — the moment this read gains a window, the filter removes a row from
/// inside the window and the caller silently receives a short page — and it also read back a row the
/// caller was never entitled to see. These tests pin the decision to the QUERY.
/// </summary>
[TestFixture]
public class GetRolesQueryTests
{
    private Mock<IRoleReader> _roleReader = null!;
    private Mock<IUserReader> _userReader = null!;
    private GetRolesQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _roleReader = new Mock<IRoleReader>();
        _userReader = new Mock<IUserReader>();

        var user = new Mock<IUser>();
        user.SetupGet(u => u.Id).Returns(Guid.NewGuid().ToString());

        _roleReader
            .Setup(r => r.GetRolesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new RoleVm(1, "Manager")]);

        _handler = new GetRolesQueryHandler(_roleReader.Object, _userReader.Object, user.Object);
    }

    [Test]
    public async Task NonAdminCaller_AsksTheReaderToExcludeAdministrator()
    {
        _userReader.Setup(u => u.IsAdminAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await _handler.Handle(new GetRolesQuery(), CancellationToken.None);

        // includeAdministrator: false — the row is never fetched, not fetched and discarded.
        _roleReader.Verify(r => r.GetRolesAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AdminCaller_AsksTheReaderToIncludeAdministrator()
    {
        _userReader.Setup(u => u.IsAdminAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _handler.Handle(new GetRolesQuery(), CancellationToken.None);

        _roleReader.Verify(r => r.GetRolesAsync(true, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Whatever the reader returns is what the caller gets: no post-fetch filtering may creep back in.
    [Test]
    public async Task ReturnsTheReaderResultUnfiltered()
    {
        _userReader.Setup(u => u.IsAdminAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _roleReader
            .Setup(r => r.GetRolesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new RoleVm(1, "Administrator"), new RoleVm(2, "Manager")]);

        var result = await _handler.Handle(new GetRolesQuery(), CancellationToken.None);

        Assert.That(result.Select(r => r.Name), Is.EqualTo(new[] { "Administrator", "Manager" }));
    }
}
