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
 * Alert-subscription GraphQL documents (Manager backend). Codegen validates
 * these against schemas/manager.graphql — values always travel as variables.
 */

import { graphql } from './generated';

export const AlertSubscriptionItemFragment = graphql(`
  fragment AlertSubscriptionItem on AlertSubscriptionVm {
    alertSubscriptionId
    accountId
    principalType
    principalId
    eventTypeFilter
    channel
    contact
    enabled
    lastModified
  }
`);

export const GetAlertSubscriptionsDocument = graphql(`
  query GetAlertSubscriptions($accountId: UUID!, $principalId: UUID, $skip: Int!, $take: Int!) {
    alertSubscriptions(
      query: { accountId: $accountId, principalId: $principalId, skip: $skip, take: $take }
    ) {
      ...AlertSubscriptionItem
    }
  }
`);

export const CreateAlertSubscriptionDocument = graphql(`
  mutation CreateAlertSubscription($subscription: AlertSubscriptionDtoInput!) {
    createAlertSubscription(command: { subscription: $subscription }) {
      alertSubscriptionId
      accountId
      principalType
      principalId
      channel
      enabled
    }
  }
`);

export const UpdateAlertSubscriptionDocument = graphql(`
  mutation UpdateAlertSubscription(
    $alertSubscriptionId: UUID!
    $subscription: AlertSubscriptionDtoInput!
  ) {
    updateAlertSubscription(
      command: { alertSubscriptionId: $alertSubscriptionId, subscription: $subscription }
    )
  }
`);

export const DeleteAlertSubscriptionDocument = graphql(`
  mutation DeleteAlertSubscription($alertSubscriptionId: UUID!) {
    deleteAlertSubscription(command: { alertSubscriptionId: $alertSubscriptionId })
  }
`);
