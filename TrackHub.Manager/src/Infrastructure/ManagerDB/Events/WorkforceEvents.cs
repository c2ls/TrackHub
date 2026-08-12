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

namespace TrackHub.Manager.Infrastructure.Events;

/// <summary>
/// Workforce domain events (spec 09 §10), dispatched by the existing
/// <c>DispatchDomainEventsInterceptor</c> after a successful save.
/// <para>
/// NOTE for future maintainers: spec 09 §10 describes these as "consumed for audit today". That is
/// inaccurate — the audit trail is written directly by the writers via <c>AddAuditEvent</c>, and these
/// events currently have NO subscriber. They exist because the spec commits to them as an extension
/// point for future modules; publishing with no registered handler is a no-op. Do not remove the
/// audit writes on the assumption that a handler covers them.
/// </para>
/// </summary>
public sealed class DriverAssignedToTransporterEvent(Guid accountId, Guid driverTransporterAssignmentId, Guid driverId, Guid transporterId, DateTimeOffset startsAt, string assignmentType) : BaseEvent
{
    public Guid AccountId { get; } = accountId;
    public Guid DriverTransporterAssignmentId { get; } = driverTransporterAssignmentId;
    public Guid DriverId { get; } = driverId;
    public Guid TransporterId { get; } = transporterId;
    public DateTimeOffset StartsAt { get; } = startsAt;
    public string AssignmentType { get; } = assignmentType;
}

/// <inheritdoc cref="DriverAssignedToTransporterEvent"/>
public sealed class DriverAssignmentEndedEvent(Guid accountId, Guid driverTransporterAssignmentId, Guid driverId, Guid transporterId, DateTimeOffset endsAt) : BaseEvent
{
    public Guid AccountId { get; } = accountId;
    public Guid DriverTransporterAssignmentId { get; } = driverTransporterAssignmentId;
    public Guid DriverId { get; } = driverId;
    public Guid TransporterId { get; } = transporterId;
    public DateTimeOffset EndsAt { get; } = endsAt;
}

/// <inheritdoc cref="DriverAssignedToTransporterEvent"/>
public sealed class DriverQualificationCreatedEvent(Guid accountId, Guid driverQualificationId, Guid driverId, string qualificationType, DateOnly? expiresAt) : BaseEvent
{
    public Guid AccountId { get; } = accountId;
    public Guid DriverQualificationId { get; } = driverQualificationId;
    public Guid DriverId { get; } = driverId;
    public string QualificationType { get; } = qualificationType;
    public DateOnly? ExpiresAt { get; } = expiresAt;
}

/// <inheritdoc cref="DriverAssignedToTransporterEvent"/>
public sealed class DriverQualificationUpdatedEvent(Guid accountId, Guid driverQualificationId, Guid driverId, string qualificationType, DateOnly? expiresAt) : BaseEvent
{
    public Guid AccountId { get; } = accountId;
    public Guid DriverQualificationId { get; } = driverQualificationId;
    public Guid DriverId { get; } = driverId;
    public string QualificationType { get; } = qualificationType;
    public DateOnly? ExpiresAt { get; } = expiresAt;
}

/// <inheritdoc cref="DriverAssignedToTransporterEvent"/>
public sealed class DriverQualificationDeletedEvent(Guid accountId, Guid driverQualificationId, Guid driverId, string qualificationType) : BaseEvent
{
    public Guid AccountId { get; } = accountId;
    public Guid DriverQualificationId { get; } = driverQualificationId;
    public Guid DriverId { get; } = driverId;
    public string QualificationType { get; } = qualificationType;
}
