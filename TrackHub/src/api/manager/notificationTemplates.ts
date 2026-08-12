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
 * Notification-template API (Manager backend): plain typed async functions.
 * Failures THROW ApiError — consumed imperatively (load-on-expand), caller owns
 * the try/catch. The read returns platform defaults (accountId == null,
 * read-only) merged with the account's overrides.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  NotificationTemplateItemFragment as NotificationTemplateItemType,
  NotificationTemplateDtoInput,
} from './generated/graphql';
import {
  GetNotificationTemplatesDocument,
  CreateNotificationTemplateDocument,
  UpdateNotificationTemplateDocument,
  DeleteNotificationTemplateDocument,
} from './notificationTemplatesOperations';

export type NotificationTemplate = NotificationTemplateItemType;
export type { NotificationTemplateDtoInput };

export async function getNotificationTemplates(
  accountId: string
): Promise<NotificationTemplate[]> {
  const data = await executeGraphQL('manager', GetNotificationTemplatesDocument, { accountId });
  return data.notificationTemplates;
}

export async function createNotificationTemplate(
  template: NotificationTemplateDtoInput
): Promise<{ notificationTemplateId: string }> {
  const data = await executeGraphQL('manager', CreateNotificationTemplateDocument, { template });
  return data.createNotificationTemplate;
}

export async function updateNotificationTemplate(
  notificationTemplateId: string,
  template: NotificationTemplateDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateNotificationTemplateDocument, {
    notificationTemplateId,
    template,
  });
  return data.updateNotificationTemplate;
}

export async function deleteNotificationTemplate(
  notificationTemplateId: string
): Promise<string> {
  const data = await executeGraphQL('manager', DeleteNotificationTemplateDocument, {
    notificationTemplateId,
  });
  return data.deleteNotificationTemplate;
}
