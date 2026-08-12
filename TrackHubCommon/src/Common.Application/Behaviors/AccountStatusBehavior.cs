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
using Common.Domain.Enums;
using Common.Mediator;
using Microsoft.Extensions.Logging;

namespace Common.Application.Behaviors;

/// <summary>
/// Fail-closed pipeline behavior that blocks account-scoped operations whose resolved account is
/// non-operational (<see cref="AccountStatus.Suspended"/>/<see cref="AccountStatus.Cancelled"/>/
/// <see cref="AccountStatus.Archived"/>), throwing <see cref="AccountSuspendedException"/>
/// (GraphQL <c>ACCOUNT_SUSPENDED</c>). Enforcement is opt-out: a request marked
/// <see cref="AllowSuspendedAccountAttribute"/> (billing/support/read-own-status/lifecycle) is always
/// allowed. The evaluated account is, in order: the request's <c>AccountId</c> property (if a
/// non-empty Guid), else the current principal's account. When no account can be resolved the check
/// is skipped (platform-level operations). Mirrors <see cref="FeatureFlagBehavior{TRequest, TResponse}"/>.
/// </summary>
public sealed class AccountStatusBehavior<TRequest, TResponse>(
    IAccountOperationalStatusService statusService,
    ICurrentPrincipal principal,
    ILogger<AccountStatusBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private static readonly ConcurrentDictionary<Type, bool> AllowSuspendedCache = new();

    public async Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        var allowSuspended = AllowSuspendedCache.GetOrAdd(typeof(TRequest), static t =>
            t.GetCustomAttributes<AllowSuspendedAccountAttribute>(inherit: true).Any());
        if (allowSuspended)
        {
            return await next();
        }

        var accountId = ResolveAccountId(request);
        if (accountId is null)
        {
            return await next();
        }

        var status = await statusService.GetStatusAsync(accountId.Value, cancellationToken);
        if (status is not null && !status.Value.IsOperational())
        {
            logger.LogWarning(
                "Account {AccountId} is non-operational (status {Status}); blocking {RequestType}.",
                accountId.Value, status.Value, typeof(TRequest).FullName);
            throw new AccountSuspendedException(accountId.Value, status.Value);
        }

        return await next();
    }

    private Guid? ResolveAccountId(TRequest request)
        => RequestAccountResolver.GetRequestAccountId(request) ?? principal.AccountId;
}
