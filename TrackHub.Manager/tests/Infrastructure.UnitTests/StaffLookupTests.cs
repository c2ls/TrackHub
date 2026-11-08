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
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;

namespace Infrastructure.UnitTests;

// The trip board's picker: names and ids only, active staff on search, any named staff by id.
[TestFixture]
public class StaffLookupTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

    private static StaffMemberReader Reader(ApplicationDbContext context, Guid accountId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        return new StaffMemberReader(context as IApplicationDbContext, principal.Object);
    }

    private static StaffMember Seed(Guid accountId, string name, bool active)
        => new(accountId, name, "+57300000000", "CC", "1", active, null, null, null, null);

    [Test]
    public async Task Search_ReturnsActiveStaffOnly_ByIds_ReturnsTheNamedOnesEvenWhenInactive()
    {
        await using var context = NewContext(nameof(Search_ReturnsActiveStaffOnly_ByIds_ReturnsTheNamedOnesEvenWhenInactive));
        var accountId = Guid.NewGuid();
        var ana = Seed(accountId, "Ana Ruiz", true);
        var retired = Seed(accountId, "Andres Retired", false);
        var other = Seed(Guid.NewGuid(), "Ana Other", true);
        await context.StaffMembers.AddRangeAsync(ana, retired, other);
        await context.SaveChangesAsync(CancellationToken.None);
        var reader = Reader(context, accountId);

        var search = await reader.GetStaffLookupAsync(accountId, null, null, 50, CancellationToken.None);
        var byIds = await reader.GetStaffLookupAsync(accountId, null, [retired.StaffId, other.StaffId], 50, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(search.Select(d => d.StaffId), Is.EquivalentTo(new[] { ana.StaffId }), "the picker feed offers the active staff of the caller's account");
            Assert.That(byIds.Select(d => d.StaffId), Is.EquivalentTo(new[] { retired.StaffId }), "the board names an inactive staff but never another account's");
            Assert.That(byIds.Single().Active, Is.False);
        });
    }
}
