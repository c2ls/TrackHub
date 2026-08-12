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

namespace TrackHub.Manager.Domain.Models;

// ApiKey is decrypted; this VM is only served to the SuperAdministrator CRUD surface.
public readonly record struct GeocodingProviderVm(
    Guid GeocodingProviderId,
    string Name,
    short Type,
    string EndpointUri,
    string? ApiKey,
    int RequestsPerSecond,
    int TimeoutSeconds,
    string? ConfigurationJson,
    bool Active);

// ApiKey stays encrypted alongside its per-row Salt; the Router service client decrypts
// with the shared encryption key (same consumption pattern as operator credentials).
public readonly record struct GeocodingProviderTokenVm(
    Guid GeocodingProviderId,
    string Name,
    short Type,
    string EndpointUri,
    string? ApiKey,
    string? Salt,
    int RequestsPerSecond,
    int TimeoutSeconds,
    string? ConfigurationJson);
