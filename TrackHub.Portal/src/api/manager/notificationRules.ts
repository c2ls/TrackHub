/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
 * Notification-rule API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — consumed imperatively (load-on-expand), caller owns try/catch.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  NotificationRuleItemFragment as NotificationRuleItemType,
  NotificationRuleDtoInput,
} from './generated/graphql';
import {
  GetNotificationRulesDocument,
  CreateNotificationRuleDocument,
  UpdateNotificationRuleDocument,
  DisableNotificationRuleDocument,
} from './notificationRulesOperations';

export type NotificationRule = NotificationRuleItemType;
export type { NotificationRuleDtoInput };

export async function getNotificationRules(
  accountId: string,
  skip = 0,
  take = 50
): Promise<NotificationRule[]> {
  const data = await executeGraphQL('manager', GetNotificationRulesDocument, {
    accountId,
    skip,
    take,
  });
  return data.notificationRules;
}

export async function createNotificationRule(
  notificationRule: NotificationRuleDtoInput
): Promise<{ notificationRuleId: string }> {
  const data = await executeGraphQL('manager', CreateNotificationRuleDocument, { notificationRule });
  return data.createNotificationRule;
}

export async function updateNotificationRule(
  notificationRuleId: string,
  notificationRule: NotificationRuleDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateNotificationRuleDocument, {
    notificationRuleId,
    notificationRule,
  });
  return data.updateNotificationRule;
}

export async function disableNotificationRule(notificationRuleId: string): Promise<boolean> {
  const data = await executeGraphQL('manager', DisableNotificationRuleDocument, {
    notificationRuleId,
  });
  return data.disableNotificationRule;
}
