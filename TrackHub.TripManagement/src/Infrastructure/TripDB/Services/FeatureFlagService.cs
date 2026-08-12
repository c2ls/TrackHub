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
using Microsoft.Extensions.Caching.Memory;

namespace TrackHub.TripManagement.Infrastructure.TripDB.Services;

/// <summary>
/// TripManagement's own <see cref="IFeatureFlagService"/>, backed by the Manager-owned
/// app.account_features table.
/// <para>
/// <b>This registration is load-bearing, not an optimisation</b> (spec 11 section 15, the row
/// marked critical; acceptance 10). Common registers a FAIL-OPEN AlwaysEnabledFeatureFlagService
/// via TryAddScoped, so a service that does not override it has every [RequireFeature] silently
/// pass. Without this class, every trip surface would be reachable by an account that never
/// bought trip-management. Do not remove it, and do not let the DI registration regress to
/// TryAddScoped - the override must win.
/// </para>
/// <para>
/// Decisions are cached for 30 seconds per (accountId, featureKey) because every command and query
/// goes through the FeatureFlagBehavior; missing rows are treated as DISABLED, and
/// EffectiveFrom/EffectiveTo windows are honoured (the Manager FeatureFlagService shape).
/// </para>
/// </summary>
public sealed class FeatureFlagService(IApplicationDbContext context, IMemoryCache cache) : IFeatureFlagService
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    public async Task<bool> IsEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty || string.IsNullOrWhiteSpace(featureKey))
        {
            return false;
        }

        var cacheKey = $"feature-flag:{accountId:N}:{featureKey}";
        if (cache.TryGetValue<bool>(cacheKey, out var cached))
        {
            return cached;
        }

        var now = DateTimeOffset.UtcNow;
        var enabled = await context.AccountFeatures.AnyAsync(x =>
            x.AccountId == accountId
            && x.FeatureKey == featureKey
            && x.Enabled
            && (!x.EffectiveFrom.HasValue || x.EffectiveFrom <= now)
            && (!x.EffectiveTo.HasValue || x.EffectiveTo >= now), cancellationToken);

        cache.Set(cacheKey, enabled, CacheTtl);
        return enabled;
    }
}
