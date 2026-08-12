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

using TrackHub.AuthorityServer.Domain.Interfaces;
using TrackHub.AuthorityServer.Domain.Models;

namespace TrackHub.AuthorityServer.Infrastructure.Readers;

public sealed class ServiceClientPermissionReader(SecurityDbContext context) : IServiceClientPermissionReader
{
    // Reads the currently effective grants of a service client and reduces them to the account
    // reach the token must declare. Only active, in-effect rows count: an expired or deactivated
    // grant must not keep a token account-bound after Security revoked it.
    public async Task<ServiceClientAccountScopeVm> GetAccountScopeAsync(string clientId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var grants = await context.ServiceClientPermissions
            .AsNoTracking()
            .Where(permission => permission.Active
                && permission.ClientId == clientId
                && (!permission.EffectiveFrom.HasValue || permission.EffectiveFrom <= now)
                && (!permission.EffectiveTo.HasValue || permission.EffectiveTo >= now))
            .Select(permission => new { permission.AllowCrossAccount, permission.AccountId })
            .Distinct()
            .ToListAsync(cancellationToken);

        return new ServiceClientAccountScopeVm(
            grants.Exists(grant => grant.AllowCrossAccount),
            grants
                .Where(grant => !grant.AllowCrossAccount && grant.AccountId.HasValue)
                .Select(grant => grant.AccountId!.Value)
                .Distinct()
                .ToArray());
    }
}
