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

using Common.Application.Interfaces;
using Common.Domain.Helpers;
using TrackHub.Manager.Infrastructure.Entities;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Readers;

// AccountSettingsReader class for reading account settings data
public sealed class AccountSettingsReader(IApplicationDbContext context, ICurrentPrincipal principal)
    : AccountScopedDataAccess(context, principal), IAccountSettingsReader
{
    /// <summary>
    /// Retrieves an account settings by ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns an AccountSettingsVm object</returns>
    public async Task<AccountSettingsVm> GetAccountSettingsAsync(Guid id, CancellationToken cancellationToken)
    {
        // By-id tenant guard ([AccountScopeEnforcedInHandler] on GetAccountSettingsQuery cites
        // this); the master path is the Filters overload below (accountSettingsMaster surface).
        RequireAccountAccess(id);

        var accountSettings = await Context.AccountSettings
            .Where(a => a.AccountId.Equals(id))
            .FirstAsync(cancellationToken);

        return ToVm(accountSettings);
    }

    /// <summary>
    /// Retrieves a collection of account settings
    /// </summary>
    /// <param name="filters">Filters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns a collection of AccountSettingsVm objects</returns>
    public async Task<IReadOnlyCollection<AccountSettingsVm>> GetAccountSettingsAsync(Filters filters, CancellationToken cancellationToken)
    {
        var query = Context.AccountSettings.AsQueryable();
        query = filters.Apply(query);

        var settings = await query.ToListAsync(cancellationToken);
        var result = new List<AccountSettingsVm>();

        foreach (var accountSettings in settings)
        {
            result.Add(ToVm(accountSettings));
        }

        return result;
    }

    private static AccountSettingsVm ToVm(AccountSettings accountSettings)
        => new(
            accountSettings.AccountId,
            accountSettings.Maps,
            accountSettings.MapsKey,
            accountSettings.OnlineInterval,
            accountSettings.RefreshMap,
            accountSettings.RefreshMapInterval);
}
