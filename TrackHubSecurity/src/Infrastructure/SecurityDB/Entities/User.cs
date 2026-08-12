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

namespace TrackHub.Security.Infrastructure.Entities;

public sealed class User(string username,
    string password,
    string emailAddress,
    string firstName,
    string? secondName,
    string lastName,
    string? secondSurname,
    DateOnly? dOB,
    bool active,
    int loginAttempts,
    Guid accountId,
    bool integrationUser = false) : BaseAuditableEntity
{

    public Guid UserId { get; private set; } = Guid.NewGuid();
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
    public string EmailAddress { get; set; } = emailAddress;
    public string FirstName { get; set; } = firstName;
    public string? SecondName { get; set; } = secondName;
    public string LastName { get; set; } = lastName;
    public string? SecondSurname { get; set; } = secondSurname;
    public DateOnly? DOB { get; set; } = dOB;
    public DateTimeOffset? Verified { get; set; }
    public bool Active { get; set; } = active;
    public int LoginAttempts { get; set; } = loginAttempts;
    public DateTimeOffset? LockedUntil { get; set; }
    public Guid AccountId { get; set; } = accountId;
    public bool IntegrationUser { get; set; } = integrationUser;
    public Client? Client { get; set; }
    public ICollection<Role> Roles { get; } = [];
    public ICollection<Policy> Policies { get; } = [];
}
