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
 * SyncWorker liveness API (Telemetry backend): plain typed async functions.
 * Failures THROW ApiError — fallbacks and toasts belong to the caller layer.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import { GetPlatformSyncActivityDocument } from './platformStatusOperations';

export async function getPlatformSyncActivity(lookbackMinutes = 60) {
  const data = await executeGraphQL('telemetry', GetPlatformSyncActivityDocument, { lookbackMinutes });
  return data.platformSyncActivity;
}

export type PlatformSyncActivity = Awaited<ReturnType<typeof getPlatformSyncActivity>>;

/** Recency window that counts as a live worker: it writes at least once a minute when it has work. */
export const SYNC_WORKER_FRESH_MS = 5 * 60 * 1000;

/**
 * Derives the SyncWorker tile state from data recency (ST-04):
 * `up` when anything was written in the last 5 minutes; `unknown` when no
 * account has GPS integration enabled ("nothing to sync"); `down` otherwise.
 */
export function deriveSyncWorkerState(
  activity: Pick<PlatformSyncActivity, 'lastSyncRunAt' | 'lastHealthCheckAt' | 'hasEnabledGpsIntegration'> | undefined,
  now: number = Date.now()
): 'up' | 'down' | 'unknown' {
  if (!activity) return 'unknown';

  const timestamps = [activity.lastSyncRunAt, activity.lastHealthCheckAt]
    .filter((value): value is string => !!value)
    .map((value) => new Date(value).getTime())
    .filter((value) => !Number.isNaN(value));

  const mostRecent = timestamps.length > 0 ? Math.max(...timestamps) : null;
  if (mostRecent !== null && now - mostRecent <= SYNC_WORKER_FRESH_MS) return 'up';

  // No enabled GPS integration anywhere ⇒ the worker has nothing to write, so
  // silence proves nothing. Never render that as a failure.
  if (!activity.hasEnabledGpsIntegration) return 'unknown';

  return 'down';
}
