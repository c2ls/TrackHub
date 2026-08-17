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
 * Single source of truth for backend endpoints. All env access for API URLs
 * lives here; nothing else in src/api reads process.env.
 */

export const GRAPHQL_ENDPOINTS = {
  manager: process.env.REACT_APP_MANAGER_ENDPOINT,
  security: process.env.REACT_APP_SECURITY_ENDPOINT,
  geofencing: process.env.REACT_APP_GEOFENCING_ENDPOINT,
  router: process.env.REACT_APP_ROUTER_ENDPOINT,
  telemetry: process.env.REACT_APP_TELEMETRY_ENDPOINT,
} as const;

export type GraphQLBackend = keyof typeof GRAPHQL_ENDPOINTS;

/**
 * Manager REST base (document upload/download live outside GraphQL). The
 * fallback only matters under Vitest, where env vars are absent at load time.
 */
const managerRestBase = (GRAPHQL_ENDPOINTS.manager ?? '').replace(/graphql\/?$/, '');

export const REST_ENDPOINTS = {
  /** Report generation — xlsx/pdf file bytes (Reporting service, REST). */
  reportingBasicReports: `${process.env.REACT_APP_REPORTING_ENDPOINT}api/BasicReports`,
  /** On-screen report preview — JSON dataset (Reporting service, REST). */
  reportingBasicReportsPreview: `${process.env.REACT_APP_REPORTING_ENDPOINT}api/BasicReports/preview`,
  /**
   * Manager document REST base, e.g. `${managerDocuments}/upload`. The Manager
   * document endpoints are mapped at `~/documents/...` (no `api/` segment) —
   * see TrackHub.Manager Web/Endpoints/Documents.cs.
   */
  managerDocuments: `${managerRestBase}documents`,
  /**
   * Anonymous platform announcements (Manager service, REST). Deliberately
   * unauthenticated — the status page must show a maintenance banner to a
   * visitor who cannot sign in. See TrackHub.Manager Web/Endpoints/PlatformStatus.cs.
   */
  managerPlatformAnnouncements: `${managerRestBase}api/PlatformStatus/announcements`,
} as const;

/**
 * Anonymous `/health` probe URLs, one per backend, for the public status page.
 *
 * Lazy getters (like OAUTH_ENDPOINTS): production builds inline the process.env
 * expressions, while tests can assign REACT_APP_* at runtime. A getter returns
 * `undefined` when its backend is not configured, which the status page renders
 * as "unknown" rather than "down".
 */
const toHealthUrl = (base: string | undefined): string | undefined =>
  base ? `${base.replace(/graphql\/?$/, '').replace(/\/+$/, '')}/health` : undefined;

export const HEALTH_ENDPOINTS = {
  get authority() {
    // The AuthorityServer base carries its /Identity path base; the token
    // endpoint is the only configured URL that reliably points at it.
    const token = process.env.REACT_APP_TOKEN_ENDPOINT;
    return token ? `${token.replace(/\/+$/, '').replace(/\/[^/]*$/, '')}/health` : undefined;
  },
  get security() {
    return toHealthUrl(process.env.REACT_APP_SECURITY_ENDPOINT);
  },
  get manager() {
    return toHealthUrl(process.env.REACT_APP_MANAGER_ENDPOINT);
  },
  get router() {
    return toHealthUrl(process.env.REACT_APP_ROUTER_ENDPOINT);
  },
  get telemetry() {
    return toHealthUrl(process.env.REACT_APP_TELEMETRY_ENDPOINT);
  },
  get geofencing() {
    return toHealthUrl(process.env.REACT_APP_GEOFENCING_ENDPOINT);
  },
  get reporting() {
    return toHealthUrl(process.env.REACT_APP_REPORTING_ENDPOINT);
  },
} as const;

/** Default map center (Bogotá) used before real positions load. */
export const MAP_DEFAULTS = {
  lat: parseFloat(process.env.REACT_APP_DEFAULT_LAT),
  lng: parseFloat(process.env.REACT_APP_DEFAULT_LNG),
} as const;

// Lazy getters: in production builds the process.env expressions are replaced
// statically (vite define shim); in tests they read the live process.env so
// suites can assign REACT_APP_* values at runtime (apiService/auth tests do).
export const OAUTH_ENDPOINTS = {
  get authorization() {
    return process.env.REACT_APP_AUTHORIZATION_ENDPOINT;
  },
  get token() {
    return process.env.REACT_APP_TOKEN_ENDPOINT;
  },
  get revocation() {
    return process.env.REACT_APP_REVOKE_TOKEN_ENDPOINT;
  },
  get logout() {
    return process.env.REACT_APP_LOGOUT_ENDPOINT;
  },
  get callback() {
    return process.env.REACT_APP_CALLBACK_ENDPOINT;
  },
  get clientId() {
    return process.env.REACT_APP_CLIENT_ID;
  },
} as const;
