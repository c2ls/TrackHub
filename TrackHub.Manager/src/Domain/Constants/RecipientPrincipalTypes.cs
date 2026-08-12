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
/// Recipient principal types on NotificationDelivery/AlertSubscription. Role-addressed deliveries
/// fan out at read time (feed matches the caller's token role within the account) — Manager stores
/// no per-user role data, so role selectors never enumerate users.
/// </summary>
public static class RecipientPrincipalTypes
{
    public const string User = nameof(User);
    public const string Driver = nameof(Driver);
    public const string Role = nameof(Role);
    /// <summary>Explicit contact endpoint from a rule selector; Recipient holds the address.</summary>
    public const string Contact = nameof(Contact);
    /// <summary>Rule-level webhook delivery; Recipient holds the target URL.</summary>
    public const string Rule = nameof(Rule);

    public static readonly IReadOnlyCollection<string> All = [User, Driver, Role, Contact, Rule];

    public static bool IsValid(string? value) => value != null && All.Contains(value);
}
