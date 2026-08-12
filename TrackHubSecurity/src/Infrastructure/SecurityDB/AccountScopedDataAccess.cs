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

using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Domain.Constants;
using TrackHub.Security.Infrastructure.Interfaces;

namespace TrackHub.Security.Infrastructure;

/// <summary>
/// Base for readers/writers that touch tenant-owned rows by key. Requests marked
/// <c>[AccountScopeEnforcedInHandler]</c> cite a <see cref="RequireAccountAccess"/> call in a
/// subclass as their load-bearing enforcement point: the row is loaded first, then its owning
/// account is checked against the caller before any data is returned or mutated. Same pattern as
/// the Manager/Telemetry <c>AccountScopedDataAccess</c> bases.
/// </summary>
public abstract class AccountScopedDataAccess(IApplicationDbContext context, ICurrentPrincipal principal)
{
    protected IApplicationDbContext Context { get; } = context;
    protected ICurrentPrincipal Principal { get; } = principal;

    /// <summary>
    /// Global service identities (client-credentials tokens with no account claim) and the platform
    /// Administrator operate across accounts; every other caller is bound to its own account.
    /// </summary>
    protected bool CanAccessAllAccounts =>
        (Principal.PrincipalType == PrincipalType.ServiceClient && !Principal.AccountId.HasValue)
        || string.Equals(Principal.Role, Roles.Administrator, StringComparison.OrdinalIgnoreCase);

    protected Guid RequireAccountAccess(Guid accountId)
    {
        if (accountId == Guid.Empty)
        {
            throw new ForbiddenAccessException("Insufficient permissions. Required account access: a non-empty account id.");
        }

        if (CanAccessAllAccounts
            || Principal.AccountId == accountId
            || UserBelongsToAccount(accountId))
        {
            return accountId;
        }

        throw new ForbiddenAccessException($"Insufficient permissions. Required account access: {accountId}.");
    }

    private bool UserBelongsToAccount(Guid accountId)
        => Principal.PrincipalType == PrincipalType.User
           && Principal.UserId.HasValue
           && Context.Users.Any(x => x.UserId == Principal.UserId.Value && x.AccountId == accountId);
}
