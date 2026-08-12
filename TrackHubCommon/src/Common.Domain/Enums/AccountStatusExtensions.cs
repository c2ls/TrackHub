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

namespace Common.Domain.Enums;

public static class AccountStatusExtensions
{
    /// <summary>
    /// True when the account is in an operational state (<see cref="AccountStatus.Trial"/> or
    /// <see cref="AccountStatus.Active"/>) and account-scoped operations are permitted. All other
    /// states are non-operational and are blocked by the account-status enforcement pipeline
    /// (except allow-listed billing/support/read-own-status/lifecycle operations).
    /// </summary>
    public static bool IsOperational(this AccountStatus status)
        => status is AccountStatus.Trial or AccountStatus.Active;

    /// <summary>
    /// Maps the legacy <c>Active</c> boolean onto a lifecycle status. Used by migration backfill and
    /// by <c>UpdateAccount</c>, which may only move an account within the operational statuses.
    /// </summary>
    public static AccountStatus FromActiveFlag(bool active)
        => active ? AccountStatus.Active : AccountStatus.Suspended;
}
