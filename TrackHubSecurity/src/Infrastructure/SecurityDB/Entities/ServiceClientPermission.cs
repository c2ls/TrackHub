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

namespace TrackHub.Security.Infrastructure.SecurityDB.Entities;

public sealed class ServiceClientPermission(
    string clientId,
    Guid? accountId,
    string resource,
    string action,
    string scope,
    string audience,
    bool active) : BaseAuditableEntity
{
    public Guid ServiceClientPermissionId { get; private set; } = Guid.NewGuid();
    public string ClientId { get; set; } = clientId;
    public Guid? AccountId { get; set; } = accountId;
    public string Resource { get; set; } = resource;
    public string Action { get; set; } = action;
    public string Scope { get; set; } = scope;
    public string Audience { get; set; } = audience;
    public bool Active { get; set; } = active;
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
}
