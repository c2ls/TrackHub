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

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace Common.Application.Behaviors;

/// <summary>
/// Single source of truth for "which account does this request name?".
/// <para>
/// <see cref="AccountScopeBehavior{TRequest, TResponse}"/>, <see cref="AccountStatusBehavior{TRequest, TResponse}"/>
/// and <see cref="FeatureFlagBehavior{TRequest, TResponse}"/> all key off the request's
/// <c>AccountId</c>; they must agree exactly, or the tenant guard would evaluate a different
/// account than the feature/status checks.
/// </para>
/// <para>
/// The account id is NOT always a property of the request root: several commands carry it inside a
/// DTO member (<c>CreateBackgroundJobRunCommand(BackgroundJobRunDto)</c> and friends). A guard that a
/// request escapes merely by nesting the field one level deeper is no guard at all, so the lookup
/// recurses — under strict limits, because it sits on the hot path of every request:
/// </para>
/// <list type="bullet">
/// <item><b>Depth.</b> At most <see cref="MaxNestingDepth"/> (2) levels below the root — request →
/// DTO → nested DTO. Every real case in the platform is depth 1; depth 2 is headroom, not an
/// invitation to walk object graphs.</item>
/// <item><b>Only TrackHub-owned types.</b> Recursion descends exclusively into types declared in a
/// <c>TrackHub*</c> or <c>Common.*</c> assembly, so framework types, primitives, enums and
/// <see cref="string"/> terminate the walk immediately. Anything implementing
/// <see cref="IEnumerable"/> is skipped outright: a collection names zero or many accounts, not one,
/// and a request that batches across accounts is a cross-account surface that must SAY so with
/// <see cref="Attributes.AllowCrossAccountAttribute"/>.</item>
/// <item><b>Precedence.</b> The search is breadth-first and, within a level, ordered by ordinal
/// property name, so the resolved account is deterministic and never depends on reflection's
/// member ordering. A root-level <c>AccountId</c> ALWAYS wins over a nested one, and a shallower
/// nested one wins over a deeper one. A request that exposes two accounts at the same depth
/// resolves the alphabetically-first path — such a request is a design smell and should carry a
/// single authoritative top-level <c>AccountId</c> instead.</item>
/// <item><b>No per-call reflection.</b> The discovered property path is cached per request type;
/// steady-state cost is one dictionary lookup plus one <see cref="PropertyInfo.GetValue(object)"/>
/// per path step.</item>
/// </list>
/// </summary>
public static class RequestAccountResolver
{
    /// <summary>
    /// How far below the request root an <c>AccountId</c> is looked for. 0 would mean "root only"
    /// (the original, escapable behaviour); 2 covers request → DTO → nested DTO. Shared with
    /// <see cref="Testing.AccountScopeCoverage"/> so the coverage walk and the account walk agree
    /// on reach: a wire key the coverage test can see is a key this resolver could have scoped.
    /// </summary>
    internal const int MaxNestingDepth = 2;

    private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

    /// <summary>
    /// Property chain from the request root to its <c>AccountId</c>, or <c>null</c> when the type
    /// names no account at all. Resolved once per request type.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]?> AccountIdPaths = new();

    /// <summary>
    /// Returns the account id carried by the request itself, or <c>null</c> when the request type
    /// exposes no <c>AccountId</c> (at the root or within reach of the bounded nested search) or the
    /// value is absent/<see cref="Guid.Empty"/>. Deliberately does NOT fall back to the principal —
    /// callers decide whether a fallback is meaningful for their check.
    /// </summary>
    public static Guid? GetRequestAccountId<TRequest>(TRequest request)
    {
        var path = AccountIdPaths.GetOrAdd(typeof(TRequest), static type => FindAccountIdPath(type));

        return ReadAccountId(request, path);
    }

    /// <summary>
    /// Whether the request type exposes a resolvable <c>AccountId</c> at the root or within the
    /// bounded nested search — i.e. whether <see cref="AccountScopeBehavior{TRequest, TResponse}"/>
    /// has an account to scope. A type that returns <c>false</c> here must carry
    /// <see cref="Attributes.PlatformScopedAttribute"/> or <see cref="Attributes.AllowCrossAccountAttribute"/>
    /// (or reach the guard under a principal that carries an account) or the guard denies it. Exposed
    /// for the per-service enforcement tests that prove every request is either account-bearing or a
    /// declared opt-out; type-only, so it needs no request instance.
    /// </summary>
    public static bool NamesAccount(Type requestType)
        => AccountIdPaths.GetOrAdd(requestType, static type => FindAccountIdPath(type)) is not null;

    private static Guid? ReadAccountId(object? request, PropertyInfo[]? path)
    {
        if (path is null)
        {
            return null;
        }

        object? current = request;
        foreach (var step in path)
        {
            if (current is null)
            {
                return null;
            }

            current = step.GetValue(current);
        }

        return current is Guid guid && guid != Guid.Empty ? guid : null;
    }

    /// <summary>
    /// Breadth-first search for the request's <c>AccountId</c>, shallowest first. Returns the
    /// property chain to read it, or <c>null</c> if the type names no account.
    /// </summary>
    private static PropertyInfo[]? FindAccountIdPath(Type requestType)
    {
        var frontier = new List<PropertyInfo[]> { Array.Empty<PropertyInfo>() };

        for (var depth = 0; depth <= MaxNestingDepth && frontier.Count > 0; depth++)
        {
            var next = new List<PropertyInfo[]>();

            foreach (var prefix in frontier)
            {
                var owner = prefix.Length == 0 ? requestType : UnwrapNullable(prefix[^1].PropertyType);

                var accountId = GetAccountIdProperty(owner);
                if (accountId is not null)
                {
                    return [.. prefix, accountId];
                }

                if (depth == MaxNestingDepth)
                {
                    continue;
                }

                foreach (var candidate in GetRecursableProperties(owner))
                {
                    next.Add([.. prefix, candidate]);
                }
            }

            frontier = next;
        }

        return null;
    }

    /// <summary>
    /// The type's own readable <c>Guid</c>/<c>Guid?</c> account property, if it has one.
    /// </summary>
    private static PropertyInfo? GetAccountIdProperty(Type type)
    {
        var property = FindProperty(type, "AccountId") ?? FindProperty(type, "accountId");

        return property is not null && UnwrapNullable(property.PropertyType) == typeof(Guid)
            ? property
            : null;
    }

    private static PropertyInfo? FindProperty(Type type, string name)
    {
        try
        {
            var property = type.GetProperty(name, PublicInstance);
            return IsReadable(property) ? property : null;
        }
        catch (AmbiguousMatchException)
        {
            // A shadowed property ('new AccountId') is not a scope the guard can resolve
            // unambiguously; treat it as absent rather than guessing which declaration wins.
            return null;
        }
    }

    /// <summary>
    /// Complex TrackHub-owned members worth descending into, in a deterministic ordinal order.
    /// Shared with <see cref="Testing.AccountScopeCoverage"/> so both walks descend identically.
    /// </summary>
    internal static IEnumerable<PropertyInfo> GetRecursableProperties(Type type)
        => type.GetProperties(PublicInstance)
            .Where(property => IsReadable(property) && IsTrackHubComplexType(UnwrapNullable(property.PropertyType)))
            .OrderBy(property => property.Name, StringComparer.Ordinal);

    internal static bool IsReadable(PropertyInfo? property)
        => property is { CanRead: true } && property.GetIndexParameters().Length == 0;

    /// <summary>
    /// True for a DTO/record/class declared by this platform. Framework types, primitives, enums,
    /// <see cref="string"/> and every collection are excluded, which is what keeps the walk cheap
    /// and bounded regardless of the depth limit.
    /// </summary>
    private static bool IsTrackHubComplexType(Type type)
    {
        if (type.IsPrimitive || type.IsEnum || type == typeof(string) || typeof(IEnumerable).IsAssignableFrom(type))
        {
            return false;
        }

        var assemblyName = type.Assembly.GetName().Name;

        return assemblyName is not null
            && (assemblyName.StartsWith("TrackHub", StringComparison.Ordinal)
                || assemblyName.StartsWith("Common.", StringComparison.Ordinal));
    }

    internal static Type UnwrapNullable(Type type)
        => Nullable.GetUnderlyingType(type) ?? type;
}
