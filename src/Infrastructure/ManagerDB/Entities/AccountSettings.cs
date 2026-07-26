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

public class AccountSettings (Guid accountId) : BaseAuditableEntity
{
    public Guid AccountId { get; set; } = accountId;
    public string Maps { get; set; } = "OSM";
    public string? MapsKey { get; set; } = "";
    public int OnlineInterval { get; set; } = 60;
    public bool RefreshMap { get; set; } = false;
    public int RefreshMapInterval { get; set; } = 60;

    public Account Account
    {
        get => field ?? throw new InvalidOperationException("Account is not loaded");
        set;
    }
}
