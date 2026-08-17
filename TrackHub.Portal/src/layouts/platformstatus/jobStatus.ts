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
 * Background-job presentation rules.
 *
 * The critical rule (ST-05): staleness may ONLY be asserted for jobs the backend
 * audit verified record on every cycle — `recordsEveryCycle` on the VM. Every
 * other job legitimately writes a row only when it did work, so an old timestamp
 * is the normal healthy state and must render neutrally as "last activity …".
 */

/** Matches TrackHub.Manager Domain/Constants/BackgroundJobKeys.StalenessThresholdHours. */
export const STALENESS_THRESHOLD_MS = 26 * 60 * 60 * 1000;

export type JobDisplayState = 'ok' | 'failed' | 'stale' | 'idle';

export interface JobStatusLike {
  jobKey: string;
  status: string;
  startedAt: string;
  recordsEveryCycle: boolean;
}

/**
 * - `failed` — the last recorded run failed, regardless of recording semantics.
 * - `stale`  — ONLY for per-cycle jobs whose last run is older than the threshold.
 * - `idle`   — an on-work-only job with nothing recent: neutral, not a problem.
 * - `ok`     — recent successful activity.
 */
export function deriveJobState(job: JobStatusLike, now: number = Date.now()): JobDisplayState {
  if (job.status && job.status.toLowerCase() === 'failed') return 'failed';

  const startedAt = new Date(job.startedAt).getTime();
  const age = Number.isNaN(startedAt) ? Number.POSITIVE_INFINITY : now - startedAt;

  if (age <= STALENESS_THRESHOLD_MS) return 'ok';

  // Past the threshold: only a verified per-cycle job can be called stale.
  return job.recordsEveryCycle ? 'stale' : 'idle';
}

export const JOB_STATE_COLOR: Record<JobDisplayState, 'success' | 'error' | 'warning' | 'secondary'> = {
  ok: 'success',
  failed: 'error',
  stale: 'warning',
  idle: 'secondary',
};

/**
 * Relative time in whole units, for "Last activity: 5 minutes ago". Returns a
 * unit + count so the caller can localize with i18n plurals.
 */
export function relativeAge(timestamp: string, now: number = Date.now()): { unit: 'minute' | 'hour' | 'day'; count: number } | null {
  const parsed = new Date(timestamp).getTime();
  if (Number.isNaN(parsed)) return null;

  const minutes = Math.max(0, Math.floor((now - parsed) / 60_000));
  if (minutes < 60) return { unit: 'minute', count: minutes };
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return { unit: 'hour', count: hours };
  return { unit: 'day', count: Math.floor(hours / 24) };
}
