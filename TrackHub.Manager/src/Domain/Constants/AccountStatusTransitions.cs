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

namespace TrackHub.Manager.Domain.Constants;

/// <summary>
/// The account lifecycle state machine. Enforced by the
/// <c>ChangeAccountStatusValidator</c>; a transition not present here is a validation error.
/// </summary>
public static class AccountStatusTransitions
{
    private static readonly IReadOnlyDictionary<AccountStatus, AccountStatus[]> Allowed =
        new Dictionary<AccountStatus, AccountStatus[]>
        {
            [AccountStatus.Trial] = [AccountStatus.Active, AccountStatus.Suspended, AccountStatus.Cancelled],
            [AccountStatus.Active] = [AccountStatus.Suspended, AccountStatus.Cancelled],
            [AccountStatus.Suspended] = [AccountStatus.Active, AccountStatus.Cancelled, AccountStatus.Archived],
            [AccountStatus.Cancelled] = [AccountStatus.Active, AccountStatus.Archived],
            [AccountStatus.Archived] = [],
        };

    /// <summary>True when <paramref name="to"/> is a permitted transition from <paramref name="from"/>.</summary>
    public static bool IsAllowed(AccountStatus from, AccountStatus to)
        => Allowed.TryGetValue(from, out var targets) && Array.IndexOf(targets, to) >= 0;

    /// <summary>A non-empty reason is mandatory when suspending or cancelling.</summary>
    public static bool RequiresReason(AccountStatus to)
        => to is AccountStatus.Suspended or AccountStatus.Cancelled;
}
