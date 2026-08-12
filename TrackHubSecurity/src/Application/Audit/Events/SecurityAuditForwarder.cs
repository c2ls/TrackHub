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

using Common.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TrackHub.Security.Domain.Interfaces;
using TrackHub.Security.Domain.Records;

namespace TrackHub.Security.Application.Audit.Events;

// Builds the audit notification from the current principal so handlers stay one-liners.
public static class SecurityAudit
{
    public static SecurityAuditForwarder.Notification Event(
        ICurrentPrincipal principal,
        string action,
        string resourceType,
        string resourceId,
        Guid? accountId,
        string? oldValues = null,
        string? newValues = null)
    {
        var actorId = principal.UserId?.ToString()
            ?? principal.ClientId
            ?? principal.SubjectId
            ?? "unknown";

        return new SecurityAuditForwarder.Notification(new SecurityAuditEventDto(
            accountId,
            principal.PrincipalType.ToString(),
            actorId,
            action,
            resourceType,
            resourceId,
            oldValues,
            newValues,
            principal.CorrelationId));
    }
}

// Post-commit, best-effort forwarding of a security audit event to Manager. A transport failure
// (Manager down) is logged with the payload and never fails the originating command.
public sealed class SecurityAuditForwarder
{
    public readonly record struct Notification(SecurityAuditEventDto AuditEvent) : INotification
    {
        // Resolves the outbound audit writer lazily inside the try/catch: constructing it acquires the
        // security_client service token synchronously, so a missing/misconfigured identity must be
        // swallowed here rather than bubble up and fail the originating security command.
        public class EventHandler(IServiceProvider serviceProvider, ILogger<EventHandler> logger) : INotificationHandler<Notification>
        {
            public async Task Handle(Notification notification, CancellationToken cancellationToken)
            {
                try
                {
                    var auditWriter = serviceProvider.GetRequiredService<IManagerAuditWriter>();
                    await auditWriter.ForwardAuditEventAsync(notification.AuditEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Failed to forward security audit event {Action} on {ResourceType} {ResourceId}; the originating command still succeeded. Payload account={AccountId} actor={ActorId}.",
                        notification.AuditEvent.Action,
                        notification.AuditEvent.ResourceType,
                        notification.AuditEvent.ResourceId,
                        notification.AuditEvent.AccountId,
                        notification.AuditEvent.ActorId);
                }
            }
        }
    }
}
