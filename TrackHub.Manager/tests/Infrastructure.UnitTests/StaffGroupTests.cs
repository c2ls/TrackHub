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
using Common.Domain.Constants;
using Moq;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure.Writers;

namespace Infrastructure.UnitTests;

[TestFixture]
public class StaffGroupTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

    private static ICurrentPrincipal Principal(Guid accountId, Guid userId, string? role = null)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(p => p.UserId).Returns(userId);
        principal.SetupGet(p => p.Role).Returns(role);
        return principal.Object;
    }

    private sealed record Seed(Guid AccountId, Guid UserId, StaffMember Active, StaffMember Inactive, long GroupId, long OtherGroupId);

    private static async Task<Seed> SeedAsync(ApplicationDbContext context)
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var active = new StaffMember(accountId, "Ana", null, null, null, true, null, null, null, null, StaffTypes.Administrative);
        var inactive = new StaffMember(accountId, "Old", null, null, null, false, null, null, null, null);
        var group = new Group("office", "", true, accountId);
        var other = new Group("yard", "", true, accountId);
        await context.Users.AddAsync(new User(userId, "alice", true, accountId));
        await context.StaffMembers.AddRangeAsync(active, inactive);
        await context.Groups.AddRangeAsync(group, other);
        await context.SaveChangesAsync(CancellationToken.None);
        await context.UsersGroup.AddAsync(new UserGroup { UserId = userId, GroupId = group.GroupId });
        await context.SaveChangesAsync(CancellationToken.None);
        context.ChangeTracker.Clear();
        return new Seed(accountId, userId, active, inactive, group.GroupId, other.GroupId);
    }

    [Test]
    public async Task Create_ThenDelete_RecordsBothAndAudits()
    {
        await using var context = NewContext(nameof(Create_ThenDelete_RecordsBothAndAudits));
        var seed = await SeedAsync(context);
        var writer = new StaffGroupWriter(context as IApplicationDbContext, Principal(seed.AccountId, seed.UserId, Roles.Manager));

        await writer.CreateStaffGroupAsync(new StaffGroupDto(seed.Active.StaffId, seed.GroupId), CancellationToken.None);
        context.ChangeTracker.Clear();
        var joined = await context.StaffGroups.CountAsync();
        await writer.DeleteStaffGroupAsync(seed.Active.StaffId, seed.GroupId, CancellationToken.None);

        Assert.Multiple(async () =>
        {
            Assert.That(joined, Is.EqualTo(1));
            Assert.That(await context.StaffGroups.CountAsync(), Is.Zero);
            Assert.That(await context.AuditEvents.CountAsync(a => a.Action == "CreateStaffGroup"), Is.EqualTo(1));
            Assert.That(await context.AuditEvents.CountAsync(a => a.Action == "DeleteStaffGroup"), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task InactiveStaff_OrForeignGroup_CannotJoin()
    {
        await using var context = NewContext(nameof(InactiveStaff_OrForeignGroup_CannotJoin));
        var seed = await SeedAsync(context);
        var foreign = new Group("foreign", "", true, Guid.NewGuid());
        await context.Groups.AddAsync(foreign);
        await context.SaveChangesAsync(CancellationToken.None);
        var writer = new StaffGroupWriter(context as IApplicationDbContext, Principal(seed.AccountId, seed.UserId, Roles.Manager));

        Assert.Multiple(async () =>
        {
            await Assert.ThrowsAsync<NotFoundException>(() => writer.CreateStaffGroupAsync(new StaffGroupDto(seed.Inactive.StaffId, seed.GroupId), CancellationToken.None));
            await Assert.ThrowsAsync<NotFoundException>(() => writer.CreateStaffGroupAsync(new StaffGroupDto(seed.Active.StaffId, foreign.GroupId), CancellationToken.None));
            Assert.That(await context.StaffGroups.CountAsync(), Is.Zero);
        });
    }

    [Test]
    public async Task ByGroup_ListsMembers_AndRefusesAGroupTheUserIsNotIn()
    {
        await using var context = NewContext(nameof(ByGroup_ListsMembers_AndRefusesAGroupTheUserIsNotIn));
        var seed = await SeedAsync(context);
        await context.StaffGroups.AddRangeAsync(
            new StaffGroup { StaffId = seed.Active.StaffId, GroupId = seed.GroupId },
            new StaffGroup { StaffId = seed.Inactive.StaffId, GroupId = seed.OtherGroupId });
        await context.SaveChangesAsync(CancellationToken.None);
        var reader = new StaffMemberReader(context as IApplicationDbContext, Principal(seed.AccountId, seed.UserId));

        var page = await reader.GetStaffMembersByGroupAsync(seed.GroupId, null, 0, 50, CancellationToken.None);

        Assert.Multiple(async () =>
        {
            Assert.That(page.Items.Select(s => s.StaffId), Is.EquivalentTo(new[] { seed.Active.StaffId }));
            await Assert.ThrowsAsync<NotFoundException>(() => reader.GetStaffMembersByGroupAsync(seed.OtherGroupId, null, 0, 50, CancellationToken.None));
        });
    }
}
