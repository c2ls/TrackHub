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
using Common.Domain.Enums;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using Moq;

namespace Infrastructure.UnitTests;

/// <summary>
/// Every paged reader is ordered by a NON-UNIQUE display name. Name alone is not a total order, so
/// rows that tie can be returned in any order per query — which makes a Skip/Take boundary repeat one
/// row on page 2 and drop another entirely. That loss is silent: the caller sees a full page.
/// <para>
/// Each test here seeds rows that all share one name — the worst case for the tiebreaker — walks the
/// whole set one page at a time, and asserts the union is exactly the seeded set with no duplicates.
/// </para>
/// </summary>
[TestFixture]
public class PagedReaderOrderingTests
{
    private const int PageSize = 2;
    private const int RowCount = 7;
    private const string TiedName = "same";

    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal Principal(Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        return principal.Object;
    }

    /// <summary>Walks every page and returns the concatenated keys.</summary>
    private static async Task<List<TKey>> DrainAsync<TKey>(
        Func<int, int, Task<(IReadOnlyCollection<TKey> Keys, int TotalCount)>> fetchPage)
    {
        var seen = new List<TKey>();
        var skip = 0;
        while (true)
        {
            var (keys, totalCount) = await fetchPage(skip, PageSize);
            seen.AddRange(keys);
            skip += PageSize;
            if (keys.Count == 0 || seen.Count >= totalCount)
            {
                return seen;
            }
        }
    }

    private static void AssertCompleteAndDistinct<TKey>(List<TKey> seen, IEnumerable<TKey> expected)
    {
        Assert.Multiple(() =>
        {
            Assert.That(seen, Is.Unique, "a page boundary returned the same row twice");
            Assert.That(seen, Is.EquivalentTo(expected), "paging through the set lost or invented rows");
        });
    }

    [Test]
    public async Task TransportersByAccount_PagesWithoutLosingTiedNames()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(TransportersByAccount_PagesWithoutLosingTiedNames));

        var transporters = Enumerable.Range(0, RowCount)
            .Select(_ => new TrackHub.Manager.Infrastructure.Entities.Transporter(TiedName, 1, accountId))
            .ToList();
        await context.Transporters.AddRangeAsync(transporters);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new TransporterReader(context, Principal(accountId));
        var seen = await DrainAsync<Guid>(async (skip, take) =>
        {
            var page = await reader.GetTransportersByAccountAsync(accountId, skip, take, null, CancellationToken.None);
            return ([.. page.Items.Select(t => t.TransporterId)], page.TotalCount);
        });

        AssertCompleteAndDistinct(seen, transporters.Select(t => t.TransporterId));
    }

    // The by-group and by-user reads project through the same shared row type and a Distinct. Cover
    // them explicitly so a translation break there cannot hide behind the by-account test.
    [Test]
    public async Task TransportersByGroup_PagesTheGroupMembership()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(TransportersByGroup_PagesTheGroupMembership));

        var group = new TrackHub.Manager.Infrastructure.Entities.Group("g", "d", true, accountId);
        await context.Groups.AddAsync(group);
        await context.SaveChangesAsync(CancellationToken.None);

        var transporters = Enumerable.Range(0, RowCount)
            .Select(_ => new TrackHub.Manager.Infrastructure.Entities.Transporter(TiedName, 1, accountId))
            .ToList();
        foreach (var transporter in transporters)
        {
            group.Transporters.Add(transporter);
        }
        await context.Transporters.AddRangeAsync(transporters);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new TransporterReader(context, Principal(accountId));
        var seen = await DrainAsync<Guid>(async (skip, take) =>
        {
            var page = await reader.GetTransportersByGroupAsync(group.GroupId, skip, take, null, CancellationToken.None);
            return ([.. page.Items.Select(t => t.TransporterId)], page.TotalCount);
        });

        AssertCompleteAndDistinct(seen, transporters.Select(t => t.TransporterId));
    }

    [Test]
    public async Task TransportersByUser_PagesTheUsersVisibleSet()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var context = NewContext(nameof(TransportersByUser_PagesTheUsersVisibleSet));

        var group = new TrackHub.Manager.Infrastructure.Entities.Group("g", "d", true, accountId);
        var user = new TrackHub.Manager.Infrastructure.Entities.User(userId, "alice", true, accountId);
        group.Users.Add(user);
        await context.Groups.AddAsync(group);
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync(CancellationToken.None);

        var transporters = Enumerable.Range(0, RowCount)
            .Select(_ => new TrackHub.Manager.Infrastructure.Entities.Transporter(TiedName, 1, accountId))
            .ToList();
        foreach (var transporter in transporters)
        {
            group.Transporters.Add(transporter);
        }
        await context.Transporters.AddRangeAsync(transporters);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new TransporterReader(context, Principal(accountId));
        var seen = await DrainAsync<Guid>(async (skip, take) =>
        {
            var page = await reader.GetTransportersByUserAsync(userId, skip, take, null, CancellationToken.None);
            return ([.. page.Items.Select(t => t.TransporterId)], page.TotalCount);
        });

        AssertCompleteAndDistinct(seen, transporters.Select(t => t.TransporterId));
    }

    // The lookup feeds are the picker path: unpaged by design, so they must return the WHOLE set.
    [Test]
    public async Task TransporterLookupByAccount_ReturnsEveryTransporterUnpaged()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(TransporterLookupByAccount_ReturnsEveryTransporterUnpaged));

        var transporters = Enumerable.Range(0, RowCount)
            .Select(_ => new TrackHub.Manager.Infrastructure.Entities.Transporter(TiedName, 1, accountId))
            .ToList();
        await context.Transporters.AddRangeAsync(transporters);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new TransporterReader(context, Principal(accountId));
        var lookup = await reader.GetTransporterLookupByAccountAsync(accountId, 5_001, CancellationToken.None);

        Assert.That(lookup.Select(t => t.TransporterId), Is.EquivalentTo(transporters.Select(t => t.TransporterId)));
    }

    [Test]
    public async Task GroupsByAccount_PagesWithoutLosingTiedNames()
    {
        await using var context = NewContext(nameof(GroupsByAccount_PagesWithoutLosingTiedNames));

        var accountId = Guid.NewGuid();
        var groups = Enumerable.Range(0, RowCount)
            .Select(_ => new TrackHub.Manager.Infrastructure.Entities.Group(TiedName, "d", true, accountId))
            .ToList();
        await context.Groups.AddRangeAsync(groups);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new GroupReader(context, Principal(accountId));
        var seen = await DrainAsync<long>(async (skip, take) =>
        {
            var page = await reader.GetGroupsByAccountAsync(accountId, skip, take, null, CancellationToken.None);
            return ([.. page.Items.Select(g => g.GroupId)], page.TotalCount);
        });

        AssertCompleteAndDistinct(seen, groups.Select(g => g.GroupId));
    }

    [Test]
    public async Task UsersByGroup_PagesWithoutLosingTiedUsernames()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(UsersByGroup_PagesWithoutLosingTiedUsernames));

        var group = new TrackHub.Manager.Infrastructure.Entities.Group("g", "d", true, accountId);
        await context.Groups.AddAsync(group);
        await context.SaveChangesAsync(CancellationToken.None);

        var users = Enumerable.Range(0, RowCount)
            .Select(_ => new TrackHub.Manager.Infrastructure.Entities.User(Guid.NewGuid(), TiedName, true, accountId))
            .ToList();
        foreach (var user in users)
        {
            group.Users.Add(user);
        }
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new UserReader(context, Principal(accountId));
        var seen = await DrainAsync<Guid>(async (skip, take) =>
        {
            var page = await reader.GetUsersByGroupAsync(group.GroupId, skip, take, null, CancellationToken.None);
            return ([.. page.Items.Select(u => u.UserId)], page.TotalCount);
        });

        AssertCompleteAndDistinct(seen, users.Select(u => u.UserId));
    }

    [Test]
    public async Task PointsOfInterestByAccount_PagesWithoutLosingTiedNames()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(PointsOfInterestByAccount_PagesWithoutLosingTiedNames));

        var points = Enumerable.Range(0, RowCount)
            .Select(_ => new TrackHub.Manager.Infrastructure.Entities.PointOfInterest(
                TiedName, null, 1, 10d, 20d, null, null, null, true, accountId))
            .ToList();
        await context.PointsOfInterest.AddRangeAsync(points);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new PointOfInterestReader(context, Principal(accountId));
        var seen = await DrainAsync<Guid>(async (skip, take) =>
        {
            var page = await reader.GetPointsOfInterestByAccountAsync(accountId, null, skip, take, null, CancellationToken.None);
            return ([.. page.Items.Select(p => p.PointOfInterestId)], page.TotalCount);
        });

        AssertCompleteAndDistinct(seen, points.Select(p => p.PointOfInterestId));
    }

    [Test]
    public async Task Accounts_PageWithoutLosingTiedNames()
    {
        await using var context = NewContext(nameof(Accounts_PageWithoutLosingTiedNames));

        var accounts = Enumerable.Range(0, RowCount)
            .Select(_ => new TrackHub.Manager.Infrastructure.Entities.Account(TiedName, "d", 1, true))
            .ToList();
        await context.Accounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new AccountReader(context, Principal(Guid.NewGuid()));
        var seen = await DrainAsync<Guid>(async (skip, take) =>
        {
            var page = await reader.GetAccountsAsync(skip, take, null, CancellationToken.None);
            return ([.. page.Items.Select(a => a.AccountId)], page.TotalCount);
        });

        AssertCompleteAndDistinct(seen, accounts.Select(a => a.AccountId));
    }
}
