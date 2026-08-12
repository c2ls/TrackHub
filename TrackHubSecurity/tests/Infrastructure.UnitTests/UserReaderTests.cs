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
using Microsoft.EntityFrameworkCore;
using TrackHub.Security.Infrastructure;
using TrackHub.Security.Infrastructure.Entities;
using TrackHub.Security.Infrastructure.Readers;

namespace Infrastructure.UnitTests;

/// <summary>
/// <c>usersByAccount</c> now returns the paged envelope (items + unpaged total) that the users
/// screen needs for an exact "showing X–Y of Z" and a real search box. These tests pin the total,
/// the tiebreaker that keeps the page window stable across tied usernames, and account isolation.
/// The <c>ILike</c> search itself is a PostgreSQL function with no in-memory implementation, so the
/// forwarding of the search term is asserted at the handler level instead.
/// </summary>
[TestFixture]
internal class UserReaderTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    // A global service identity: account-transparent, so the paging assertions are undisturbed
    // by the reader's by-id account guard.
    private static ICurrentPrincipal ServicePrincipal()
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.ServiceClient);
        principal.SetupGet(p => p.AccountId).Returns((Guid?)null);
        return principal.Object;
    }

    private static User NewUser(Guid accountId, string username)
        => new(username, "pw", $"{username}@mail.com", "first", null, "last", null, null, true, 0, accountId);

    [Test]
    public async Task GetUsersAsync_ReportsTheUnpagedTotalNotThePageLength()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetUsersAsync_ReportsTheUnpagedTotalNotThePageLength));

        await context.Users.AddRangeAsync(Enumerable.Range(0, 7).Select(i => NewUser(accountId, $"user-{i:00}")));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new UserReader(context, ServicePrincipal());
        var page = await reader.GetUsersAsync(accountId, 0, 3, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(page.Items, Has.Count.EqualTo(3));
            Assert.That(page.TotalCount, Is.EqualTo(7));
        });
    }

    [Test]
    public async Task GetUsersAsync_ScopesToTheAccount()
    {
        var accountId = Guid.NewGuid();
        var otherAccount = Guid.NewGuid();
        await using var context = NewContext(nameof(GetUsersAsync_ScopesToTheAccount));

        await context.Users.AddAsync(NewUser(accountId, "mine"));
        await context.Users.AddAsync(NewUser(otherAccount, "theirs"));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new UserReader(context, ServicePrincipal());
        var page = await reader.GetUsersAsync(accountId, 0, 50, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(page.TotalCount, Is.EqualTo(1));
            Assert.That(page.Items.Single().Username, Is.EqualTo("mine"));
        });
    }

    [Test]
    public async Task GetUsersAsync_PagesTiedUsernamesWithoutLosingRows()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetUsersAsync_PagesTiedUsernamesWithoutLosingRows));

        var users = Enumerable.Range(0, 7).Select(_ => NewUser(accountId, "same")).ToList();
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new UserReader(context, ServicePrincipal());
        var seen = new List<Guid>();
        for (var skip = 0; ; skip += 2)
        {
            var page = await reader.GetUsersAsync(accountId, skip, 2, null, CancellationToken.None);
            seen.AddRange(page.Items.Select(u => u.UserId));
            if (page.Items.Count == 0 || seen.Count >= page.TotalCount)
            {
                break;
            }
        }

        Assert.Multiple(() =>
        {
            Assert.That(seen, Is.Unique, "a page boundary returned the same user twice");
            Assert.That(seen, Is.EquivalentTo(users.Select(u => u.UserId)), "paging lost or invented users");
        });
    }
}
