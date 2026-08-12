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

using Common.Infrastructure;

namespace TrackHub.Security.Infrastructure.Entities;

public sealed class ServiceClientPermission(
    string clientId,
    Guid? accountId,
    string resource,
    string action,
    string scope,
    string audience,
    bool active,
    bool allowCrossAccount = false) : BaseAuditableEntity
{
    public Guid ServiceClientPermissionId { get; private set; } = Guid.NewGuid();
    public string ClientId { get; set; } = clientId;
    /// <summary>
    /// The single account this grant is restricted to. A grant with an account is usable ONLY by a
    /// token carrying that same account claim; a grant with no account is usable only by a token
    /// that carries no account claim. A NULL account is NOT a wildcard — see
    /// <see cref="AllowCrossAccount"/>.
    /// </summary>
    public Guid? AccountId { get; set; } = accountId;

    /// <summary>
    /// Declares this grant as a platform-wide (cross-tenant) grant: it matches regardless of the
    /// token's account claim. This replaces the old implicit rule where a NULL
    /// <see cref="AccountId"/> silently matched every account, which made "global" indistinguishable
    /// from "unscoped" and gave any service client an unbounded tenant reach.
    /// </summary>
    public bool AllowCrossAccount { get; set; } = allowCrossAccount;
    public string Resource { get; set; } = resource;
    public string Action { get; set; } = action;
    public string Scope { get; set; } = scope;
    public string Audience { get; set; } = audience;
    public bool Active { get; set; } = active;
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
}
