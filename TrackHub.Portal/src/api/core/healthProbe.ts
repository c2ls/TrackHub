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
 * Browser-side service health probes for the public platform status page.
 *
 * SANCTIONED DIRECT-FETCH EXCEPTION to the api-layering rule: every backend maps
 * an anonymous `UseHealthChecks("/health")` BEFORE authentication, and CORS runs
 * before it, so the portal origin can probe each service with no token at all.
 * That is precisely the property the status page needs — it must work when the
 * visitor cannot sign in, or when the sign-in service itself is down. Using the
 * GraphQL/REST clients here would defeat the purpose (they acquire a token).
 *
 * URLs are derived ONLY from `endpoints.ts` bases; nothing here reads env vars.
 */

import { HEALTH_ENDPOINTS } from './endpoints';

/** Probe budget. A service that has not answered in this long is treated as Down. */
export const HEALTH_TIMEOUT_MS = 6000;

/** Coarse, non-technical service state. `unknown` means "not probed yet". */
export type ServiceState = 'up' | 'down' | 'unknown';

export interface HealthProbeResult {
  state: ServiceState;
  /**
   * Machine-readable reason, mapped to a localized detail line by the UI. Never
   * shown raw. `unhealthy` means the service answered but reported a failing
   * dependency (typically its database).
   */
  reason?: 'timeout' | 'unreachable' | 'unhealthy' | 'httpError';
  /** HTTP status when one was received — detail line only, never the primary UI. */
  status?: number;
}

/**
 * The services shown as tiles, in display order. `id` is both the i18n key
 * suffix and the React key. Plain-language names live in i18n (ST-08) — this
 * module deliberately carries no display strings.
 */
export const PROBED_SERVICES = [
  'authority',
  'security',
  'manager',
  'router',
  'telemetry',
  'geofencing',
  'reporting',
] as const;

export type ProbedService = (typeof PROBED_SERVICES)[number];

/** Health URL per probed service, or undefined when that backend is not configured. */
export function healthUrlFor(service: ProbedService): string | undefined {
  return HEALTH_ENDPOINTS[service];
}

/**
 * Probes one service. Never throws — a status page must degrade per tile, never
 * blank the page. A 200 whose body does not say `Healthy` is still treated as up
 * (Router/Reporting are liveness-only), while an explicit `Unhealthy` body or a
 * 503 is down-with-detail ("reachable, but its database is not").
 */
export async function probeService(
  service: ProbedService,
  timeoutMs: number = HEALTH_TIMEOUT_MS
): Promise<HealthProbeResult> {
  const url = healthUrlFor(service);
  if (!url) return { state: 'unknown' };

  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), timeoutMs);
  try {
    const response = await fetch(url, {
      method: 'GET',
      signal: controller.signal,
      // No credentials, no Authorization header: the whole point is that this
      // works for a signed-out visitor.
      credentials: 'omit',
      cache: 'no-store',
    });
    // NOT swallowed with `.catch(() => '')`: the abort signal also governs body streaming, so a
    // service that returns 200 headers and then stalls would otherwise be scored "up".
    const body = await response.text();

    // Note the order: `Unhealthy` contains `healthy`, so test for the failure word first.
    const unhealthy = /unhealthy/i.test(body);

    if (response.ok && !unhealthy) {
      // A bare 200 with no body counts as up: Router and Reporting are liveness-only.
      return { state: 'up', status: response.status };
    }
    if (response.status === 503 || unhealthy) {
      return { state: 'down', reason: 'unhealthy', status: response.status };
    }
    return { state: 'down', reason: 'httpError', status: response.status };
  } catch (error) {
    const aborted = error instanceof DOMException && error.name === 'AbortError';
    return { state: 'down', reason: aborted ? 'timeout' : 'unreachable' };
  } finally {
    clearTimeout(timer);
  }
}

/** Probes every configured service concurrently. Always resolves. */
export async function probeAllServices(
  timeoutMs: number = HEALTH_TIMEOUT_MS
): Promise<Record<ProbedService, HealthProbeResult>> {
  const results = await Promise.all(
    PROBED_SERVICES.map(async (service) => [service, await probeService(service, timeoutMs)] as const)
  );
  return Object.fromEntries(results) as Record<ProbedService, HealthProbeResult>;
}

/** Overall banner state: down if anything is down, unknown if nothing could be probed. */
export function overallState(results: Record<ProbedService, HealthProbeResult>): ServiceState {
  const states = Object.values(results).map((result) => result.state);
  if (states.some((state) => state === 'down')) return 'down';
  if (states.some((state) => state === 'up')) return 'up';
  return 'unknown';
}
