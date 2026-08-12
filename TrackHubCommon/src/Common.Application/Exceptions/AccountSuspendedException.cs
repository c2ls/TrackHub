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

namespace Common.Application.Exceptions;

/// <summary>
/// Thrown by <see cref="Common.Application.Behaviors.AccountStatusBehavior{TRequest, TResponse}"/>
/// when an account-scoped request resolves to a non-operational account. Mapped to the GraphQL
/// <c>ACCOUNT_SUSPENDED</c> error code and to HTTP 403 for REST.
/// </summary>
public sealed class AccountSuspendedException(Guid accountId, AccountStatus status)
    : Exception($"Account '{accountId}' is not operational (status: {status}).")
{
    public Guid AccountId { get; } = accountId;
    public AccountStatus Status { get; } = status;
}
