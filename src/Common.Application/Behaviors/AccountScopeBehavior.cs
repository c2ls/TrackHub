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

using System.Collections.Concurrent;
using System.Reflection;
using Common.Application.Attributes;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Mediator;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviors;

/// <summary>
/// Central tenant-scope guard. Runs immediately AFTER
/// <see cref="AuthorizationBehavior{TRequest, TResponse}"/> (the principal must already be
/// established and its permission grant verified) and before every other behavior and the handler.
/// <para>
/// Authorization answers "may this caller perform this action?"; it says nothing about WHICH
/// tenant's data the caller named. The premise of the platform is that every business request
/// belongs to an account — resolvable EITHER from the request (an <c>AccountId</c> field) or from
/// the caller's identity. This guard establishes that account and denies anything it cannot.
/// </para>
/// <para>The rules, in order:</para>
/// <list type="number">
/// <item><b>The request names an account</b> (an <c>AccountId</c> within reach). It MUST equal the
/// principal's own account, unless the type is marked <see cref="AllowCrossAccountAttribute"/> (a
/// deliberate, audited cross-tenant surface). A mismatch, or a principal carrying no account at all,
/// throws <see cref="ForbiddenAccessException"/>.</item>
/// <item><b>The request names no account</b> but is marked
/// <see cref="AllowCrossAccountAttribute"/> (service-identity cross-tenant surface) or
/// <see cref="PlatformScopedAttribute"/> (platform-owned data, identical for every tenant) —
/// pass, the marker (and its mandatory justification) is the record of why.</item>
/// <item><b>The request names no account and carries no pass-through marker</b> — the account is
/// derived from the CALLER'S identity: the handler is trusted to scope to
/// <c>ICurrentPrincipal.AccountId</c>, which it cannot cross. This is the ordinary tenant request
/// that simply reads its account from the token. It passes ONLY when the principal actually has an
/// account; a principal with no account scope (a global service identity) reaching an account-less
/// request cannot be scoped and is denied — the fail-closed line that still catches the
/// service-identity escape shapes. <see cref="AccountScopeEnforcedInHandlerAttribute"/> deliberately
/// does NOT change this outcome: it documents WHERE the by-id ownership check lives (the handler),
/// but the handler still needs a caller account to check AGAINST, so a no-account principal is
/// denied all the same — a service identity that legitimately needs such a surface must use a
/// dedicated <see cref="AllowCrossAccountAttribute"/> request instead.</item>
/// </list>
/// </summary>
public sealed class AccountScopeBehavior<TRequest, TResponse>(
    ICurrentPrincipal principal,
    ILogger<AccountScopeBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private static readonly ConcurrentDictionary<Type, AllowCrossAccountAttribute?> CrossAccountCache = new();
    private static readonly ConcurrentDictionary<Type, PlatformScopedAttribute?> PlatformScopedCache = new();
    private static readonly ConcurrentDictionary<Type, bool> ScopeEnforcedInHandlerCache = new();

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        var requestAccountId = RequestAccountResolver.GetRequestAccountId(request);
        if (requestAccountId is not null)
        {
            // The request names a concrete account off the wire — enforce it against the principal.
            var crossAccount = IsCrossAccount(typeof(TRequest));
            if (crossAccount is not null)
            {
                logger.LogDebug(
                    "Cross-account call permitted for {RequestType} targeting account {AccountId} by principal {PrincipalType} (account {PrincipalAccountId}). Justification: {Justification}",
                    typeof(TRequest).FullName,
                    requestAccountId.Value,
                    principal.PrincipalType,
                    principal.AccountId,
                    crossAccount.Justification);
                return await next();
            }

            if (principal.AccountId is not { } principalAccountId || principalAccountId == Guid.Empty)
            {
                logger.LogWarning(
                    "Tenant scope denied for {RequestType}: the request targets account {AccountId} but principal {PrincipalType} ('{SubjectId}') carries no account scope.",
                    typeof(TRequest).FullName,
                    requestAccountId.Value,
                    principal.PrincipalType,
                    principal.ClientId ?? principal.SubjectId);
                throw new ForbiddenAccessException(
                    "Insufficient permissions. The caller has no account scope and may not act on a specific account.");
            }

            if (principalAccountId != requestAccountId.Value)
            {
                logger.LogWarning(
                    "Tenant scope denied for {RequestType}: principal {PrincipalType} ('{SubjectId}') of account {PrincipalAccountId} targeted account {AccountId}.",
                    typeof(TRequest).FullName,
                    principal.PrincipalType,
                    principal.ClientId ?? principal.SubjectId,
                    principalAccountId,
                    requestAccountId.Value);
                throw new ForbiddenAccessException(
                    "Insufficient permissions. The requested account is outside the caller's account scope.");
            }

            return await next();
        }

        // The request names no account in its fields. A declared pass-through surface passes on
        // its marker (each marker's constructor forces a recorded justification):
        if (IsCrossAccount(typeof(TRequest)) is not null)     // service-identity cross-tenant surface
        {
            return await next();
        }

        if (IsPlatformScoped(typeof(TRequest)) is { } platformScoped)  // platform-owned data, identical for every tenant
        {
            logger.LogDebug(
                "Platform-scoped request {RequestType} passed for principal {PrincipalType} (account {PrincipalAccountId}). Justification: {Justification}",
                typeof(TRequest).FullName,
                principal.PrincipalType,
                principal.AccountId,
                platformScoped.Justification);
            return await next();
        }

        // Otherwise the account is derived from the caller's identity: the handler scopes to the
        // principal's own account, which it cannot cross. This holds ONLY when the principal has an
        // account — a global service identity with no account scope reaching an account-less request
        // cannot be scoped, so it is denied (the fail-closed line). [AccountScopeEnforcedInHandler]
        // lands here deliberately: the handler enforces the by-id ownership check, but it still
        // needs a caller account to check against.
        if (principal.AccountId is { } callerAccountId && callerAccountId != Guid.Empty)
        {
            return await next();
        }

        var enforcedInHandler = IsScopeEnforcedInHandler(typeof(TRequest));
        logger.LogWarning(
            "Tenant scope denied for {RequestType}: the request names no account and principal {PrincipalType} " +
            "('{SubjectId}') has no account scope to derive it from.{HandlerEnforcedHint}",
            typeof(TRequest).FullName,
            principal.PrincipalType,
            principal.ClientId ?? principal.SubjectId,
            enforcedInHandler
                ? " [AccountScopeEnforcedInHandler] does not admit account-less principals — the handler's " +
                  "ownership check needs a caller account to check against; a service identity that must reach " +
                  "this data needs a dedicated [AllowCrossAccount] request."
                : string.Empty);
        throw new ForbiddenAccessException(
            "Insufficient permissions. The request names no account and the caller carries no account scope.");
    }

    private static AllowCrossAccountAttribute? IsCrossAccount(Type requestType)
        => CrossAccountCache.GetOrAdd(requestType, static t =>
            t.GetCustomAttribute<AllowCrossAccountAttribute>(inherit: true));

    private static PlatformScopedAttribute? IsPlatformScoped(Type requestType)
        => PlatformScopedCache.GetOrAdd(requestType, static t =>
            t.GetCustomAttribute<PlatformScopedAttribute>(inherit: true));

    private static bool IsScopeEnforcedInHandler(Type requestType)
        => ScopeEnforcedInHandlerCache.GetOrAdd(requestType, static t =>
            t.GetCustomAttribute<AccountScopeEnforcedInHandlerAttribute>(inherit: true) is not null);
}
