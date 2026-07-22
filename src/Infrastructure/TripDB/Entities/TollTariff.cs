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

namespace TrackHub.TripManagement.Infrastructure.TripDB.Entities;

/// <summary>
/// A temporal price. Tariffs are NEVER overwritten: a change closes the open row by stamping
/// <see cref="EffectiveTo"/> and inserts a new one, so a historical trip estimate stays
/// reproducible (spec 11 section 5, acceptance 21). At most one open row per
/// (TollStationId, TollVehicleClassCode), enforced by a partial unique index.
/// </summary>
public sealed class TollTariff : BaseAuditableEntity
{
    public Guid TollTariffId { get; set; } = Guid.NewGuid();
    public Guid TollStationId { get; set; }
    public string TollVehicleClassCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    /// <summary>ISO 4217.</summary>
    public string Currency { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }

    public TollStation? TollStation { get; set; }
}
