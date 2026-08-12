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

namespace TrackHub.TripManagement.Infrastructure.TripDB.Entities;

// Read-only projection of the Manager-owned app.account_features table. This is what backs this
// service's own IFeatureFlagService override - without it Common's fail-open default would let
// every [RequireFeature] pass silently (spec 11 section 15, acceptance 10).
public sealed class AccountFeature
{
    public Guid AccountFeatureId { get; set; }
    public Guid AccountId { get; set; }
    public string FeatureKey { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public string? ConfigurationJson { get; set; }
}
