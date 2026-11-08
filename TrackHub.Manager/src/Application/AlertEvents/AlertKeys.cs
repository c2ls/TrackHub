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

namespace TrackHub.Manager.Application.AlertEvents;

/// <summary>
/// Deduplication keys shared by the jobs and the manual commands that raise the same alert. A key
/// names the episode (the baseline that started it), never the calendar, so an episode alerts once
/// and a new baseline is a new alert.
/// </summary>
public static class AlertKeys
{
    public static string CommunicationLoss(Guid transporterId, DateTimeOffset lastPositionAt)
        => $"comm-loss:{transporterId:N}:{lastPositionAt.UtcTicks}";

    public static string GpsCredentialExpiring(Guid operatorId, DateTimeOffset? expiresAt)
        => $"gps-credential-expiring:{operatorId:N}:{expiresAt?.UtcTicks ?? 0}";

    public static string StaffQualification(Guid staffQualificationId, DateOnly expiresAt, int threshold)
        => $"staff-qual:{staffQualificationId:N}:{expiresAt:yyyyMMdd}:{threshold}";

    public static string DocumentExpiration(string eventType, Guid documentId, DateTimeOffset expiresAt, string threshold)
        => $"{eventType}:{documentId:N}:{expiresAt.UtcTicks}:{threshold}";

    public static string DocumentInfected(Guid documentId, int version)
        => $"document-infected:{documentId:N}:{version}";

    public static string DeliveryFailed(Guid notificationDeliveryId)
        => $"delivery-failed:{notificationDeliveryId:N}";

    public static string WebhookDisabled(Guid webhookSubscriptionId)
        => $"webhook-disabled:{webhookSubscriptionId:N}";
}
