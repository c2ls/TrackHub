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

using Common.Domain.Constants;
using Microsoft.Extensions.Caching.Memory;
using TrackHub.Router.Domain.Interfaces.Manager;

namespace TrackHub.Router.Application.Gating;

/// <inheritdoc cref="IAccountModeResolver"/>
public sealed class AccountModeResolver(IAccountReader accountReader, IMemoryCache cache) : IAccountModeResolver
{
    // Short-lived: a feature toggle takes effect within a minute across the request path, without a
    // Manager round trip per operator on every 10-second map refresh.
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    public Task<bool> IsIntegrationEnabledAsync(Guid accountId, CancellationToken cancellationToken)
        => ResolveAsync(accountId, FeatureKeys.GpsIntegration, cancellationToken);

    public Task<bool> IsPositionHistoryEnabledAsync(Guid accountId, CancellationToken cancellationToken)
        => ResolveAsync(accountId, FeatureKeys.GpsPositionHistory, cancellationToken);

    private async Task<bool> ResolveAsync(Guid accountId, string featureKey, CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty)
        {
            return false;
        }

        var cacheKey = $"account-mode:{featureKey}:{accountId}";
        if (cache.TryGetValue(cacheKey, out bool cached))
        {
            return cached;
        }

        var enabled = await accountReader.IsFeatureEnabledAsync(accountId, featureKey, cancellationToken);
        cache.Set(cacheKey, enabled, CacheTtl);
        return enabled;
    }
}
