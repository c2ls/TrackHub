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
 * Notification-delivery GraphQL documents (Manager backend): delivery history,
 * delivery health, the current user's in-app notification feed, retry and test
 * sends. Codegen validates these against schemas/manager.graphql — values
 * always travel as variables.
 */

import { graphql } from './generated';

export const NotificationDeliveryItemFragment = graphql(`
  fragment NotificationDeliveryItem on NotificationDeliveryVm {
    notificationDeliveryId
    accountId
    notificationRuleId
    alertEventId
    channel
    recipientPrincipalType
    recipient
    status
    attempts
    providerMessageId
    error
    sentAt
    readAt
  }
`);

export const GetNotificationDeliveriesDocument = graphql(`
  query GetNotificationDeliveries(
    $accountId: UUID!
    $status: String
    $channel: String
    $from: DateTime
    $to: DateTime
    $skip: Int!
    $take: Int!
  ) {
    notificationDeliveries(
      query: {
        accountId: $accountId
        status: $status
        channel: $channel
        from: $from
        to: $to
        skip: $skip
        take: $take
      }
    ) {
      ...NotificationDeliveryItem
    }
  }
`);

export const GetDeliveryHealthDocument = graphql(`
  query GetDeliveryHealth($accountId: UUID!, $from: DateTime!, $to: DateTime!) {
    deliveryHealth(query: { accountId: $accountId, from: $from, to: $to }) {
      channel
      status
      count
      averageAttempts
    }
  }
`);

export const MyNotificationItemFragment = graphql(`
  fragment MyNotificationItem on MyNotificationVm {
    notificationDeliveryId
    alertEventId
    eventType
    severity
    sourceModule
    resourceType
    resourceId
    payloadJson
    createdAt
    readAt
  }
`);

export const GetMyNotificationsDocument = graphql(`
  query GetMyNotifications($unreadOnly: Boolean!, $skip: Int!, $take: Int!) {
    myNotifications(query: { unreadOnly: $unreadOnly, skip: $skip, take: $take }) {
      ...MyNotificationItem
    }
  }
`);

export const RetryNotificationDeliveryDocument = graphql(`
  mutation RetryNotificationDelivery($notificationDeliveryId: UUID!) {
    retryNotificationDelivery(command: { notificationDeliveryId: $notificationDeliveryId })
  }
`);

export const MarkNotificationReadDocument = graphql(`
  mutation MarkNotificationRead($notificationDeliveryId: UUID!) {
    markNotificationRead(command: { notificationDeliveryId: $notificationDeliveryId })
  }
`);

export const SendTestNotificationDocument = graphql(`
  mutation SendTestNotification($accountId: UUID!, $channel: String!, $contact: String) {
    sendTestNotification(command: { accountId: $accountId, channel: $channel, contact: $contact }) {
      notificationDeliveryId
      channel
      status
    }
  }
`);
