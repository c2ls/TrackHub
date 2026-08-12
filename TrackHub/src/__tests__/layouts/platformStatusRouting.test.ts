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

// Route-table guarantees for the public /status route.
//
// These pin the two defects found in the spec-28 audit:
//   * omitting `principalTypes` DEFAULTS to [User] rather than meaning "public", so a Driver or
//     public-link principal was redirected to /dashboard — itself User-only — looping forever;
//   * the Sidenav applied the same default, hiding the link from the roles it was meant to serve.
// The `public` flag is what makes /status genuinely reachable by everyone.

import { describe, it, expect } from 'vitest';
import routes from 'routes';
import type { RouteDefinition } from 'routes';
import PrincipalTypes from 'constants/principalTypes';
import type { PrincipalType } from 'constants/principalTypes';

const flatten = (list: RouteDefinition[]): RouteDefinition[] =>
  list.flatMap((route) => (route.collapse ? [route, ...flatten(route.collapse)] : [route]));

const allRoutes = flatten(routes);
const statusRoute = allRoutes.find((route) => route.key === 'platformStatus');

/** Mirrors App.tsx `routeAllowed` and Sidenav's filter for the principal-type gate. */
const principalAllowed = (route: RouteDefinition, principalType: PrincipalType | null): boolean => {
  if (route.public) return true;
  if (!principalType) return true;
  return (route.principalTypes || [PrincipalTypes.User]).includes(principalType);
};

describe('platformStatus route definition', () => {
  it('exists, is a normal (sidenav-visible) route at /status', () => {
    expect(statusRoute).toBeDefined();
    expect(statusRoute?.type).toBe('route');
    expect(statusRoute?.route).toBe('/status');
  });

  it('is marked public so it bypasses the role and principal-type gates', () => {
    // Without this flag the route silently inherits the [User] default.
    expect(statusRoute?.public).toBe(true);
  });

  it('is reachable by EVERY principal type, and by a signed-out visitor', () => {
    const principals: (PrincipalType | null)[] = [
      null,
      PrincipalTypes.User,
      PrincipalTypes.Driver,
      PrincipalTypes.ServiceClient,
      PrincipalTypes.PublicLink,
      PrincipalTypes.Unknown,
    ];

    for (const principal of principals) {
      expect(principalAllowed(statusRoute as RouteDefinition, principal)).toBe(true);
    }
  });

  it('never redirects to a route that would redirect back (no navigation loop)', () => {
    // The loop this guards: /status denied → Navigate to /dashboard → /dashboard is User-only →
    // denied → Navigate to /dashboard → …
    const dashboard = allRoutes.find((route) => route.key === 'dashboard');
    expect(dashboard?.principalTypes).toEqual([PrincipalTypes.User]);
    expect(principalAllowed(statusRoute as RouteDefinition, PrincipalTypes.Driver)).toBe(true);
  });

  it('carries no featureKey, so no account feature flag can hide it', () => {
    expect(statusRoute?.featureKey).toBeUndefined();
  });

  it('is the only public route (the flag is not applied accidentally elsewhere)', () => {
    expect(allRoutes.filter((route) => route.public).map((route) => route.key)).toEqual(['platformStatus']);
  });
});
