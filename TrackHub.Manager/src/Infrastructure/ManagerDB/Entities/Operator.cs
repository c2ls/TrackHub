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

namespace TrackHub.Manager.Infrastructure.Entities;

public sealed class Operator(string name, string? description, string? phoneNumber, string? emailAddress, string? address, string? contactName, int protocolType, Guid accountId) : BaseAuditableEntity
{
    private Account? _account;
    public Guid OperatorId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = name;
    public string? Description { get; set; } = description;
    public string? PhoneNumber { get; set; } = phoneNumber;
    public string? EmailAddress { get; set; } = emailAddress;
    public string? Address { get; set; } = address;
    public string? ContactName { get; set; } = contactName;
    public int ProtocolType { get; set; } = protocolType;
    public Guid AccountId { get; set; } = accountId;

    public bool Enabled { get; set; } = true;
    public int SyncIntervalMinutes { get; set; } = 60;
    // Health, failure detail, latency, and failed/position sync timestamps are DERIVED from
    // telemetry (operator health checks + sync runs) at read time — the row keeps only the
    // successful-sync stamps the schedulers gate on.
    public DateTimeOffset? LastSuccessfulSyncAt { get; set; }
    public DateTimeOffset? LastManualSyncAt { get; set; }
    public DateTimeOffset? LastDeviceSyncAt { get; set; }

    public Credential? Credential { get; set; }
    public ICollection<Device> Devices { get; } = [];

    public Account Account
    {
        get => _account ?? throw new InvalidOperationException("Account is not loaded");
        set => _account = value;
    }
}
