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

namespace TrackHub.Manager.Application.Notifications.Events;

/// <summary>
/// Published by the dispatcher when a delivery exhausts its retries. The
/// delivery-failure AlertEvent is recorded by the dispatcher itself (without rule evaluation —
/// a failing channel must never notify itself into a loop); this event is the in-process signal.
/// </summary>
public sealed class NotificationDeliveryFailed
{
    public readonly record struct Notification(Guid NotificationDeliveryId, Guid AccountId, string Channel, int Attempts, string? Error) : INotification
    {
        public class EventHandler(ILogger<EventHandler> logger) : INotificationHandler<Notification>
        {
            public Task Handle(Notification notification, CancellationToken cancellationToken)
            {
                logger.LogWarning("Delivery {DeliveryId} permanently failed via {Channel} after {Attempts} attempt(s): {Error}",
                    notification.NotificationDeliveryId, notification.Channel, notification.Attempts, notification.Error);
                return Task.CompletedTask;
            }
        }
    }
}
