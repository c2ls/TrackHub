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

namespace TrackHub.Reporting.Domain.Models;

// Intermediate projections deserialized from Manager GraphQL for the admin/lifecycle reports.
public readonly record struct AdminAccountVm(
    Guid AccountId, string Name, string Status, short StatusId, short TypeId, bool Active, DateTimeOffset LastModified);

public readonly record struct AdminAccountFeatureVm(Guid AccountId, string FeatureKey, bool Enabled, string Tier);

public readonly record struct AdminGroupVm(long GroupId, string Name, bool Active, Guid AccountId);

public readonly record struct AdminUserVm(Guid UserId, string Username);

public readonly record struct AdminTransporterVm(Guid TransporterId, string Name);
