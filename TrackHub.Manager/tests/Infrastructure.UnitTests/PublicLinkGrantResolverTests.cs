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

using TrackHub.Manager.Domain.Enums;
using TrackHub.Manager.Domain.Models;
using TrackHub.Manager.Infrastructure;
using TrackHub.Manager.Infrastructure.Interfaces;
using TrackHub.Manager.Infrastructure.ManagerDB;
using TrackHub.Manager.Infrastructure.ManagerDB.Writers;

namespace Infrastructure.UnitTests;

/// <summary>
/// The single public-link resolution path (spec 11 §7.8/§18.10). These tests pin the discriminated
/// outcomes AND the side effects that used to be duplicated across the REST endpoint and the removed
/// <c>RecordPublicLinkAccess</c> writer method: the access count moves exactly once, and exactly one
/// <c>PublicLinkAccessed</c> audit row is written per successful resolution.
/// </summary>
[TestFixture]
public class PublicLinkGrantResolverTests
{
    private const string Token = "a-public-link-token";
    private const string ResourceType = "Trip";
    private const string Scope = "trip.track";

    private static ApplicationDbContext NewContext(string name)
        => new(new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(name).Options);

    private static PublicLinkGrant Grant(Guid accountId, string resourceId, string scopes = Scope, string token = Token)
        => new(accountId, ResourceType, resourceId, scopes, "customer tracking", PublicLinkTokenHasher.Hash(token), DateTimeOffset.UtcNow.AddDays(1), "creator");

    private static PublicLinkGrantResolver NewResolver(ApplicationDbContext context)
        => new(context as IApplicationDbContext);

    private static async Task<(ApplicationDbContext Context, PublicLinkGrant Grant, PublicLinkGrantResolver Resolver)> SeedAsync(
        string name, Guid accountId, string resourceId, string scopes = Scope)
    {
        var context = NewContext(name);
        var grant = Grant(accountId, resourceId, scopes);
        await context.PublicLinkGrants.AddAsync(grant);
        await context.SaveChangesAsync(CancellationToken.None);
        return (context, grant, NewResolver(context));
    }

    private static int AccessAuditCount(ApplicationDbContext context)
        => context.AuditEvents.Count(x => x.Action == "PublicLinkAccessed");

    [Test]
    public async Task Resolve_ValidToken_ReturnsFound_CountsAccessOnce_AndWritesOneAuditRow()
    {
        var accountId = Guid.NewGuid();
        var resourceId = Guid.NewGuid().ToString();
        var (context, grant, resolver) = await SeedAsync(nameof(Resolve_ValidToken_ReturnsFound_CountsAccessOnce_AndWritesOneAuditRow), accountId, resourceId);
        await using var _ = context;

        var result = await resolver.ResolvePublicLinkGrantAsync(
            grant.PublicLinkGrantId, accountId, ResourceType, resourceId, Scope, Token, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(PublicLinkResolution.Found));
            Assert.That(result.Grant, Is.Not.Null, "a Found resolution must carry the grant");
            Assert.That(result.Grant!.Value.PublicLinkGrantId, Is.EqualTo(grant.PublicLinkGrantId));
            Assert.That(result.Grant!.Value.ResourceId, Is.EqualTo(resourceId));
            Assert.That(result.Grant!.Value.Token, Is.Null, "resolution never re-issues the plaintext token");
        });

        var stored = context.PublicLinkGrants.Single();
        Assert.Multiple(() =>
        {
            Assert.That(stored.AccessCount, Is.EqualTo(1), "access must be counted exactly once per resolution");
            Assert.That(stored.LastAccessedAt, Is.Not.Null);
            // The whole point of the extraction: one place counts, one place audits.
            Assert.That(AccessAuditCount(context), Is.EqualTo(1), "exactly one PublicLinkAccessed audit row per successful resolution");
        });

        var audit = context.AuditEvents.Single(x => x.Action == "PublicLinkAccessed");
        Assert.Multiple(() =>
        {
            Assert.That(audit.ActorType, Is.EqualTo("PublicLink"), "the audit contract fixes the PublicLink actor type");
            Assert.That(audit.ResourceType, Is.EqualTo("PublicLinkGrant"));
            Assert.That(audit.ResourceId, Is.EqualTo(grant.PublicLinkGrantId.ToString()));
            Assert.That(audit.AccountId, Is.EqualTo(accountId));
            Assert.That(audit.Result, Is.EqualTo("Succeeded"));
        });
    }

    [Test]
    public async Task Resolve_TwoResolutions_CountTwice_AndAuditTwice()
    {
        var accountId = Guid.NewGuid();
        var resourceId = Guid.NewGuid().ToString();
        var (context, grant, resolver) = await SeedAsync(nameof(Resolve_TwoResolutions_CountTwice_AndAuditTwice), accountId, resourceId);
        await using var _ = context;

        await resolver.ResolvePublicLinkGrantAsync(grant.PublicLinkGrantId, accountId, ResourceType, resourceId, Scope, Token, CancellationToken.None);
        await resolver.ResolvePublicLinkGrantAsync(grant.PublicLinkGrantId, accountId, ResourceType, resourceId, Scope, Token, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(context.PublicLinkGrants.Single().AccessCount, Is.EqualTo(2));
            Assert.That(AccessAuditCount(context), Is.EqualTo(2), "one audit row per resolution, never a doubled pair");
        });
    }

    [Test]
    public async Task Resolve_RevokedGrant_ReturnsNotFound_AndRecordsNothing()
    {
        var accountId = Guid.NewGuid();
        var resourceId = Guid.NewGuid().ToString();
        var (context, grant, resolver) = await SeedAsync(nameof(Resolve_RevokedGrant_ReturnsNotFound_AndRecordsNothing), accountId, resourceId);
        await using var _ = context;
        grant.RevokedAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        grant.RevokedBy = "admin";
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await resolver.ResolvePublicLinkGrantAsync(
            grant.PublicLinkGrantId, accountId, ResourceType, resourceId, Scope, Token, CancellationToken.None);

        AssertRejected(context, result, PublicLinkResolution.NotFound);
    }

    [Test]
    public async Task Resolve_ScopeNotGranted_ReturnsNotFound_AndRecordsNothing()
    {
        var accountId = Guid.NewGuid();
        var resourceId = Guid.NewGuid().ToString();
        var (context, grant, resolver) = await SeedAsync(
            nameof(Resolve_ScopeNotGranted_ReturnsNotFound_AndRecordsNothing), accountId, resourceId, scopes: "document.read");
        await using var _ = context;

        var result = await resolver.ResolvePublicLinkGrantAsync(
            grant.PublicLinkGrantId, accountId, ResourceType, resourceId, Scope, Token, CancellationToken.None);

        AssertRejected(context, result, PublicLinkResolution.NotFound);
    }

    [Test]
    public async Task Resolve_BadToken_ReturnsNotFound_AndRecordsNothing()
    {
        var accountId = Guid.NewGuid();
        var resourceId = Guid.NewGuid().ToString();
        var (context, grant, resolver) = await SeedAsync(nameof(Resolve_BadToken_ReturnsNotFound_AndRecordsNothing), accountId, resourceId);
        await using var _ = context;

        var result = await resolver.ResolvePublicLinkGrantAsync(
            grant.PublicLinkGrantId, accountId, ResourceType, resourceId, Scope, "not-the-token", CancellationToken.None);

        AssertRejected(context, result, PublicLinkResolution.NotFound);
    }

    [Test]
    public async Task Resolve_ExpiredGrant_ReturnsExpired_AndRecordsNothing()
    {
        var accountId = Guid.NewGuid();
        var resourceId = Guid.NewGuid().ToString();
        var (context, grant, resolver) = await SeedAsync(nameof(Resolve_ExpiredGrant_ReturnsExpired_AndRecordsNothing), accountId, resourceId);
        await using var _ = context;
        grant.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        await context.SaveChangesAsync(CancellationToken.None);

        var result = await resolver.ResolvePublicLinkGrantAsync(
            grant.PublicLinkGrantId, accountId, ResourceType, resourceId, Scope, Token, CancellationToken.None);

        // 410 is disclosable — the holder legitimately had the link — but it is still not an access.
        AssertRejected(context, result, PublicLinkResolution.Expired);
    }

    [Test]
    public async Task Resolve_WrongAccount_ReturnsNotFound()
    {
        var accountId = Guid.NewGuid();
        var resourceId = Guid.NewGuid().ToString();
        var (context, grant, resolver) = await SeedAsync(nameof(Resolve_WrongAccount_ReturnsNotFound), accountId, resourceId);
        await using var _ = context;

        var result = await resolver.ResolvePublicLinkGrantAsync(
            grant.PublicLinkGrantId, Guid.NewGuid(), ResourceType, resourceId, Scope, Token, CancellationToken.None);

        AssertRejected(context, result, PublicLinkResolution.NotFound);
    }

    [Test]
    public async Task Resolve_ScopeMatchIsCaseInsensitive_AcrossSpaceAndCommaSeparators()
    {
        var accountId = Guid.NewGuid();
        var resourceId = Guid.NewGuid().ToString();
        var (context, grant, resolver) = await SeedAsync(
            nameof(Resolve_ScopeMatchIsCaseInsensitive_AcrossSpaceAndCommaSeparators), accountId, resourceId,
            scopes: "document.read, TRIP.TRACK other.scope");
        await using var _ = context;

        var result = await resolver.ResolvePublicLinkGrantAsync(
            grant.PublicLinkGrantId, accountId, ResourceType, resourceId, "trip.track", Token, CancellationToken.None);

        Assert.That(result.Resolution, Is.EqualTo(PublicLinkResolution.Found));
    }

    private static void AssertRejected(ApplicationDbContext context, PublicLinkGrantResolutionResult result, PublicLinkResolution expected)
        => Assert.Multiple(() =>
        {
            Assert.That(result.Resolution, Is.EqualTo(expected));
            Assert.That(result.Grant, Is.Null, "a rejected resolution must disclose no grant data");
            Assert.That(context.PublicLinkGrants.Single().AccessCount, Is.Zero, "a rejected resolution is not an access");
            Assert.That(context.PublicLinkGrants.Single().LastAccessedAt, Is.Null);
            Assert.That(AccessAuditCount(context), Is.Zero, "a rejected resolution writes no access audit row");
        });
}
