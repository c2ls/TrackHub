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
import { toCamelCase } from 'utils/stringUtils';
import { parseFilterDefinitions } from 'layouts/reports/data/filtersData';

// The 30 seeded report codes (Manager CoreReportCatalogContribution). The filter form is
// catalog-driven, so this list only backs the i18n coverage gate below.
const SEEDED_CODES = [
  'LiveReport',
  'PositionRecord',
  'TransportersInGeofence',
  'GeofenceEvents',
  'gps.provider-health-summary',
  'gps.provider-sync-history',
  'gps.sync-statistics',
  'gps.synchronized-device-inventory',
  'gps.recently-added-devices',
  'gps.unassigned-devices',
  'gps.ignored-devices',
  'gps.assignment-history',
  'gps.latest-position-freshness',
  'gps.position-history',
  'documents-expiring',
  'documents-missing-required',
  'documents-share-activity',
  'documents-upload-volume',
  'workforce-driver-registry',
  'workforce-qualification-expirations',
  'workforce-assignment-history',
  'trip-summary',
  'trip-detail',
  'trip-on-time-performance',
  'trip-stop-dwell',
  'trip-toll-cost',
  'trip-pod-export',
  'accounts-by-status',
  'feature-enablement-matrix',
  'group-membership-export',
];

describe('parseFilterDefinitions', () => {
  test('parses a catalog filters document (the GeofenceEvents shape)', () => {
    const json = JSON.stringify([
      { name: 'transporterId', type: 'guid', labelKey: 'reports.transporter', source: 'transporters' },
      { name: 'geofenceId', type: 'guid', labelKey: 'reports.geofence', source: 'geofences' },
      { name: 'from', type: 'datetime', labelKey: 'reports.from' },
      { name: 'to', type: 'datetime', labelKey: 'reports.to' },
    ]);

    const definitions = parseFilterDefinitions(json);

    expect(definitions).toEqual([
      { name: 'transporterId', type: 'guid', labelKey: 'reports.transporter', source: 'transporters' },
      { name: 'geofenceId', type: 'guid', labelKey: 'reports.geofence', source: 'geofences' },
      { name: 'from', type: 'datetime', labelKey: 'reports.from', source: undefined },
      { name: 'to', type: 'datetime', labelKey: 'reports.to', source: undefined },
    ]);
  });

  test('null, empty, malformed and non-array documents yield an empty filter set', () => {
    expect(parseFilterDefinitions(null)).toEqual([]);
    expect(parseFilterDefinitions(undefined)).toEqual([]);
    expect(parseFilterDefinitions('')).toEqual([]);
    expect(parseFilterDefinitions('not json')).toEqual([]);
    expect(parseFilterDefinitions('{"name":"x"}')).toEqual([]);
    expect(parseFilterDefinitions('[]')).toEqual([]);
  });

  test('drops malformed entries and keeps the valid ones', () => {
    const json = JSON.stringify([
      { name: 'from', type: 'datetime', labelKey: 'reports.from' },
      { type: 'guid', labelKey: 'reports.transporter' }, // no name
      { name: 'x', type: 'guid' }, // no labelKey
      'not-an-object',
      null,
    ]);

    expect(parseFilterDefinitions(json).map((d) => d.name)).toEqual(['from']);
  });

  // A newer backend may ship datatypes or picker sources this build does not know;
  // the form must degrade to a free input, never break.
  test('unknown datatype falls back to text; unknown source falls back to a free input', () => {
    const json = JSON.stringify([
      { name: 'weird', type: 'money', labelKey: 'reports.maxRows', source: 'drivers' },
    ]);

    expect(parseFilterDefinitions(json)).toEqual([
      { name: 'weird', type: 'text', labelKey: 'reports.maxRows', source: undefined },
    ]);
  });
});

describe('report catalog i18n coverage (acceptance #9)', () => {
  const resolve = (bundle: unknown, key: string): unknown =>
    key.split('.').reduce<unknown>(
      (acc, part) => (acc && typeof acc === 'object' ? (acc as Record<string, unknown>)[part] : undefined),
      bundle
    );

  test.each(SEEDED_CODES)('%s has a non-empty EN + ES label and description', (code) => {
    const camel = toCamelCase(code);
    for (const bundle of [en, es]) {
      const label = resolve(bundle, `reportList.${camel}`);
      const description = resolve(bundle, `reportDescriptions.${camel}`);
      expect(typeof label === 'string' && label.length > 0).toBe(true);
      expect(typeof description === 'string' && description.length > 0).toBe(true);
    }
  });

  // Every label key the seeded catalog's filter definitions reference must resolve in both
  // bundles — the backend ships keys, never localized strings.
  const FILTER_LABEL_KEYS = [
    'reports.transporter',
    'reports.operator',
    'reports.geofence',
    'reports.device',
    'reports.status',
    'reports.from',
    'reports.to',
    'reports.maxRows',
    'reports.withinDays',
    'reports.lookbackHours',
    'reports.all',
  ];

  test.each(FILTER_LABEL_KEYS)('%s resolves in EN + ES', (key) => {
    for (const bundle of [en, es]) {
      const label = resolve(bundle, key);
      expect(typeof label === 'string' && label.length > 0).toBe(true);
    }
  });
});
