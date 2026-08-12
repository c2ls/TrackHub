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
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

// Separation of duties: a support grant may not be approved by the principal
// that created it.
[TestFixture]
public class AccountSupportGrantWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static ICurrentPrincipal AdminPrincipal(Guid accountId, Guid userId)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.User);
        principal.SetupGet(p => p.UserId).Returns(userId);
        principal.SetupGet(p => p.AccountId).Returns(accountId);
        principal.SetupGet(p => p.Role).Returns(Roles.Administrator);
        principal.SetupGet(p => p.SubjectId).Returns(userId.ToString());
        principal.SetupGet(p => p.DriverId).Returns((Guid?)null);
        principal.SetupGet(p => p.ClientId).Returns((string?)null);
        principal.SetupGet(p => p.CorrelationId).Returns("corr-1");
        return principal.Object;
    }

    private static AccountSupportGrant PendingGrant(Guid accountId, string createdBy)
    {
        var now = DateTimeOffset.UtcNow;
        return new AccountSupportGrant(accountId, Guid.NewGuid(), "reason", "TICKET-1", SupportAccessLevels.Full, now, now.AddHours(1))
        {
            CreatedBy = createdBy
        };
    }

    [Test]
    public async Task Approve_ByCreator_ThrowsForbidden()
    {
        var accountId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        await using var context = NewContext(nameof(Approve_ByCreator_ThrowsForbidden));
        var grant = PendingGrant(accountId, creatorId.ToString());
        await context.AccountSupportGrants.AddAsync(grant);
        await context.SaveChangesAsync(CancellationToken.None);

        // The approver IS the creator.
        var writer = new AccountSupportGrantWriter(context as IApplicationDbContext, AdminPrincipal(accountId, creatorId));

        Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
            await writer.ApproveAccountSupportGrantAsync(grant.AccountSupportGrantId, "approver", CancellationToken.None));
        Assert.That(context.AccountSupportGrants.Single().ApprovedAt, Is.Null, "self-approval must not approve the grant");
    }

    [Test]
    public async Task Approve_ByDifferentPrincipal_Succeeds()
    {
        var accountId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();
        var approverId = Guid.NewGuid();
        await using var context = NewContext(nameof(Approve_ByDifferentPrincipal_Succeeds));
        var grant = PendingGrant(accountId, creatorId.ToString());
        await context.AccountSupportGrants.AddAsync(grant);
        await context.SaveChangesAsync(CancellationToken.None);

        var writer = new AccountSupportGrantWriter(context as IApplicationDbContext, AdminPrincipal(accountId, approverId));

        await writer.ApproveAccountSupportGrantAsync(grant.AccountSupportGrantId, "approver", CancellationToken.None);

        Assert.That(context.AccountSupportGrants.Single().ApprovedAt, Is.Not.Null);
    }
}
