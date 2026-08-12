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

using Microsoft.EntityFrameworkCore;
using TrackHub.AuthorityServer.Infrastructure.Entities;
using TrackHub.AuthorityServer.Infrastructure.Readers;

namespace TrackHub.AuthorityServer.Infrastructure.UnitTests;

// The account claim on a client-credentials token is derived from the client's effective grants on
// the pre-existing security.service_client_permissions table (read-only bounded-context mapping).
[TestFixture]
public class ServiceClientPermissionReaderTests
{
    private static SecurityDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<SecurityDbContext>().UseInMemoryDatabase(name).Options);

    private static ServiceClientPermission Grant(
        string clientId,
        Guid? accountId = null,
        bool allowCrossAccount = false,
        bool active = true,
        DateTimeOffset? effectiveFrom = null,
        DateTimeOffset? effectiveTo = null)
        => new()
        {
            ServiceClientPermissionId = Guid.NewGuid(),
            ClientId = clientId,
            AccountId = accountId,
            AllowCrossAccount = allowCrossAccount,
            Active = active,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo
        };

    [Test]
    public async Task GetAccountScopeAsync_AccountBoundGrants_ReturnsTheDistinctAccounts()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetAccountScopeAsync_AccountBoundGrants_ReturnsTheDistinctAccounts));
        context.ServiceClientPermissions.AddRange(
            Grant("partner_client", accountId),
            Grant("partner_client", accountId));
        await context.SaveChangesAsync();

        var scope = await new ServiceClientPermissionReader(context).GetAccountScopeAsync("partner_client", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(scope.AllowsCrossAccount, Is.False);
            Assert.That(scope.AccountIds, Is.EqualTo(new[] { accountId }));
        });
    }

    [Test]
    public async Task GetAccountScopeAsync_PlatformWideGrant_ReportsCrossAccountReach()
    {
        await using var context = NewContext(nameof(GetAccountScopeAsync_PlatformWideGrant_ReportsCrossAccountReach));
        context.ServiceClientPermissions.Add(Grant("router_client", allowCrossAccount: true));
        await context.SaveChangesAsync();

        var scope = await new ServiceClientPermissionReader(context).GetAccountScopeAsync("router_client", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(scope.AllowsCrossAccount, Is.True);
            Assert.That(scope.AccountIds, Is.Empty);
        });
    }

    [Test]
    public async Task GetAccountScopeAsync_InactiveOrExpiredGrants_AreIgnored()
    {
        await using var context = NewContext(nameof(GetAccountScopeAsync_InactiveOrExpiredGrants_AreIgnored));
        context.ServiceClientPermissions.AddRange(
            Grant("partner_client", Guid.NewGuid(), active: false),
            Grant("partner_client", Guid.NewGuid(), effectiveTo: DateTimeOffset.UtcNow.AddDays(-1)),
            Grant("partner_client", Guid.NewGuid(), effectiveFrom: DateTimeOffset.UtcNow.AddDays(1)));
        await context.SaveChangesAsync();

        var scope = await new ServiceClientPermissionReader(context).GetAccountScopeAsync("partner_client", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(scope.AllowsCrossAccount, Is.False);
            Assert.That(scope.AccountIds, Is.Empty);
        });
    }

    [Test]
    public async Task GetAccountScopeAsync_OtherClientsGrants_DoNotLeak()
    {
        var accountId = Guid.NewGuid();
        await using var context = NewContext(nameof(GetAccountScopeAsync_OtherClientsGrants_DoNotLeak));
        context.ServiceClientPermissions.AddRange(
            Grant("partner_client", accountId),
            Grant("other_client", Guid.NewGuid()),
            Grant("other_client", allowCrossAccount: true));
        await context.SaveChangesAsync();

        var scope = await new ServiceClientPermissionReader(context).GetAccountScopeAsync("partner_client", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(scope.AllowsCrossAccount, Is.False);
            Assert.That(scope.AccountIds, Is.EqualTo(new[] { accountId }));
        });
    }
}
