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
using Common.Infrastructure;

namespace TrackHub.Manager.Infrastructure.Entities;

public class Account(string name, string? description, short type, bool active) : BaseAuditableEntity
{
    public Guid AccountId { get; private set; } = Guid.NewGuid();

    public string Name { get; set; } = name;

    public string? Description { get; set; } = description;

    public short Type { get; set; } = type;

    // Legacy on/off flag, retained as a derived compatibility surface: Active == Status ∈ {Trial, Active}.
    public bool Active { get; set; } = active;

    // Authoritative operational state. Backfilled from Active on migration; seeded
    // from Active on create.
    public short Status { get; set; } = (short)AccountStatusExtensions.FromActiveFlag(active);

    public DateTimeOffset? StatusChangedAt { get; set; }

    public IEnumerable<User> Users { get; } = [];
    public IEnumerable<Group> Groups { get; } = [];
    public IEnumerable<Driver> Drivers { get; } = [];
    public IEnumerable<AccountFeature> AccountFeatures { get; } = [];
    public IEnumerable<Operator> Operators { get; } = [];
    public IEnumerable<Transporter> Transporters { get; } = [];
    public IEnumerable<Device> Devices { get; } = [];
    public AccountSettings? AccountSettings { get; set; }
    public AccountBranding? AccountBranding { get; set; }

}
