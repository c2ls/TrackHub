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
 * Static catalogs for the alerts & notifications module. Labels come
 * from i18n (`alertEventTypes.*`, `notificationChannels.*`) via toCamelCase;
 * these are the raw backend identifiers. UI gating is cosmetic — the backend
 * is authoritative for feature entitlements.
 */

/** Alert event types emitted across the platform. */
export const ALERT_EVENT_TYPES = [
  'GeofenceEntered',
  'GeofenceExited',
  'GeofenceDwellExceeded',
  'CommunicationLoss',
  'GpsCredentialExpiring',
  'GpsOperatorPositionSyncFailed',
  'DocumentExpiring',
  'DocumentExpired',
  'NotificationDeliveryFailed',
] as const;

/** Rule delivery channels. Push is not yet available and is intentionally hidden. */
export const NOTIFICATION_CHANNELS = ['InApp', 'Email', 'Webhook', 'WhatsApp'] as const;

/** Channels a principal can subscribe to directly (no Webhook at principal level). */
export const SUBSCRIPTION_CHANNELS = ['InApp', 'Email', 'WhatsApp'] as const;

/** Delivery lifecycle statuses (filterable in the delivery-history panel). */
export const DELIVERY_STATUSES = [
  'Pending',
  'Sending',
  'Sent',
  'Failed',
  'Deferred',
  'Digested',
] as const;

/** Template keys: one per alert event type plus the system template keys. */
export const TEMPLATE_KEYS = [
  ...ALERT_EVENT_TYPES,
  'TestNotification',
  'NotificationDigest',
] as const;

/** Digest modes accepted in a rule's throttlingJson. */
export const DIGEST_MODES = ['None', 'Hourly', 'Daily'] as const;

/** Feature keys gating the notifications module and its billable channels. */
export const NOTIFICATIONS_FEATURE_KEY = 'notifications';
export const NOTIFICATIONS_EMAIL_FEATURE_KEY = 'notifications.email';
export const NOTIFICATIONS_WHATSAPP_FEATURE_KEY = 'notifications.whatsapp';
