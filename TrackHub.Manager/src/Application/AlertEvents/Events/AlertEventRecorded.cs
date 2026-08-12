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

using Microsoft.Extensions.Logging;

namespace TrackHub.Manager.Application.AlertEvents.Events;

/// <summary>
/// Published after a recorded (or coalesced) alert event is persisted. Rule
/// evaluation runs here and is strictly non-blocking: a failure is logged and never fails the
/// originating recordAlertEvent (same rule as the security audit forwarding).
/// </summary>
public sealed class AlertEventRecorded
{
    public readonly record struct Notification(AlertEventVm AlertEvent) : INotification
    {
        public class EventHandler(IAlertRuleEvaluator evaluator, ILogger<EventHandler> logger) : INotificationHandler<Notification>
        {
            public async Task Handle(Notification notification, CancellationToken cancellationToken)
            {
                try
                {
                    var created = await evaluator.EvaluateAsync(notification.AlertEvent, cancellationToken);
                    if (created > 0)
                    {
                        logger.LogInformation("Alert event {AlertEventId} ({EventType}) queued {Count} notification deliveries.",
                            notification.AlertEvent.AlertEventId, notification.AlertEvent.EventType, created);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Alert rule evaluation failed for alert event {AlertEventId}; the originating command is unaffected.",
                        notification.AlertEvent.AlertEventId);
                }
            }
        }
    }
}
