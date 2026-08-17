/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
*
*  Licensed under the Apache License, Version 2.0 (the "License").
*  You may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License.
*/

/**
 * Notification-delivery API (Manager backend): delivery history, delivery
 * health, the current user's in-app notification feed, retry and test sends.
 * Plain typed async functions that THROW ApiError — the caller decides
 * fallback/toast (admin panels own the try/catch; the bell consumes the feed
 * through TanStack Query hooks in src/queries/notifications.ts).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  NotificationDeliveryItemFragment as NotificationDeliveryItemType,
  MyNotificationItemFragment as MyNotificationItemType,
  GetDeliveryHealthQuery,
} from './generated/graphql';
import {
  GetNotificationDeliveriesDocument,
  GetDeliveryHealthDocument,
  GetMyNotificationsDocument,
  RetryNotificationDeliveryDocument,
  MarkNotificationReadDocument,
  SendTestNotificationDocument,
} from './notificationDeliveriesOperations';

export type NotificationDelivery = NotificationDeliveryItemType;
export type MyNotification = MyNotificationItemType;
export type DeliveryHealth = GetDeliveryHealthQuery['deliveryHealth'][number];

export interface NotificationDeliveryFilters {
  status?: string | null;
  channel?: string | null;
  from?: string | null;
  to?: string | null;
}

export async function getNotificationDeliveries(
  accountId: string,
  filters: NotificationDeliveryFilters = {},
  skip = 0,
  take = 50
): Promise<NotificationDelivery[]> {
  const data = await executeGraphQL('manager', GetNotificationDeliveriesDocument, {
    accountId,
    status: filters.status ?? null,
    channel: filters.channel ?? null,
    from: filters.from ?? null,
    to: filters.to ?? null,
    skip,
    take,
  });
  return data.notificationDeliveries;
}

export async function getDeliveryHealth(
  accountId: string,
  from: string,
  to: string
): Promise<DeliveryHealth[]> {
  const data = await executeGraphQL('manager', GetDeliveryHealthDocument, { accountId, from, to });
  return data.deliveryHealth;
}

export async function getMyNotifications(
  unreadOnly = false,
  skip = 0,
  take = 50
): Promise<MyNotification[]> {
  const data = await executeGraphQL('manager', GetMyNotificationsDocument, {
    unreadOnly,
    skip,
    take,
  });
  return data.myNotifications;
}

export async function retryNotificationDelivery(notificationDeliveryId: string): Promise<boolean> {
  const data = await executeGraphQL('manager', RetryNotificationDeliveryDocument, {
    notificationDeliveryId,
  });
  return data.retryNotificationDelivery;
}

export async function markNotificationRead(notificationDeliveryId: string): Promise<boolean> {
  const data = await executeGraphQL('manager', MarkNotificationReadDocument, {
    notificationDeliveryId,
  });
  return data.markNotificationRead;
}

export async function sendTestNotification(
  accountId: string,
  channel: string,
  contact: string | null = null
): Promise<{ notificationDeliveryId: string; channel: string; status: string }> {
  const data = await executeGraphQL('manager', SendTestNotificationDocument, {
    accountId,
    channel,
    contact,
  });
  return data.sendTestNotification;
}
