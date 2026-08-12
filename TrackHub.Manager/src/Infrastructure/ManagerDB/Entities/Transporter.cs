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

public sealed class Transporter(string name, short transporterTypeId, Guid accountId) : BaseAuditableEntity
{
    private Account? _account;

    public Guid TransporterId { get; private set; } = Guid.NewGuid();
    public string Name { get; set; } = name;
    public short TransporterTypeId { get; set; } = transporterTypeId;
    public Guid AccountId { get; set; } = accountId;

    public ICollection<Group> Groups { get; set; } = [];
    public ICollection<TransporterDeviceAssignment> Assignments { get; set; } = [];
    public TransporterPosition? Position { get; set; }
    public TransporterType? TransporterType { get; set; }

    public Account Account
    {
        get => _account ?? throw new InvalidOperationException("Account is not loaded");
        set => _account = value;
    }
}
