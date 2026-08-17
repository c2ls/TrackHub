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

/** Keys the workforce surfaces render and that must resolve in both locales. */
const WORKFORCE_KEYS = [
  'workforce.searchDriver',
  'workforce.noDriverMatches',
  'workforce.selectDriverHint',
  'workforce.credentials.statusActive',
  'workforce.credentials.statusRevoked',
  'workforce.credentials.statusLocked',
  'workforce.credentials.statusPending',
  'workforce.qualifications.documentId',
  'workforce.qualifications.documentIdHelp',
  'workforce.qualifications.documentNone',
  'workforce.qualifications.documentUnavailable',
  'workforce.qualifications.documents',
  'workforce.assignments.defaultTransporter',
  'workforce.assignments.defaultTransporterHint',
  'workforce.assignments.tooManyResults',
  'workforce.assignments.emptyActive',
];

/** Keys that were present in both bundles but referenced by nothing — deleted. */
const REMOVED_KEYS = [
  'workforce.credentials.credential',
  'workforce.credentials.verifiedAt',
  'workforce.devices.deviceId',
  'workforce.devices.registeredAt',
  'workforce.devices.revokedAt',
];

describe.each([
  ['en', en],
  ['es', es],
])('workforce translations (%s)', (_, bundle) => {
  test('every workforce key rendered by the screens resolves', () => {
    const missing = WORKFORCE_KEYS.filter((key) => typeof resolveKey(bundle, key) !== 'string');
    expect(missing).toEqual([]);
  });

  test('the dead workforce keys are gone', () => {
    const remaining = REMOVED_KEYS.filter((key) => resolveKey(bundle, key) !== undefined);
    expect(remaining).toEqual([]);
  });
});

describe('en/es key parity', () => {
  test('the workforce namespace keeps exact parity', () => {
    const bundles = { en, es } as unknown as Record<string, Record<string, Record<string, unknown>>>;
    expect(flatten(bundles.es.workforce ?? {}).sort()).toEqual(
      flatten(bundles.en.workforce ?? {}).sort()
    );
  });
});
