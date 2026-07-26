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
/// Declares that the decorated request is allowed to carry an <c>AccountId</c> that differs from
/// the calling principal's account — i.e. it is a deliberate cross-tenant surface.
/// <para>
/// <see cref="Common.Application.Behaviors.AccountScopeBehavior{TRequest, TResponse}"/> enforces
/// tenant scope fail-closed for every request exposing an <c>AccountId</c>; this attribute is the
/// ONLY opt-out. The <paramref name="justification"/> is mandatory by design: every cross-tenant
/// entry point in the platform must state why it crosses tenants, and the full inventory is one
/// <c>grep -r "AllowCrossAccount"</c> away for review.
/// </para>
/// <para>
/// Typical legitimate uses: a global service identity (Router/SyncWorker) that iterates every
/// account and pushes per-account batches under one token; platform/master administration queries;
/// background jobs whose ambient principal carries no account; anonymous public-link resolution.
/// Partner/tenant-bound service clients must NOT be marked — they are seeded with an account
/// restriction and are expected to fail closed when they address another tenant.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public sealed class AllowCrossAccountAttribute : Attribute
{
    /// <param name="justification">
    /// Why this request legitimately operates outside the caller's own account. Required and
    /// non-blank — it is the audit trail for a deliberately unguarded tenant boundary.
    /// </param>
    public AllowCrossAccountAttribute(string justification)
    {
        if (string.IsNullOrWhiteSpace(justification))
        {
            throw new ArgumentException(
                "A cross-account declaration requires a justification explaining why the request may act outside the caller's account.",
                nameof(justification));
        }

        Justification = justification;
    }

    /// <summary>
    /// The stated reason this request may act outside the calling principal's account.
    /// </summary>
    public string Justification { get; }
}
