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

using Ardalis.GuardClauses;
using Common.Application.Interfaces;
using Common.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using TrackHub.Security.Infrastructure;
using TrackHub.Security.Infrastructure.Entities;
using TrackHub.Security.Infrastructure.Interfaces;
using TrackHub.Security.Infrastructure.Writers;
using TrackHub.Security.Domain.Interfaces;
using TrackHub.Security.Domain.Records;

namespace Infrastructure.UnitTests;

// Every credential change that must end live sessions re-stamps the credential; the AuthorityServer
// compares the token's security_stamp against the row on authorize and refresh.
[TestFixture]
public class StaffIdentityWriterTests
{
    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options);

    private static StaffIdentityWriter NewWriter(ApplicationDbContext context, bool staffBelongsToAccount = true)
    {
        var principal = new Mock<ICurrentPrincipal>();
        principal.SetupGet(p => p.PrincipalType).Returns(PrincipalType.ServiceClient);
        principal.SetupGet(p => p.AccountId).Returns((Guid?)null);
        var staff = new Mock<IManagerStaffMemberReader>();
        staff.Setup(d => d.StaffBelongsToAccountAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(staffBelongsToAccount);
        return new StaffIdentityWriter(context as IApplicationDbContext, principal.Object, staff.Object);
    }

    private static async Task<StaffCredential> SeedAsync(ApplicationDbContext context, Guid staffId, Guid accountId, string login)
    {
        var credential = new StaffCredential(staffId, accountId, login, "secret".HashPassword(), active: true) { VerifiedAt = DateTimeOffset.UtcNow };
        await context.StaffCredentials.AddAsync(credential);
        await context.SaveChangesAsync(CancellationToken.None);
        return credential;
    }

    private static async Task<StaffCredential> ReloadAsync(ApplicationDbContext context, Guid credentialId)
        => await context.StaffCredentials.AsNoTracking().SingleAsync(x => x.StaffCredentialId == credentialId);

    [Test]
    public async Task CreateCredential_ForAStaffOfAnotherAccount_IsNotFound()
    {
        await using var context = NewContext(nameof(CreateCredential_ForAStaffOfAnotherAccount_IsNotFound));
        var writer = NewWriter(context, staffBelongsToAccount: false);
        var dto = new StaffCredentialDto(Guid.NewGuid(), Guid.NewGuid(), "staff1", "secret", Active: true, ResetRequired: false);

        await Assert.ThrowsAsync<NotFoundException>(() => writer.CreateStaffCredentialAsync(dto, CancellationToken.None));
        Assert.That(await context.StaffCredentials.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task Lock_Reset_Activate_EachReStampTheCredential()
    {
        await using var context = NewContext(nameof(Lock_Reset_Activate_EachReStampTheCredential));
        var credential = await SeedAsync(context, Guid.NewGuid(), Guid.NewGuid(), "STAFF1");
        var writer = NewWriter(context);
        var stamps = new List<Guid> { credential.SecurityStamp };

        await writer.LockStaffCredentialAsync(credential.StaffCredentialId, DateTimeOffset.UtcNow.AddMinutes(15), CancellationToken.None);
        stamps.Add((await ReloadAsync(context, credential.StaffCredentialId)).SecurityStamp);

        await writer.ResetStaffCredentialAsync(credential.StaffCredentialId, "new-secret", resetRequired: true, CancellationToken.None);
        stamps.Add((await ReloadAsync(context, credential.StaffCredentialId)).SecurityStamp);

        await writer.ActivateStaffCredentialAsync(credential.StaffCredentialId, "final-secret", CancellationToken.None);
        stamps.Add((await ReloadAsync(context, credential.StaffCredentialId)).SecurityStamp);

        Assert.That(stamps.Distinct().Count(), Is.EqualTo(stamps.Count), "each change must invalidate the sessions issued before it");
    }

    [Test]
    public async Task Revoke_DeactivatesAndReStamps()
    {
        await using var context = NewContext(nameof(Revoke_DeactivatesAndReStamps));
        var credential = await SeedAsync(context, Guid.NewGuid(), Guid.NewGuid(), "STAFF1");
        var before = credential.SecurityStamp;

        await NewWriter(context).RevokeStaffCredentialAsync(credential.StaffCredentialId, CancellationToken.None);

        var stored = await ReloadAsync(context, credential.StaffCredentialId);
        Assert.Multiple(() =>
        {
            Assert.That(stored.Active, Is.False);
            Assert.That(stored.SecurityStamp, Is.Not.EqualTo(before));
        });
    }

    [Test]
    public async Task RevokeAll_EndsEveryLiveCredentialOfTheStaff_AndOnlyThose()
    {
        await using var context = NewContext(nameof(RevokeAll_EndsEveryLiveCredentialOfTheStaff_AndOnlyThose));
        var accountId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var first = await SeedAsync(context, staffId, accountId, "STAFF1");
        var second = await SeedAsync(context, staffId, accountId, "STAFF2");
        var otherStaff = await SeedAsync(context, Guid.NewGuid(), accountId, "OTHER");
        var otherAccount = await SeedAsync(context, staffId, Guid.NewGuid(), "ELSEWHERE");

        await NewWriter(context).RevokeStaffCredentialsAsync(staffId, accountId, CancellationToken.None);

        Assert.Multiple(async () =>
        {
            Assert.That((await ReloadAsync(context, first.StaffCredentialId)).Active, Is.False);
            Assert.That((await ReloadAsync(context, second.StaffCredentialId)).Active, Is.False);
            Assert.That((await ReloadAsync(context, otherStaff.StaffCredentialId)).Active, Is.True, "another staff's credential is untouched");
            Assert.That((await ReloadAsync(context, otherAccount.StaffCredentialId)).Active, Is.True, "the same staff id in another account is untouched");
        });
    }
}
