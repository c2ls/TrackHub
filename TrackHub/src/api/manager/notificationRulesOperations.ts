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
 * Notification-rule GraphQL documents (Manager backend). Codegen validates
 * these against schemas/manager.graphql — values always travel as variables.
 */

import { graphql } from './generated';

export const NotificationRuleItemFragment = graphql(`
  fragment NotificationRuleItem on NotificationRuleVm {
    notificationRuleId
    accountId
    ruleKey
    ruleType
    enabled
    triggerEvent
    recipientSelector
    channelsJson
    throttlingJson
    configurationJson
    lastModified
  }
`);

export const GetNotificationRulesDocument = graphql(`
  query GetNotificationRules($accountId: UUID!, $skip: Int!, $take: Int!) {
    notificationRules(query: { accountId: $accountId, skip: $skip, take: $take }) {
      ...NotificationRuleItem
    }
  }
`);

export const CreateNotificationRuleDocument = graphql(`
  mutation CreateNotificationRule($notificationRule: NotificationRuleDtoInput!) {
    createNotificationRule(command: { notificationRule: $notificationRule }) {
      notificationRuleId
      accountId
      ruleKey
      enabled
      lastModified
    }
  }
`);

export const UpdateNotificationRuleDocument = graphql(`
  mutation UpdateNotificationRule(
    $notificationRuleId: UUID!
    $notificationRule: NotificationRuleDtoInput!
  ) {
    updateNotificationRule(
      command: { notificationRuleId: $notificationRuleId, notificationRule: $notificationRule }
    )
  }
`);

export const DisableNotificationRuleDocument = graphql(`
  mutation DisableNotificationRule($notificationRuleId: UUID!) {
    disableNotificationRule(command: { notificationRuleId: $notificationRuleId })
  }
`);
