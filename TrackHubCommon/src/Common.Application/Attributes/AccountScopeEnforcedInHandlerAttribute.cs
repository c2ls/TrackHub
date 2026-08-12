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
/// Declares that the decorated request carries a wire identifier for a tenant-owned entity (a by-id
/// key, a by-key lookup, or a collection of them) and that its tenant scope is enforced INSIDE THE
/// HANDLER: the handler or its reader loads the referenced entity, resolves its owning account, and
/// enforces caller access — either through the
/// <c>AccountScopedDataAccess.RequireAccountAccess</c> reader guard, or by filtering the query on the
/// caller's account, or by an explicit <c>AccountId</c> comparison that throws
/// <see cref="Common.Application.Exceptions.ForbiddenAccessException"/> on a mismatch.
/// <para>
/// <see cref="Common.Application.Behaviors.AccountScopeBehavior{TRequest, TResponse}"/> runs BEFORE
/// the handler and cannot load the entity, so it cannot police such a request itself — the request
/// names no account, and the account is only knowable once the keyed row is read. This attribute is
/// the explicit, greppable record that the enforcement has been placed and verified in the handler.
/// </para>
/// <para>
/// The cited enforcement point is LOAD-BEARING: removing it (or adding a new keyed request without
/// it) reopens the exact by-id cross-tenant surface this finding exists to close. A keyed request
/// that lacks the marker is denied the benefit of review and must not silently rely on the behavior.
/// Do not use this attribute to wave a request through: apply it only after confirming the handler
/// enforces caller access to the referenced entity, and keep that enforcement in place.
/// </para>
/// <para>
/// This marker does NOT admit account-less principals. The handler's ownership check needs a caller
/// account to check AGAINST, so the behavior still requires the principal to carry an account scope
/// and denies a global service identity exactly as it would an unmarked request. A service identity
/// that legitimately needs a keyed lookup must use a dedicated request marked
/// <see cref="AllowCrossAccountAttribute"/> — never this attribute.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public sealed class AccountScopeEnforcedInHandlerAttribute : Attribute;
