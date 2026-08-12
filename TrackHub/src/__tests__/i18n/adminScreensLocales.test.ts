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

import en from 'locales/en.json';
import es from 'locales/es.json';
import { SECTION_GROUP_KEYS } from 'layouts/manageadmin/data/sectionGroups';

const resolveKey = (bundle: unknown, key: string): unknown =>
  key.split('.').reduce<unknown>(
    (acc, part) =>
      acc && typeof acc === 'object' ? (acc as Record<string, unknown>)[part] : undefined,
    bundle
  );

const flatten = (obj: Record<string, unknown>, prefix = ''): string[] =>
  Object.entries(obj).flatMap(([key, value]) =>
    value && typeof value === 'object'
      ? flatten(value as Record<string, unknown>, `${prefix}${key}.`)
      : [`${prefix}${key}`]
  );

// Keys rendered by the reworked GPS-integration dashboard (summary cards +
// provider/status pivot table).
const GPS_DASHBOARD_KEYS = [
  'gpsIntegration.dashboard.operators',
  'gpsIntegration.dashboard.devices',
  'gpsIntegration.dashboard.sync',
  'gpsIntegration.dashboard.enabledOfTotal',
  'gpsIntegration.dashboard.okFailed24h',
  'gpsIntegration.dashboard.operatorsHealthy',
  'gpsIntegration.dashboard.operatorsDegraded',
  'gpsIntegration.dashboard.operatorsOffline',
  'gpsIntegration.dashboard.recentlyAdded24h',
  'gpsIntegration.dashboard.averageSyncDuration',
  'gpsIntegration.dashboard.lastAutoSync',
  'gpsIntegration.dashboard.lastManualSync',
  'gpsIntegration.dashboard.deviceCountsByProviderStatus',
  'gpsIntegration.status.assigned',
  'gpsIntegration.status.available',
  'gpsIntegration.status.ignored',
  'gpsIntegration.status.removed',
  'operator.singleTitle',
  'generic.total',
];

describe.each([
  ['en', en],
  ['es', es],
])('admin screens translations (%s)', (_, bundle) => {
  test('every manageAdmin section group has a label', () => {
    const missing = SECTION_GROUP_KEYS
      .map((key) => `manageAdmin.groups.${key}`)
      .filter((key) => typeof resolveKey(bundle, key) !== 'string');
    expect(missing).toEqual([]);
  });

  test('every GPS dashboard key resolves', () => {
    const missing = GPS_DASHBOARD_KEYS.filter((key) => typeof resolveKey(bundle, key) !== 'string');
    expect(missing).toEqual([]);
  });
});

describe('en/es key parity', () => {
  test.each(['manageAdmin', 'gpsIntegration'])('namespace %s keeps parity', (namespace) => {
    const bundles = { en, es } as unknown as Record<string, Record<string, Record<string, unknown>>>;
    expect(flatten(bundles.es[namespace] ?? {}).sort()).toEqual(
      flatten(bundles.en[namespace] ?? {}).sort()
    );
  });
});
