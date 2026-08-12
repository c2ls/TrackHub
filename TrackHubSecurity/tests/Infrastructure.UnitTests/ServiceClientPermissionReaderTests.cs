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
using TrackHub.Security.Infrastructure;
using TrackHub.Security.Infrastructure.Entities;
using TrackHub.Security.Infrastructure.Interfaces;
using TrackHub.Security.Infrastructure.Readers;

namespace Infrastructure.UnitTests;

// Pins the account-matching semantics of a service-client grant. A NULL accountid used to be an
// implicit wildcard — it matched a token for ANY tenant, so "global" and "unscoped" were
// indistinguishable and every service client had unbounded tenant reach. Cross-account reach is
// now an explicit declaration (allowcrossaccount).
[TestFixture]
public class ServiceClientPermissionReaderTests
{
    private const string ClientId = "partner_client";
    private const string Resource = "Trips";
    private const string Action = "Write";
    private static readonly string[] Scopes = ["service_scope"];
    private static readonly string[] Audiences = ["trackhub_api"];

    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static async Task<ServiceClientPermissionReader> ReaderWith(string name, Guid? grantAccountId, bool allowCrossAccount)
    {
        var context = NewContext(name);
        context.ServiceClientPermissions.Add(new ServiceClientPermission(
            ClientId, grantAccountId, Resource, Action, Scopes[0], Audiences[0], active: true, allowCrossAccount));
        await context.SaveChangesAsync();
        return new ServiceClientPermissionReader(context as IApplicationDbContext);
    }

    private static Task<bool> Check(ServiceClientPermissionReader reader, Guid? tokenAccountId)
        => reader.HasPermissionAsync(ClientId, Resource, Action, tokenAccountId, Scopes, Audiences, CancellationToken.None);

    [Test]
    public async Task NullAccountGrant_IsNoLongerAWildcard_ForAnAccountBearingToken()
    {
        var reader = await ReaderWith(nameof(NullAccountGrant_IsNoLongerAWildcard_ForAnAccountBearingToken), null, false);

        Assert.That(await Check(reader, Guid.NewGuid()), Is.False);
    }

    [Test]
    public async Task NullAccountGrant_MatchesAnUnscopedToken()
    {
        var reader = await ReaderWith(nameof(NullAccountGrant_MatchesAnUnscopedToken), null, false);

        Assert.That(await Check(reader, null), Is.True);
    }

    [Test]
    public async Task DeclaredCrossAccountGrant_MatchesAnyToken()
    {
        var reader = await ReaderWith(nameof(DeclaredCrossAccountGrant_MatchesAnyToken), null, true);

        Assert.Multiple(async () =>
        {
            Assert.That(await Check(reader, Guid.NewGuid()), Is.True);
            Assert.That(await Check(reader, null), Is.True);
        });
    }

    [Test]
    public async Task AccountBoundGrant_MatchesOnlyItsOwnAccount()
    {
        var accountId = Guid.NewGuid();
        var reader = await ReaderWith(nameof(AccountBoundGrant_MatchesOnlyItsOwnAccount), accountId, false);

        Assert.Multiple(async () =>
        {
            Assert.That(await Check(reader, accountId), Is.True);
            Assert.That(await Check(reader, Guid.NewGuid()), Is.False);
            Assert.That(await Check(reader, null), Is.False);
        });
    }
}
