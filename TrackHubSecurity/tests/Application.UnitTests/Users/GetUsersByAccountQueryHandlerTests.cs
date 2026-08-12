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
using TrackHub.Security.Application.Users.Queries.GetByAccount;
using TrackHub.Security.Domain.Models;

namespace Application.UnitTests.Users;

[TestFixture]
internal class GetUsersByAccountQueryHandlerTests
{
    private static (int Skip, int Take, string? Search) Capture(GetUsersByAccountQuery query)
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var reader = new Mock<IUserReader>();
        reader.Setup(r => r.GetUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserVm { AccountId = accountId });

        (int Skip, int Take, string? Search) captured = default;
        reader.Setup(r => r.GetUsersAsync(accountId, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, int, int, string?, CancellationToken>((_, skip, take, search, _) => captured = (skip, take, search))
            .ReturnsAsync(new UsersPageVm([], 0));

        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId.ToString());

        var handler = new GetUsersByAccountQueryHandler(reader.Object, user.Object);
        handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();
        return captured;
    }

    [Test]
    public void Handle_ForwardsTheSearchTermToTheReader()
    {
        var captured = Capture(new GetUsersByAccountQuery(Skip: 10, Take: 10, Search: "ann"));

        Assert.That(captured.Search, Is.EqualTo("ann"));
    }

    [Test]
    public void Handle_ClampsAnUnboundedTakeToTheSharedCeiling()
    {
        var captured = Capture(new GetUsersByAccountQuery(Skip: 0, Take: 100_000, Search: null));

        // PageRequest.MaxPageSize is 500; a mutant that skipped the clamp would forward 100000.
        Assert.That(captured.Take, Is.EqualTo(500));
    }

    [Test]
    public void Handle_DefaultsAMissingWindowRatherThanRequestingEverything()
    {
        var captured = Capture(new GetUsersByAccountQuery());

        Assert.Multiple(() =>
        {
            Assert.That(captured.Skip, Is.EqualTo(0));
            Assert.That(captured.Take, Is.EqualTo(50));
            Assert.That(captured.Search, Is.Null);
        });
    }
}
