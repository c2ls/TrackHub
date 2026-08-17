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
 * Operator health / sync-run API (Telemetry backend): plain typed async
 * functions. Failures THROW ApiError — fallbacks and toasts belong to the
 * caller layer (src/queries).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  GetOperatorSyncRunsQuery,
  GetOperatorHealthQuery,
  GetOperatorHealthHistoryQuery,
} from './generated/graphql';
import {
  GetOperatorSyncRunsDocument,
  GetOperatorHealthDocument,
  GetOperatorHealthHistoryDocument,
} from './operatorHealthOperations';

export type OperatorSyncRun = GetOperatorSyncRunsQuery['operatorSyncRuns'][number];
export type OperatorHealth = GetOperatorHealthQuery['operatorHealth'];
export type OperatorHealthCheck =
  GetOperatorHealthHistoryQuery['operatorHealthHistory'][number];

export async function getOperatorSyncRuns(
  accountId: string | null = null,
  operatorId: string | null = null,
  take = 20
): Promise<OperatorSyncRun[]> {
  const data = await executeGraphQL('telemetry', GetOperatorSyncRunsDocument, {
    accountId,
    operatorId,
    take,
  });
  return data.operatorSyncRuns;
}

export async function getOperatorHealth(operatorId: string): Promise<OperatorHealth> {
  const data = await executeGraphQL('telemetry', GetOperatorHealthDocument, { operatorId });
  return data.operatorHealth;
}

export async function getOperatorHealthHistory(
  operatorId: string,
  take = 50
): Promise<OperatorHealthCheck[]> {
  const data = await executeGraphQL('telemetry', GetOperatorHealthHistoryDocument, {
    operatorId,
    take,
  });
  return data.operatorHealthHistory;
}
