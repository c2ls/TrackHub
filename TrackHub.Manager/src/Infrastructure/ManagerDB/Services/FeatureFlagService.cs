// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using Common.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using TrackHub.Manager.Infrastructure.Interfaces;

namespace TrackHub.Manager.Infrastructure.ManagerDB.Services;

/// <summary>
/// Manager-side <see cref="IFeatureFlagService"/> backed by the <c>account_features</c> table.
/// Decisions are cached for a short window per <c>(accountId, featureKey)</c> tuple to keep
/// the pipeline behavior cheap on hot paths (every command/query goes through it).
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
