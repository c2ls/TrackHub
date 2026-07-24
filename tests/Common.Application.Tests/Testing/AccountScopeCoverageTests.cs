using Common.Application.Attributes;
using Common.Application.Testing;
using Common.Mediator;
using FluentAssertions;

namespace Common.Application.Tests.Testing;

/// <summary>
/// Proves the shared TS-06 coverage engine (<see cref="AccountScopeCoverage"/>) that every
/// service's <c>AccountScopeCoverageTests</c> delegates to. The critical case is the WRAPPED key:
/// a wire key hidden one level down inside a TrackHub-owned DTO (the <c>FiltersInput</c> shape)
/// must be found — a root-only inspection let exactly those requests bypass the gate.
/// </summary>
public class AccountScopeCoverageTests
{
    // --- fixtures: keyed / keyless shapes -------------------------------------------------

    public readonly record struct FiltersDto(Guid DeviceId, string? Search);

    public readonly record struct KeylessFiltersDto(string? Search, int Page);

    /// <summary>Root-level wire key, unmarked — the classic offender.</summary>
    public class RootKeyRequest : IRequest<string>
    {
        public Guid Id { get; init; }
    }

    /// <summary>Wire key WRAPPED in a DTO member — must still be detected (the hardening).</summary>
    public class WrappedKeyRequest : IRequest<string>
    {
        public FiltersDto Filters { get; init; }
    }

    [AccountScopeEnforcedInHandler]
    public class WrappedKeyMarkedRequest : IRequest<string>
    {
        public FiltersDto Filters { get; init; }
    }

    /// <summary>No key anywhere — caller-scoped, needs no marker.</summary>
    public class KeylessRequest : IRequest<string>
    {
        public KeylessFiltersDto Filters { get; init; }

        public string Name { get; init; } = string.Empty;
    }

    /// <summary>Keyed but account-bearing — the behavior scopes it off the wire account.</summary>
    public class AccountBearingKeyedRequest : IRequest<string>
    {
        public Guid AccountId { get; init; }

        public Guid DeviceId { get; init; }
    }

    // --- fixtures: [Caching] × scope shapes (SVD-09) ---------------------------------------

    [Caching]
    public class CachedCallerScopedRequest : IRequest<string>
    {
        public int Page { get; init; }
    }

    [Caching]
    [AccountScopeEnforcedInHandler]
    public class CachedEnforcedRequest : IRequest<string>
    {
        public Guid Id { get; init; }
    }

    [Caching]
    [PlatformScoped("Test fixture standing in for a platform-owned catalog.")]
    public class CachedPlatformScopedRequest : IRequest<string>
    {
        public int Page { get; init; }
    }

    [Caching]
    public class CachedAccountBearingRequest : IRequest<string>
    {
        public Guid AccountId { get; init; }
    }

    private static IReadOnlyList<string> UndeclaredKeyed()
        => AccountScopeCoverage.UndeclaredKeyedRequests(typeof(AccountScopeCoverageTests).Assembly);

    private static IReadOnlyList<string> CachedUnscoped()
        => AccountScopeCoverage.CachedUnscopedRequests(typeof(AccountScopeCoverageTests).Assembly);

    [Fact]
    public void UndeclaredKeyedRequests_FindsRootLevelKey()
    {
        UndeclaredKeyed().Should().Contain(typeof(RootKeyRequest).FullName);
    }

    [Fact]
    public void UndeclaredKeyedRequests_FindsKeyWrappedInsideDto()
    {
        // The FiltersInput shape: the key sits one level down inside a TrackHub-owned DTO. A
        // root-only inspection missed it; the shared walk must not.
        UndeclaredKeyed().Should().Contain(typeof(WrappedKeyRequest).FullName);
    }

    [Fact]
    public void UndeclaredKeyedRequests_AcceptsDeclaredAndKeylessShapes()
    {
        var offenders = UndeclaredKeyed();

        offenders.Should().NotContain(typeof(WrappedKeyMarkedRequest).FullName,
            "a declared [AccountScopeEnforcedInHandler] request is covered");
        offenders.Should().NotContain(typeof(KeylessRequest).FullName,
            "a keyless request is caller-scoped and needs no marker");
        offenders.Should().NotContain(typeof(AccountBearingKeyedRequest).FullName,
            "an account-bearing request is scoped off its wire account");
    }

    [Fact]
    public void CachedUnscopedRequests_FlagsCallerScopedAndHandlerEnforcedCaching()
    {
        var offenders = CachedUnscoped();

        offenders.Should().Contain(typeof(CachedCallerScopedRequest).FullName,
            "a caller-scoped response cached under a request-only key is served across accounts");
        offenders.Should().Contain(typeof(CachedEnforcedRequest).FullName,
            "the cache short-circuits the handler that performs the ownership check");
    }

    [Fact]
    public void CachedUnscopedRequests_AcceptsPlatformScopedAndAccountBearingCaching()
    {
        var offenders = CachedUnscoped();

        offenders.Should().NotContain(typeof(CachedPlatformScopedRequest).FullName,
            "platform-owned data is identical for every caller");
        offenders.Should().NotContain(typeof(CachedAccountBearingRequest).FullName,
            "the account is part of the request and therefore part of the cache key");
    }
}
