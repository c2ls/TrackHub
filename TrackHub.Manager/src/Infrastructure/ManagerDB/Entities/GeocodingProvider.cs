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

// Platform-level reverse-geocoding service configuration. No AccountId: geocoding is
// platform infrastructure, SuperAdministrator-owned. ApiKey is encrypted at rest with
// the per-row Salt (same pattern as operator credentials).
public sealed class GeocodingProvider(
    string name,
    short type,
    string endpointUri,
    string? apiKey,
    string? salt,
    int requestsPerSecond,
    int timeoutSeconds,
    string? configurationJson,
    bool active) : BaseAuditableEntity
{
    public Guid GeocodingProviderId { get; private set; } = Guid.NewGuid();
    public string Name { get; set; } = name;
    public short Type { get; set; } = type;
    public string EndpointUri { get; set; } = endpointUri;
    public string? ApiKey { get; set; } = apiKey;
    public string? Salt { get; set; } = salt;
    public int RequestsPerSecond { get; set; } = requestsPerSecond;
    public int TimeoutSeconds { get; set; } = timeoutSeconds;
    public string? ConfigurationJson { get; set; } = configurationJson;
    public bool Active { get; set; } = active;
}
