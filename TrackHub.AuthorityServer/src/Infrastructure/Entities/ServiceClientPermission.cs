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

namespace TrackHub.AuthorityServer.Infrastructure.Entities;

// Read-only projection of security.service_client_permissions — only the columns token issuance
// needs to decide the account scope of a client-credentials token. Security owns the table.
public sealed class ServiceClientPermission
{
    public Guid ServiceClientPermissionId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public Guid? AccountId { get; set; }
    public bool AllowCrossAccount { get; set; }
    public bool Active { get; set; }
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
}
