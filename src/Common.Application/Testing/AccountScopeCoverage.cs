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
using System.Reflection;
using Common.Application.Attributes;
using Common.Application.Behaviors;
using Common.Mediator;

namespace Common.Application.Testing;

/// <summary>
/// Tenant-scope coverage (TS-06) — the SHARED engine behind every service's
/// <c>AccountScopeCoverageTests</c>. Each service's unit-test project asserts, against its own
/// Application assembly, that both of these return empty:
/// <list type="number">
/// <item><see cref="UndeclaredKeyedRequests"/> — a request that resolves no <c>AccountId</c> AND
/// carries a wire entity key (a by-id/by-key <see cref="Guid"/>, a group <see cref="long"/>, or a
/// collection of them — at the root or inside a TrackHub-owned DTO member) could reference another
/// tenant's entity, so it MUST declare how its scope is enforced:
/// <see cref="AccountScopeEnforcedInHandlerAttribute"/> (the handler loads the entity and checks
/// caller access), <see cref="PlatformScopedAttribute"/> (platform-owned data), or
/// <see cref="AllowCrossAccountAttribute"/> (a service-identity cross-tenant surface). A keyless
/// request derives its account from the caller and needs no marker. This fails the build the
/// instant a new keyed request is added without declaring its scope — the guard that catches the
/// next by-id escape at test time, since handler-level unit tests never run the pipeline.</item>
/// <item><see cref="CachedUnscopedRequests"/> — the SVD-09 class. The cache key is built from the
/// REQUEST's fields only, so a <see cref="CachingAttribute"/> response for a request whose real
/// scope comes from the caller (caller-scoped, no resolvable account) or from a handler-side
/// ownership check (<see cref="AccountScopeEnforcedInHandlerAttribute"/>) is served across
/// callers — a cross-account cache leak. Such requests must not be cached.</item>
/// </list>
/// <para>
/// The wire-key walk deliberately shares <see cref="RequestAccountResolver"/>'s reach (same depth
/// bound, same TrackHub-owned-member descent): a key nested inside a DTO the resolver would walk is
/// found here too, so wrapping the key in a <c>FiltersInput</c>-style DTO cannot slip a keyed
/// request past the gate.
/// </para>
/// </summary>
public static class AccountScopeCoverage
{
    /// <summary>
    /// Request types in <paramref name="applicationAssembly"/> that carry a wire entity key,
    /// resolve no <c>AccountId</c>, and declare no scope marker — the by-id cross-tenant escape
    /// shape. Must be empty.
    /// </summary>
    public static IReadOnlyList<string> UndeclaredKeyedRequests(Assembly applicationAssembly)
        => RequestTypes(applicationAssembly)
            .Where(t => !RequestAccountResolver.NamesAccount(t)
                && CarriesWireKey(t)
                && t.GetCustomAttribute<AccountScopeEnforcedInHandlerAttribute>(inherit: true) is null
                && t.GetCustomAttribute<PlatformScopedAttribute>(inherit: true) is null
                && t.GetCustomAttribute<AllowCrossAccountAttribute>(inherit: true) is null)
            .Select(t => t.FullName!)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Request types in <paramref name="applicationAssembly"/> marked <see cref="CachingAttribute"/>
    /// whose scope is enforced per-caller — either caller-scoped (no resolvable account, no
    /// pass-through marker) or <see cref="AccountScopeEnforcedInHandlerAttribute"/> (the cache
    /// short-circuits the handler that performs the ownership check). Must be empty: the cache key
    /// carries only request fields, so caching such a request leaks one caller's response to
    /// another (SVD-09). Must be empty.
    /// </summary>
    public static IReadOnlyList<string> CachedUnscopedRequests(Assembly applicationAssembly)
        => RequestTypes(applicationAssembly)
            .Where(t => t.GetCustomAttribute<CachingAttribute>(inherit: true) is not null
                && (t.GetCustomAttribute<AccountScopeEnforcedInHandlerAttribute>(inherit: true) is not null
                    || (!RequestAccountResolver.NamesAccount(t)
                        && t.GetCustomAttribute<PlatformScopedAttribute>(inherit: true) is null
                        && t.GetCustomAttribute<AllowCrossAccountAttribute>(inherit: true) is null)))
            .Select(t => t.FullName!)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

    private static IEnumerable<Type> RequestTypes(Assembly assembly)
        => assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                && type.GetInterfaces().Any(i =>
                    i == typeof(IRequest)
                    || (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))));

    /// <summary>
    /// A wire entity key: a <c>Guid</c>/<c>Guid?</c>, <c>long</c>/<c>long?</c>, or a collection
    /// (excluding <see cref="string"/>) — anything that could name another tenant's entity — at the
    /// request root or inside a TrackHub-owned complex member, walked breadth-first to the same
    /// depth bound as <see cref="RequestAccountResolver"/>. Paging ints and filter strings are not
    /// keys and do not require a marker (those requests are scoped to the caller's own account).
    /// </summary>
    private static bool CarriesWireKey(Type requestType)
    {
        var frontier = new List<Type> { requestType };

        for (var depth = 0; depth <= RequestAccountResolver.MaxNestingDepth && frontier.Count > 0; depth++)
        {
            var next = new List<Type>();

            foreach (var owner in frontier)
            {
                if (HasOwnWireKey(owner))
                {
                    return true;
                }

                if (depth == RequestAccountResolver.MaxNestingDepth)
                {
                    continue;
                }

                next.AddRange(RequestAccountResolver.GetRecursableProperties(owner)
                    .Select(property => RequestAccountResolver.UnwrapNullable(property.PropertyType)));
            }

            frontier = next;
        }

        return false;
    }

    private static bool HasOwnWireKey(Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => RequestAccountResolver.IsReadable(property))
            .Select(property => RequestAccountResolver.UnwrapNullable(property.PropertyType))
            .Any(propertyType => propertyType == typeof(Guid)
                || propertyType == typeof(long)
                || (propertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyType)));
}
