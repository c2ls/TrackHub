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
 * Portal build stamp — the only version the UI reports.
 *
 * The backend services are versioned by their Docker image tags, not by an API surface:
 * `rollback.sh list` / `docker compose ps` already answer "what is deployed" for them, and
 * exposing a version per service would mean touching eight repos for information an
 * operator can read from Docker. The portal is different — it is served as static files
 * from a volume, so the only way to see WHICH build a browser is actually running is for
 * that build to say so. Hence: this one stamp, rendered in the footer and on `/status`.
 *
 * `APP_VERSION` is `package.json` "version". `BUILD_TIME` is stamped at `vite build` and
 * is what distinguishes two builds of the same version — the practical case when verifying
 * a deployment or a rollback, since neither bumps `package.json`.
 *
 * Both are statically replaced by vite.config.ts `define`. The `typeof` guard covers a
 * consumer that evaluates this module outside a Vite pipeline, where the identifier would
 * otherwise throw a ReferenceError.
 */
export const APP_VERSION: string = typeof __APP_VERSION__ === 'string' ? __APP_VERSION__ : '0.0.0';

export const BUILD_TIME: string = typeof __BUILD_TIME__ === 'string' ? __BUILD_TIME__ : '';

/** `v1.1.0` — the compact form for the footer. */
export const versionLabel = (): string => `v${APP_VERSION}`;

/**
 * `v1.1.0 · 2026-07-28 14:32 UTC` — the form used where the build has to be identified
 * precisely (the status page). Kept in UTC on purpose: the person reading it is comparing
 * it against a server-side deployment time, not against their own clock.
 */
export const buildLabel = (): string => {
  if (!BUILD_TIME) return versionLabel();
  const stamp = new Date(BUILD_TIME);
  if (Number.isNaN(stamp.getTime())) return versionLabel();
  return `${versionLabel()} · ${stamp.toISOString().slice(0, 16).replace('T', ' ')} UTC`;
};
