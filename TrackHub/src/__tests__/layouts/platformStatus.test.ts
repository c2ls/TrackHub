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

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { probeService, overallState, PROBED_SERVICES } from 'api/core/healthProbe';
import type { HealthProbeResult, ProbedService } from 'api/core/healthProbe';
import { deriveSyncWorkerState, SYNC_WORKER_FRESH_MS } from 'api/telemetry/platformStatus';
import { deriveJobState, relativeAge, STALENESS_THRESHOLD_MS } from 'layouts/platformstatus/jobStatus';
import { announcementText, severityToMuiSeverity } from 'layouts/platformstatus/announcementText';

describe('healthProbe — service state derivation', () => {
  const originalFetch = global.fetch;

  afterEach(() => {
    global.fetch = originalFetch;
    vi.unstubAllEnvs();
    vi.restoreAllMocks();
  });

  /**
   * `HEALTH_ENDPOINTS` uses lazy getters over the runtime env, so toggling the
   * env var between tests is enough — no module reset needed.
   */
  const probeWith = async (
    impl: () => Promise<Response>,
    { configured = true }: { configured?: boolean } = {}
  ) => {
    if (configured) {
      vi.stubEnv('REACT_APP_MANAGER_ENDPOINT', 'https://example.test/Manager/graphql/');
    } else {
      vi.stubEnv('REACT_APP_MANAGER_ENDPOINT', '');
    }
    global.fetch = vi.fn(impl) as unknown as typeof fetch;
    return probeService('manager');
  };

  it('treats 200 + "Healthy" as up', async () => {
    expect(await probeWith(async () => new Response('Healthy', { status: 200 }))).toEqual({
      state: 'up',
      status: 200,
    });
  });

  it('treats a liveness-only 200 with an empty body as up', async () => {
    // Router and Reporting register bare health checks; absence of the word
    // "Healthy" must not be read as a failure.
    expect((await probeWith(async () => new Response('', { status: 200 }))).state).toBe('up');
  });

  it('treats 503 as down with an "unhealthy" detail', async () => {
    expect(await probeWith(async () => new Response('Unhealthy', { status: 503 }))).toEqual({
      state: 'down',
      reason: 'unhealthy',
      status: 503,
    });
  });

  it('treats a 200 whose body says Unhealthy as down', async () => {
    expect((await probeWith(async () => new Response('Unhealthy', { status: 200 }))).reason).toBe('unhealthy');
  });

  it('treats a non-2xx, non-503 response as down with an httpError detail', async () => {
    expect(await probeWith(async () => new Response('nope', { status: 500 }))).toEqual({
      state: 'down',
      reason: 'httpError',
      status: 500,
    });
  });

  it('treats an aborted request (timeout) as down', async () => {
    expect(
      await probeWith(async () => {
        throw new DOMException('aborted', 'AbortError');
      })
    ).toEqual({ state: 'down', reason: 'timeout' });
  });

  it('treats a network failure as down and never throws', async () => {
    await expect(
      probeWith(async () => {
        throw new TypeError('Failed to fetch');
      })
    ).resolves.toEqual({ state: 'down', reason: 'unreachable' });
  });

  it('reports unknown when the backend is not configured', async () => {
    expect(
      await probeWith(async () => new Response('Healthy', { status: 200 }), { configured: false })
    ).toEqual({ state: 'unknown' });
  });
});

describe('healthProbe — overall banner state', () => {
  const build = (states: Record<string, HealthProbeResult['state']>) =>
    Object.fromEntries(
      PROBED_SERVICES.map((service) => [service, { state: states[service] ?? 'unknown' }])
    ) as Record<ProbedService, HealthProbeResult>;

  it('is up when everything probed is up', () => {
    expect(overallState(build(Object.fromEntries(PROBED_SERVICES.map((s) => [s, 'up' as const]))))).toBe('up');
  });

  it('is down when any single service is down', () => {
    expect(overallState(build({ ...Object.fromEntries(PROBED_SERVICES.map((s) => [s, 'up' as const])), manager: 'down' }))).toBe('down');
  });

  it('is unknown when nothing could be probed at all', () => {
    expect(overallState(build({}))).toBe('unknown');
  });
});

describe('SyncWorker recency thresholds (ST-04)', () => {
  const now = Date.now();
  const iso = (offsetMs: number) => new Date(now - offsetMs).toISOString();

  it('is up when a sync run landed inside the freshness window', () => {
    expect(
      deriveSyncWorkerState(
        { lastSyncRunAt: iso(60_000), lastHealthCheckAt: null, hasEnabledGpsIntegration: true },
        now
      )
    ).toBe('up');
  });

  it('is up when only a health check is recent', () => {
    expect(
      deriveSyncWorkerState(
        { lastSyncRunAt: iso(60 * 60_000), lastHealthCheckAt: iso(30_000), hasEnabledGpsIntegration: true },
        now
      )
    ).toBe('up');
  });

  it('is down when activity is older than the window and there IS work to do', () => {
    expect(
      deriveSyncWorkerState(
        { lastSyncRunAt: iso(SYNC_WORKER_FRESH_MS + 60_000), lastHealthCheckAt: null, hasEnabledGpsIntegration: true },
        now
      )
    ).toBe('down');
  });

  it('is unknown — never down — when no account has GPS integration enabled', () => {
    // "Nothing to sync" must never be presented as a failure.
    expect(
      deriveSyncWorkerState(
        { lastSyncRunAt: null, lastHealthCheckAt: null, hasEnabledGpsIntegration: false },
        now
      )
    ).toBe('unknown');
  });

  it('is unknown when the query has not resolved', () => {
    expect(deriveSyncWorkerState(undefined, now)).toBe('unknown');
  });
});

describe('background job staleness vs last activity (ST-05)', () => {
  const now = Date.now();
  const iso = (offsetMs: number) => new Date(now - offsetMs).toISOString();

  it('flags a failed last run regardless of recording semantics', () => {
    expect(
      deriveJobState({ jobKey: 'document-scan', status: 'Failed', startedAt: iso(1000), recordsEveryCycle: false }, now)
    ).toBe('failed');
  });

  it('reports ok for recent successful activity', () => {
    expect(
      deriveJobState({ jobKey: 'alert-evaluation', status: 'Succeeded', startedAt: iso(1000), recordsEveryCycle: true }, now)
    ).toBe('ok');
  });

  it('calls a per-cycle job stale once it passes the threshold', () => {
    expect(
      deriveJobState(
        { jobKey: 'alert-evaluation', status: 'Succeeded', startedAt: iso(STALENESS_THRESHOLD_MS + 60_000), recordsEveryCycle: true },
        now
      )
    ).toBe('stale');
  });

  it('NEVER calls an on-work-only job stale — it renders as idle', () => {
    // The digest job records nothing when it had nothing to fold; an old
    // timestamp is the healthy steady state, not a fault.
    expect(
      deriveJobState(
        { jobKey: 'notification-digest', status: 'Succeeded', startedAt: iso(STALENESS_THRESHOLD_MS * 10), recordsEveryCycle: false },
        now
      )
    ).toBe('idle');
  });

  it('does not treat an unparseable timestamp on an on-work job as stale', () => {
    expect(
      deriveJobState({ jobKey: 'document-scan', status: 'Succeeded', startedAt: 'not-a-date', recordsEveryCycle: false }, now)
    ).toBe('idle');
  });
});

describe('relativeAge', () => {
  const now = Date.now();

  it('reports whole minutes under an hour', () => {
    expect(relativeAge(new Date(now - 5 * 60_000).toISOString(), now)).toEqual({ unit: 'minute', count: 5 });
  });

  it('rolls up to hours and then days', () => {
    expect(relativeAge(new Date(now - 3 * 3_600_000).toISOString(), now)).toEqual({ unit: 'hour', count: 3 });
    expect(relativeAge(new Date(now - 2 * 86_400_000).toISOString(), now)).toEqual({ unit: 'day', count: 2 });
  });

  it('returns null for an unparseable timestamp', () => {
    expect(relativeAge('nonsense', now)).toBeNull();
  });
});

describe('announcement language fallback', () => {
  const announcement = { messageEn: 'Maintenance window', messageEs: 'Ventana de mantenimiento' };

  it('shows Spanish to an es user when the author supplied it', () => {
    expect(announcementText(announcement, 'es')).toBe('Ventana de mantenimiento');
    expect(announcementText(announcement, 'es-CO')).toBe('Ventana de mantenimiento');
  });

  it('shows English to an en user', () => {
    expect(announcementText(announcement, 'en')).toBe('Maintenance window');
  });

  it('falls back to English when the Spanish message is absent or blank', () => {
    expect(announcementText({ messageEn: 'Only English', messageEs: null }, 'es')).toBe('Only English');
    expect(announcementText({ messageEn: 'Only English', messageEs: '   ' }, 'es')).toBe('Only English');
  });

  it('maps severity to the right banner colour', () => {
    expect(severityToMuiSeverity('INFO')).toBe('info');
    expect(severityToMuiSeverity('WARNING')).toBe('warning');
    expect(severityToMuiSeverity('CRITICAL')).toBe('error');
  });
});
