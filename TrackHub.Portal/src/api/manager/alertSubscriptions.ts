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
 * Alert-subscription API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — consumed imperatively (load-on-expand), caller owns try/catch.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  AlertSubscriptionItemFragment as AlertSubscriptionItemType,
  AlertSubscriptionDtoInput,
} from './generated/graphql';
import {
  GetAlertSubscriptionsDocument,
  CreateAlertSubscriptionDocument,
  UpdateAlertSubscriptionDocument,
  DeleteAlertSubscriptionDocument,
} from './alertSubscriptionsOperations';

export type AlertSubscription = AlertSubscriptionItemType;
export type { AlertSubscriptionDtoInput };

export async function getAlertSubscriptions(
  accountId: string,
  principalId: string | null = null,
  skip = 0,
  take = 50
): Promise<AlertSubscription[]> {
  const data = await executeGraphQL('manager', GetAlertSubscriptionsDocument, {
    accountId,
    principalId,
    skip,
    take,
  });
  return data.alertSubscriptions;
}

export async function createAlertSubscription(
  subscription: AlertSubscriptionDtoInput
): Promise<{ alertSubscriptionId: string }> {
  const data = await executeGraphQL('manager', CreateAlertSubscriptionDocument, { subscription });
  return data.createAlertSubscription;
}

export async function updateAlertSubscription(
  alertSubscriptionId: string,
  subscription: AlertSubscriptionDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateAlertSubscriptionDocument, {
    alertSubscriptionId,
    subscription,
  });
  return data.updateAlertSubscription;
}

export async function deleteAlertSubscription(alertSubscriptionId: string): Promise<string> {
  const data = await executeGraphQL('manager', DeleteAlertSubscriptionDocument, {
    alertSubscriptionId,
  });
  return data.deleteAlertSubscription;
}
