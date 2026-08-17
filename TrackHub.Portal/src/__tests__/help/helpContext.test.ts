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

// Screen/topic resolution and index visibility filtering.

import { describe, expect, it } from 'vitest';
import { isTopicVisible, resolveScreenKey, topicsForScreen } from 'context/help/HelpContext';
import type { HelpManifest, HelpTopicMeta } from 'queries/help';

const topic = (overrides: Partial<HelpTopicMeta>): HelpTopicMeta => ({
  id: 'x',
  category: 'operation',
  order: 10,
  screens: [],
  related: [],
  featureKey: null,
  hash: 'h',
  i18n: { en: { title: 'X', description: '', tags: [] } },
  ...overrides,
});

const manifest: HelpManifest = {
  version: 'v',
  languages: ['en', 'es'],
  categories: [
    { id: 'getting-started', order: 1 },
    { id: 'operation', order: 2 },
    { id: 'administration', order: 3 },
  ],
  topics: [
    topic({ id: 'management-overview', category: 'administration', order: 10, screens: ['manageAdmin'] }),
    topic({ id: 'dashboard-trips-replay', category: 'operation', order: 20, screens: ['dashboard'] }),
    topic({ id: 'dashboard-live-map', category: 'operation', order: 10, screens: ['dashboard'] }),
    topic({ id: 'geofences', category: 'operation', order: 30, screens: ['geofenceManager'], featureKey: 'geofencing' }),
    topic({ id: 'getting-started', category: 'getting-started', order: 10, screens: [] }),
  ],
};

describe('resolveScreenKey', () => {
  it('matches routes from routes.tsx exactly', () => {
    expect(resolveScreenKey('/dashboard')).toBe('dashboard');
    expect(resolveScreenKey('/manage-admin/gps-integration')).toBe('gpsIntegration');
  });
  it('resolves sub-paths to their owning route', () => {
    expect(resolveScreenKey('/dashboard/anything/nested')).toBe('dashboard');
  });
  it('returns null for unknown locations', () => {
    expect(resolveScreenKey('/authentication/callback')).toBeNull();
    expect(resolveScreenKey('/nope')).toBeNull();
  });
});

describe('topicsForScreen', () => {
  it('orders by category order then topic order — first is the primary topic', () => {
    expect(topicsForScreen(manifest, 'dashboard').map((entry) => entry.id)).toEqual([
      'dashboard-live-map',
      'dashboard-trips-replay',
    ]);
  });
  it('returns empty for null or uncovered screens', () => {
    expect(topicsForScreen(manifest, null)).toEqual([]);
    expect(topicsForScreen(manifest, 'profile')).toEqual([]);
  });
});

describe('isTopicVisible', () => {
  const enabled = () => true;
  it('hides topics behind a disabled account feature', () => {
    const geofences = manifest.topics.find((entry) => entry.id === 'geofences')!;
    expect(isTopicVisible(geofences, ['geofenceManager'], (key) => key !== 'geofencing')).toBe(false);
    expect(isTopicVisible(geofences, ['geofenceManager'], enabled)).toBe(true);
  });
  it('hides topics whose only screens the principal cannot access', () => {
    const management = manifest.topics.find((entry) => entry.id === 'management-overview')!;
    expect(isTopicVisible(management, ['dashboard'], enabled)).toBe(false);
    expect(isTopicVisible(management, ['dashboard', 'manageAdmin'], enabled)).toBe(true);
  });
  it('always shows index-only topics', () => {
    const gettingStarted = manifest.topics.find((entry) => entry.id === 'getting-started')!;
    expect(isTopicVisible(gettingStarted, [], enabled)).toBe(true);
  });
});
