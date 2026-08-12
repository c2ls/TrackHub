using System.Reflection;
using Common.Application.Testing;

namespace TrackHub.Security.Application.UnitTests;

// Tenant-scope coverage (TS-06), delegated to the shared engine in Common.Application.Testing so
// every service enforces the identical gate (including keys WRAPPED inside request DTOs). Two
// invariants: (1) a request that resolves no AccountId but carries a wire entity key must declare
// how its scope is enforced — [AccountScopeEnforcedInHandler] / [PlatformScoped] /
// [AllowCrossAccount]; (2) a request whose scope is enforced per-caller must not be [Caching] —
// the cache key carries only request fields, so the cached response leaks across accounts (SVD-09).
[TestFixture]
public class AccountScopeCoverageTests
{
    private static readonly Assembly ApplicationAssembly = Assembly.Load("TrackHub.Security.Application");

    [Test]
    public void EveryKeyedRequest_DeclaresHowItsScopeIsEnforced()
    {
        var offenders = AccountScopeCoverage.UndeclaredKeyedRequests(ApplicationAssembly);

        Assert.That(offenders, Is.Empty,
            "These requests carry a wire entity key (root or DTO-wrapped), resolve no AccountId, and "
            + "declare no scope ([AccountScopeEnforcedInHandler] / [PlatformScoped] / [AllowCrossAccount]) "
            + "- a by-id cross-tenant escape shape:\n" + string.Join("\n", offenders));
    }

    [Test]
    public void NoCallerScopedOrHandlerEnforcedRequest_IsCached()
    {
        var offenders = AccountScopeCoverage.CachedUnscopedRequests(ApplicationAssembly);

        Assert.That(offenders, Is.Empty,
            "These requests are [Caching] but their scope is enforced per-caller, so the cached "
            + "response would be served across accounts (SVD-09):\n" + string.Join("\n", offenders));
    }
}
