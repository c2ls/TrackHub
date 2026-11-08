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

public sealed class StaffCredential(
    Guid staffId,
    Guid accountId,
    string normalizedLogin,
    string passwordHash,
    bool active) : BaseAuditableEntity
{
    public Guid StaffCredentialId { get; private set; } = Guid.NewGuid();
    public Guid StaffId { get; set; } = staffId;
    public Guid AccountId { get; set; } = accountId;
    public string NormalizedLogin { get; set; } = normalizedLogin;
    public string PasswordHash { get; set; } = passwordHash;
    public int FailedAttempts { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public bool Active { get; set; } = active;
    public bool ResetRequired { get; set; }
    public Guid SecurityStamp { get; set; } = Guid.NewGuid();
}
