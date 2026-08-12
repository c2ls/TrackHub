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
 * Alert-event API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — the caller decides fallback/toast. These reads are consumed
 * imperatively (load-on-expand admin screens), so callers own the try/catch.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type { AlertEventItemFragment as AlertEventItemType } from './generated/graphql';
import {
  GetAlertEventsDocument,
  AcknowledgeAlertEventDocument,
  ResolveAlertEventDocument,
} from './alertEventsOperations';

export type AlertEvent = AlertEventItemType;

export async function getAlertEvents(
  accountId: string,
  skip = 0,
  take = 50
): Promise<AlertEvent[]> {
  const data = await executeGraphQL('manager', GetAlertEventsDocument, { accountId, skip, take });
  return data.alertEvents;
}

export async function acknowledgeAlertEvent(alertEventId: string): Promise<boolean> {
  const data = await executeGraphQL('manager', AcknowledgeAlertEventDocument, { alertEventId });
  return data.acknowledgeAlertEvent;
}

export async function resolveAlertEvent(alertEventId: string): Promise<boolean> {
  const data = await executeGraphQL('manager', ResolveAlertEventDocument, { alertEventId });
  return data.resolveAlertEvent;
}
