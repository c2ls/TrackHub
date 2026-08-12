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
using Common.Domain.Enums;

namespace TrackHub.Geofencing.Infrastructure.Readers;

// Reads Manager account status directly over the shared database. Backs the cached
// IAccountOperationalStatusService that AccountStatusBehavior consumes to fail-close suspended accounts.
public sealed class AccountOperationalStatusReader(IApplicationDbContext context) : IAccountOperationalStatusReader
{
    public async Task<AccountStatus?> GetAccountStatusAsync(Guid accountId, CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty)
        {
            return null;
        }

        var status = await context.Accounts
            .Where(a => a.AccountId == accountId)
            .Select(a => (short?)a.Status)
            .FirstOrDefaultAsync(cancellationToken);

        return status.HasValue ? (AccountStatus)status.Value : null;
    }
}
