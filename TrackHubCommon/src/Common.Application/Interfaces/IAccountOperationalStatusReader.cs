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

using Common.Domain.Enums;

namespace Common.Application.Interfaces;

/// <summary>
/// Per-service port that reads the authoritative operational status of an account. Each
/// account-scoped service implements this the same way it already implements its account/feature
/// reader: Manager/Geofencing/Telemetry over the shared Manager database directly; Reporting/Router
/// via Manager GraphQL. The cached <see cref="IAccountOperationalStatusService"/> wraps it.
/// </summary>
public interface IAccountOperationalStatusReader
{
    /// <summary>
    /// Returns the account's lifecycle status, or <c>null</c> when the account does not exist
    /// (so the status gate never masks a genuine not-found with <c>ACCOUNT_SUSPENDED</c>).
    /// </summary>
    Task<AccountStatus?> GetAccountStatusAsync(Guid accountId, CancellationToken cancellationToken);
}
