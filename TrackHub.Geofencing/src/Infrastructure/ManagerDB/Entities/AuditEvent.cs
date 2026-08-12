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

namespace TrackHub.Geofencing.Infrastructure.Entities;

public sealed class AuditEvent(
    Guid accountId,
    string actorType,
    string actorId,
    string action,
    string resourceType,
    string resourceId,
    string result,
    string? oldValuesJson,
    string? newValuesJson,
    string? reason,
    string? ipAddress,
    string? userAgent,
    string? correlationId)
{
    public Guid AuditEventId { get; set; } = Guid.NewGuid();
    public Guid AccountId { get; set; } = accountId;
    public string ActorType { get; set; } = actorType;
    public string ActorId { get; set; } = actorId;
    public string Action { get; set; } = action;
    public string ResourceType { get; set; } = resourceType;
    public string ResourceId { get; set; } = resourceId;
    public string Result { get; set; } = result;
    public string? OldValuesJson { get; set; } = oldValuesJson;
    public string? NewValuesJson { get; set; } = newValuesJson;
    public string? Reason { get; set; } = reason;
    public string? IpAddress { get; set; } = ipAddress;
    public string? UserAgent { get; set; } = userAgent;
    public string? CorrelationId { get; set; } = correlationId;
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}
