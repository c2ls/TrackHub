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
using Common.Domain.Enums;

namespace Common.Application.Services;

/// <summary>
/// Fail-open default registered by <c>AddApplicationServices</c> via <c>TryAddScoped</c>. Services
/// that do not enforce account status (e.g. AuthorityServer, Security) keep this no-op resolver, so
/// the shared <see cref="Common.Application.Behaviors.AccountStatusBehavior{TRequest, TResponse}"/>
/// never blocks them. Enforcing services override it with a real cached implementation.
/// </summary>
public sealed class AlwaysOperationalAccountStatusService : IAccountOperationalStatusService
{
    public Task<AccountStatus?> GetStatusAsync(Guid accountId, CancellationToken cancellationToken)
        => Task.FromResult<AccountStatus?>(AccountStatus.Active);
}
