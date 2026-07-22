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

namespace TrackHub.TripManagement.Domain.Interfaces;

/// <summary>
/// Reads <c>app.account_features</c> (Manager-owned, mapped read-only here) for explicit
/// feature checks and for the per-account job configuration in <c>ConfigurationJson</c>.
/// </summary>
public interface IAccountFeatureReader
{
    /// <summary>Throws <c>FeatureDisabledException</c> when the account lacks the key.</summary>
    Task EnsureFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken);

    Task<bool> IsFeatureEnabledAsync(Guid accountId, string featureKey, CancellationToken cancellationToken);

    /// <summary>Accounts with <c>trip-management</c> live — the working set for both jobs.</summary>
    Task<IReadOnlyCollection<Guid>> GetEnabledAccountIdsAsync(string featureKey, CancellationToken cancellationToken);

    /// <summary>
    /// The feature's <c>ConfigurationJson</c>, which carries this module's per-account settings:
    /// <c>delayThresholdMinutes</c> (default 15), <c>scheduleLeadMinutes</c> (default 60) and
    /// <c>tollMatchToleranceMeters</c> (default 100).
    /// </summary>
    Task<TripAccountConfigVm> GetAccountConfigAsync(Guid accountId, CancellationToken cancellationToken);
}

/// <summary>Per-account trip settings, parsed from the feature's <c>ConfigurationJson</c>.</summary>
public readonly record struct TripAccountConfigVm(
    int DelayThresholdMinutes,
    int ScheduleLeadMinutes,
    double TollMatchToleranceMeters)
{
    public const int DefaultDelayThresholdMinutes = 15;
    public const int DefaultScheduleLeadMinutes = 60;
    public const double DefaultTollMatchToleranceMeters = 100d;

    public static TripAccountConfigVm Default => new(
        DefaultDelayThresholdMinutes,
        DefaultScheduleLeadMinutes,
        DefaultTollMatchToleranceMeters);
}
