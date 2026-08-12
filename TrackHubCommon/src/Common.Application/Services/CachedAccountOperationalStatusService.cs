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
using Microsoft.Extensions.Caching.Memory;

namespace Common.Application.Services;

/// <summary>
/// Shared cache over a per-service <see cref="IAccountOperationalStatusReader"/>, mirroring the
/// 30-second TTL of the Manager feature-flag service. Enforcing services register this as their
/// <see cref="IAccountOperationalStatusService"/> and only need to supply the reader. Requires
/// <c>services.AddMemoryCache()</c> in the service's DI (the enforcing services' status wire-up
/// registers it).
/// </summary>
public sealed class CachedAccountOperationalStatusService(
    IAccountOperationalStatusReader reader,
    IMemoryCache cache) : IAccountOperationalStatusService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    public async Task<AccountStatus?> GetStatusAsync(Guid accountId, CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty)
        {
            return null;
        }

        var cacheKey = $"account-status:{accountId:N}";
        if (cache.TryGetValue<AccountStatus?>(cacheKey, out var cached))
        {
            return cached;
        }

        var status = await reader.GetAccountStatusAsync(accountId, cancellationToken);
        cache.Set(cacheKey, status, CacheTtl);
        return status;
    }
}
