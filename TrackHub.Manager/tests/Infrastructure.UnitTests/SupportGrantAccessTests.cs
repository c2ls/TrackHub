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
using TrackHub.Manager.Domain.Constants;
using TrackHub.Manager.Domain.Records;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Readers;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

// AccountSupportGrant.AccessLevel is enforced on mutation paths — a read-only
// cross-account grant permits reads but every cross-account mutation is rejected. AC15: the mutating
// staff writer emits an audit event. AC1: without any grant, cross-account access is forbidden.
[TestFixture]
public class SupportGrantAccessTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

    // A support engineer: Administrator role, their OWN account differs from the target account, so the
    // only possible basis for accessing the target account is an AccountSupportGrant.
    private static ICurrentPrincipal SupportPrincipal(Guid supportUserId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(p => p.UserId).Returns(supportUserId);
        principal.SetupGet(p => p.AccountId).Returns(Guid.NewGuid()); // a different account than the target
        principal.SetupGet(p => p.Role).Returns(Roles.Administrator);
        principal.SetupGet(p => p.SubjectId).Returns(supportUserId.ToString());
        principal.SetupGet(p => p.StaffId).Returns((Guid?)null);
        principal.SetupGet(p => p.ClientId).Returns((string?)null);
        principal.SetupGet(p => p.CorrelationId).Returns("corr-1");
        return principal.Object;
    }

    private static AccountSupportGrant ApprovedGrant(Guid targetAccountId, Guid supportUserId, string accessLevel)
    {
        var now = DateTimeOffset.UtcNow;
        return new AccountSupportGrant(targetAccountId, supportUserId, "reason", "TICKET-1", accessLevel, now.AddMinutes(-5), now.AddHours(1))
        {
            ApprovedBy = "approver",
            ApprovedAt = now.AddMinutes(-4)
        };
    }

    private static StaffMemberDto StaffForAccount(Guid accountId)
        => new(accountId, "Staff A", null, null, null, true, null, null, null, null, StaffTypes.Driver);

    [Test]
    public async Task CreateStaffMember_ReadOnlySupportGrant_ThrowsForbidden()
    {
        var supportUserId = Guid.NewGuid();
        var targetAccountId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateStaffMember_ReadOnlySupportGrant_ThrowsForbidden));
        await context.AccountSupportGrants.AddAsync(ApprovedGrant(targetAccountId, supportUserId, SupportAccessLevels.ReadOnly));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new StaffMemberWriter(context as IApplicationDbContext, SupportPrincipal(supportUserId));

        await Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await writer.CreateStaffMemberAsync(StaffForAccount(targetAccountId), CancellationToken.None));
        Assert.That(context.StaffMembers.Count(), Is.EqualTo(0), "read-only grant must not create a staff");
    }

    [Test]
    public async Task CreateStaffMember_FullSupportGrant_Succeeds_AndWritesAuditEvent()
    {
        var supportUserId = Guid.NewGuid();
        var targetAccountId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateStaffMember_FullSupportGrant_Succeeds_AndWritesAuditEvent));
        await context.AccountSupportGrants.AddAsync(ApprovedGrant(targetAccountId, supportUserId, SupportAccessLevels.Full));
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new StaffMemberWriter(context as IApplicationDbContext, SupportPrincipal(supportUserId));

        var result = await writer.CreateStaffMemberAsync(StaffForAccount(targetAccountId), CancellationToken.None);

        Assert.That(result.AccountId, Is.EqualTo(targetAccountId));
        Assert.That(context.StaffMembers.Count(), Is.EqualTo(1));
        // AC15: the mutation emitted exactly one audit event carrying the target account + action.
        var audit = context.AuditEvents.Single();
        Assert.That(audit.AccountId, Is.EqualTo(targetAccountId));
        Assert.That(audit.Action, Is.EqualTo("CreateStaffMember"));
        Assert.That(audit.ResourceType, Is.EqualTo("Staff"));
    }

    [Test]
    public async Task GetStaffMembersByAccount_ReadOnlySupportGrant_AllowsRead()
    {
        var supportUserId = Guid.NewGuid();
        var targetAccountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetStaffMembersByAccount_ReadOnlySupportGrant_AllowsRead));
        await context.AccountSupportGrants.AddAsync(ApprovedGrant(targetAccountId, supportUserId, SupportAccessLevels.ReadOnly));
        await context.StaffMembers.AddAsync(new StaffMember(targetAccountId, "Staff A", null, null, null, true, null, null, null, null));
        await context.SaveChangesAsync(CancellationToken.None);

        var reader = new StaffMemberReader(context as IApplicationDbContext, SupportPrincipal(supportUserId));

        var staff = (await reader.GetStaffMembersByAccountAsync(targetAccountId, null, null, 0, 50, CancellationToken.None)).Items;

        Assert.That(staff, Has.Count.EqualTo(1), "a read-only grant must still permit cross-account reads");
    }

    [Test]
    public async Task CreateStaffMember_NoSupportGrant_CrossAccount_ThrowsForbidden()
    {
        var supportUserId = Guid.NewGuid();
        var targetAccountId = Guid.NewGuid();
        await using var context = NewContext(nameof(CreateStaffMember_NoSupportGrant_CrossAccount_ThrowsForbidden));

        var writer = new StaffMemberWriter(context as IApplicationDbContext, SupportPrincipal(supportUserId));

        await Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await writer.CreateStaffMemberAsync(StaffForAccount(targetAccountId), CancellationToken.None));
    }
}
