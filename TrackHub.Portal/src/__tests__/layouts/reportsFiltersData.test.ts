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

import type { TFunction } from 'i18next';
import type { SelectListItem } from 'controls/Dialogs/CustomSelect';
import en from 'locales/en.json';
import es from 'locales/es.json';
import { toCamelCase } from 'utils/stringUtils';
import {
  REPORT_FILTER_SPECS,
  DEFAULT_FILTER_SPEC,
  getReportFilterSpec,
  reportNeedsTransporters,
  reportNeedsOperators,
  getStringInputKinds,
  buildFilterTableData,
} from 'layouts/reports/data/filtersData';

// Passthrough translator: returns the key so we can assert label wiring.
const t = ((key: string) => key) as unknown as TFunction;

// The 21 seeded report codes. Every one must have a strategy so the
// silent empty-filter fallback is gone.
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
  'accounts-by-status',
  'feature-enablement-matrix',
  'group-membership-export',
];

describe('report filter strategies', () => {
  test('every seeded report code has a registered strategy', () => {
    expect(SEEDED_CODES).toHaveLength(21);
    for (const code of SEEDED_CODES) {
      expect(REPORT_FILTER_SPECS[code]).toBeDefined();
    }
    expect(Object.keys(REPORT_FILTER_SPECS)).toHaveLength(21);
  });

  test('unknown code yields the explicit date-range default', () => {
    expect(getReportFilterSpec('does-not-exist')).toEqual(['from', 'to']);
    expect(getReportFilterSpec('does-not-exist')).toBe(DEFAULT_FILTER_SPEC);
  });

  test('only transporter-scoped reports need the transporter list', () => {
    const transporterReports = SEEDED_CODES.filter(reportNeedsTransporters);
    expect(transporterReports).toEqual([
      'PositionRecord',
      'GeofenceEvents',
      'gps.assignment-history',
      'gps.position-history',
    ]);
  });

  test('only operator-scoped reports need the operator list', () => {
    const operatorReports = SEEDED_CODES.filter(reportNeedsOperators);
    expect(operatorReports).toEqual([
      'gps.provider-sync-history',
      'gps.synchronized-device-inventory',
      'gps.ignored-devices',
    ]);
  });
});

describe('buildFilterTableData', () => {
  const transporters: SelectListItem[] = [{ value: 't-1', label: 'Unit 1' }];
  const operators: SelectListItem[] = [{ value: 'o-1', label: 'Provider 1' }];
  const pickers = { transporters, operators };

  test('empty spec hides every filter', () => {
    const data = buildFilterTableData([], pickers, t);
    expect(data.stringFilter1.visible).toBe(false);
    expect(data.stringFilter2.visible).toBe(false);
    expect(data.dateTimeFilter1.visible).toBe(false);
    expect(data.numericFilter1.visible).toBe(false);
  });

  test('transporter + date range maps to stringFilter1 and dateTimeFilter1/2', () => {
    const data = buildFilterTableData(['transporter', 'from', 'to'], pickers, t);
    expect(data.stringFilter1.visible).toBe(true);
    expect(data.stringFilter1.data).toBe(transporters);
    expect(data.stringFilter1.label).toBe('reports.transporter');
    expect(data.dateTimeFilter1.visible).toBe(true);
    expect(data.dateTimeFilter1.label).toBe('reports.from');
    expect(data.dateTimeFilter2.visible).toBe(true);
    expect(data.dateTimeFilter2.label).toBe('reports.to');
    expect(data.numericFilter1.visible).toBe(false);
  });

  test('operator maps the operator list onto stringFilter1', () => {
    const data = buildFilterTableData(['operator'], pickers, t);
    expect(data.stringFilter1.visible).toBe(true);
    expect(data.stringFilter1.data).toBe(operators);
    expect(data.stringFilter1.label).toBe('reports.operator');
  });

  test('device maps a free-text input onto stringFilter2', () => {
    const data = buildFilterTableData(['transporter', 'device'], pickers, t);
    expect(data.stringFilter2.visible).toBe(true);
    expect(data.stringFilter2.label).toBe('reports.device');
    expect(getStringInputKinds(['transporter', 'device'])).toEqual(['select', 'text', 'select']);
  });

  test('string slots default to select pickers', () => {
    expect(getStringInputKinds(['transporter'])).toEqual(['select', 'select', 'select']);
    expect(getStringInputKinds([])).toEqual(['select', 'select', 'select']);
  });

  test('maxRows maps to numericFilter1', () => {
    const data = buildFilterTableData(['maxRows'], pickers, t);
    expect(data.numericFilter1.visible).toBe(true);
    expect(data.numericFilter1.label).toBe('reports.maxRows');
  });

  test('withinDays maps to numericFilter1', () => {
    const data = buildFilterTableData(['withinDays'], pickers, t);
    expect(data.numericFilter1.visible).toBe(true);
    expect(data.numericFilter1.label).toBe('reports.withinDays');
  });

  test('status maps a five-option AccountStatus select onto stringFilter1', () => {
    const data = buildFilterTableData(['status'], pickers, t);
    expect(data.stringFilter1.visible).toBe(true);
    expect(data.stringFilter1.label).toBe('reports.status');
    expect(data.stringFilter1.data).toHaveLength(5);
    expect((data.stringFilter1.data as SelectListItem[]).map((o) => o.value)).toEqual([
      'TRIAL',
      'ACTIVE',
      'SUSPENDED',
      'CANCELLED',
      'ARCHIVED',
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
});
