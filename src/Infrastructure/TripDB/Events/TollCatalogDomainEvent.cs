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

namespace TrackHub.TripManagement.Infrastructure.TripDB.Events;

/// <summary>
/// A platform toll-catalog change (TollStationChanged / TollTariffChanged, spec 11 section 10).
/// Carries no AccountId: the catalog is platform reference data with no tenant owner
/// (spec 11 section 5). Station and tariff changes move money on estimates, which is why every
/// one of them is both audited and announced.
/// </summary>
public sealed class TollCatalogDomainEvent(string eventType, Guid entityId, string? code) : BaseEvent
{
    public string EventType { get; } = eventType;
    public Guid EntityId { get; } = entityId;
    public string? Code { get; } = code;
}
