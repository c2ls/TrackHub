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

namespace TrackHub.Security.Domain.Records;

public readonly record struct ServiceClientPermissionDto(
    string ClientId,
    Guid? AccountId,
    string Resource,
    string Action,
    string Scope,
    string Audience,
    bool Active,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? EffectiveTo,
    // Explicit platform-wide grant. A NULL AccountId is NOT a wildcard: set this to true to declare
    // that the grant deliberately spans every tenant (Router/SyncWorker-class identities).
    bool AllowCrossAccount = false);
