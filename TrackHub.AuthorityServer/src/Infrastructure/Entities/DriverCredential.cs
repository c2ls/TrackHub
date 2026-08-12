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

namespace TrackHub.AuthorityServer.Infrastructure.Entities;

public sealed class DriverCredential : BaseAuditableEntity
{
    public Guid DriverCredentialId { get; private set; }
    public Guid DriverId { get; set; }
    public Guid AccountId { get; set; }
    public string NormalizedLogin { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int FailedAttempts { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool Active { get; set; }
    public bool ResetRequired { get; set; }
}
