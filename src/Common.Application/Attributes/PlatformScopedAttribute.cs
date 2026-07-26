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

namespace Common.Application.Attributes;

/// <summary>
/// Declares that the decorated request reads or writes PLATFORM-OWNED data — data the platform
/// itself owns, identical for every tenant — so
/// <see cref="Common.Application.Behaviors.AccountScopeBehavior{TRequest, TResponse}"/> has nothing
/// to scope and passes it through even when the caller carries no account scope.
/// <para>
/// The <paramref name="justification"/> is mandatory by design: applying this attribute is a
/// positive claim that a specific platform owner exists for the data, and the claim must be stated
/// where the marker sits ("seeded RBAC catalog", "SVD-12 toll catalog", "SVD-10 platform status",
/// "login/token exchange", "background-job status"). A request you merely cannot find the account
/// for does NOT qualify — that absence is the guard working, not a reason to opt out of it.
/// </para>
/// <para>
/// This attribute is NOT for ordinary tenant data that merely lacks an <c>AccountId</c> on the wire:
/// </para>
/// <list type="bullet">
/// <item>A request whose account comes from the CALLER (the handler reads
/// <c>ICurrentPrincipal.AccountId</c> / <c>IUser</c>) needs NO marker — the behavior derives the
/// account from the principal automatically. Marking it here would wrongly let a principal with no
/// account scope reach it.</item>
/// <item>A by-id / by-key request whose owning account is resolved from the LOADED entity must use
/// <see cref="AccountScopeEnforcedInHandlerAttribute"/>, which names the handler's enforcement point.</item>
/// <item>A deliberate cross-tenant surface (a global service identity acting across accounts) must use
/// <see cref="AllowCrossAccountAttribute"/>, which is the audited <c>grep -r AllowCrossAccount</c>
/// inventory of tenant-boundary crossings.</item>
/// </list>
/// <para>
/// Marking an account-BEARING request with this attribute has no effect: it does not exempt a request
/// that resolves an <c>AccountId</c> (the same-account check still runs).
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public sealed class PlatformScopedAttribute : Attribute
{
    /// <param name="justification">
    /// The platform owner of the data this request touches and why no tenant owns it (e.g. "seeded
    /// RBAC catalog", "SVD-12 toll catalog", "SVD-10 platform status"). Required and non-blank —
    /// it is the audit trail for a request that bypasses tenant scoping entirely.
    /// </param>
    public PlatformScopedAttribute(string justification)
    {
        if (string.IsNullOrWhiteSpace(justification))
        {
            throw new ArgumentException(
                "A platform-scoped declaration requires a justification naming the platform owner of the data (e.g. \"seeded RBAC catalog\", \"SVD-10 platform status\").",
                nameof(justification));
        }

        Justification = justification;
    }

    /// <summary>
    /// The stated platform owner of the data and why no tenant owns it.
    /// </summary>
    public string Justification { get; }
}
