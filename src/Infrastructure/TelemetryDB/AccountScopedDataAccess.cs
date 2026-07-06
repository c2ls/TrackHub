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
using TrackHub.Telemetry.Infrastructure.TelemetryDB.Interfaces;

namespace TrackHub.Telemetry.Infrastructure.TelemetryDB;

// Account-scoping base for the Telemetry readers/writers. Mirrors Manager's primitive but without
// the support-grant / audit-event machinery (Telemetry has read-only cross-schema access to the app
// scoping tables and does not own audit_events - spec 01.3 section 5.2).
public abstract class AccountScopedDataAccess(IApplicationDbContext context, ICurrentPrincipal principal)
{
    protected IApplicationDbContext Context { get; } = context;
    protected ICurrentPrincipal Principal { get; } = principal;

    protected bool CanAccessAllAccounts => Principal.PrincipalType == PrincipalType.ServiceClient && !Principal.AccountId.HasValue;

    // Administrator/Manager roles (and global service clients) read account-wide; plain users are
    // narrowed by group membership. Same privileged-bypass rule as the map/POI reads (spec 01.3 A1).
    protected bool IsPrivileged =>
        CanAccessAllAccounts
        || string.Equals(Principal.Role, Roles.Administrator, StringComparison.OrdinalIgnoreCase)
        || string.Equals(Principal.Role, Roles.Manager, StringComparison.OrdinalIgnoreCase);

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
