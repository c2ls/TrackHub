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

using HotChocolate;

namespace TrackHub.Geofencing.Infrastructure.Readers;

public sealed class AccountFeatureReader(IApplicationDbContext context) : IAccountFeatureReader
{
    public async Task EnsureFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var enabled = await context.AccountFeatures.AnyAsync(x =>
            x.AccountId == accountId
            && x.FeatureKey == featureKey
            && x.Enabled
            && (!x.EffectiveFrom.HasValue || x.EffectiveFrom <= now)
            && (!x.EffectiveTo.HasValue || x.EffectiveTo >= now),
            cancellationToken);

        if (!enabled)
        {
            throw new GraphQLException(ErrorBuilder.New()
                .SetMessage("This feature is not enabled for your account.")
                .SetCode("FEATURE_DISABLED")
                .Build());
        }
    }
}

