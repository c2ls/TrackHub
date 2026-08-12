// Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
public sealed class User(
    string username,
    string password,
    string emailAddress,
    Guid accountId) : BaseAuditableEntity
{
    public Guid UserId { get; private set; } = Guid.NewGuid();
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
    public string EmailAddress { get; set; } = emailAddress;
    public DateTimeOffset? Verified { get; set; }
    public bool Active { get; set; }
    public int LoginAttempts { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public Guid AccountId { get; set; } = accountId;
    public Client? Client { get; set; }
}
