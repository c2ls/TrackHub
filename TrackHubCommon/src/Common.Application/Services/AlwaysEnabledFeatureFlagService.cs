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

namespace Common.Application.Services;

/// <summary>
/// Default implementation registered when no service has supplied a real
/// <see cref="IFeatureFlagService"/>. Always returns <c>true</c> so services that do
/// not own a feature flag store continue to function. The Manager registers a
/// DB-backed override.
/// </summary>
public sealed class AlwaysEnabledFeatureFlagService : IFeatureFlagService
{
    public Task<bool> IsEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken)
        => Task.FromResult(true);
}
