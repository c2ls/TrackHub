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

using TrackHub.Router.Domain.Models;

namespace TrackHub.Router.Application.Gating;

/// <summary>
/// Pure predicate for the provider-vs-stored map read split. The per-account "integration enabled"
/// decision is owned by <see cref="IAccountModeResolver"/>; this only turns that set
/// plus the operator's own enabled flag into the on-demand decision, so there is one source of truth.
/// </summary>
public static class GpsFeatureGate
{
    /// <summary>
    /// Returns true when map reads should contact the provider directly because background sync is not enabled.
    /// </summary>
    public static bool CanReadProviderOnDemand(
        OperatorVm @operator,
        IReadOnlySet<Guid> enabledAccountIds)
        => @operator.Enabled && !enabledAccountIds.Contains(@operator.AccountId);
}
