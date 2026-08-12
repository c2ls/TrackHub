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

import fs from 'fs';
import path from 'path';
import en from 'locales/en.json';
import es from 'locales/es.json';
import { poiTypes } from 'data/poiTypes';
import { geocodingProviderTypes } from 'data/geocodingProviderTypes';
import { colors } from 'data/colors';
import { toCamelCase } from 'utils/stringUtils';

const SRC = path.join(__dirname, '..', '..');

// Every source file that renders the POI and Geocoding Providers panels.
const PANEL_SOURCES = [
  'layouts/manageadmin/components/pois/index.js',
  'layouts/manageadmin/components/pois/PoiDialog/index.js',
  'layouts/manageadmin/data/poisTableData.js',
  'layouts/systemadmin/components/geocodingProviders/index.js',
  'layouts/systemadmin/components/geocodingProviders/GeocodingProviderDialog/index.js',
  'layouts/systemadmin/data/geocodingProvidersTableData.js',
  'controls/Maps/core/poiUtils.js',
];

const resolveKey = (bundle: unknown, key: string): unknown =>
  key.split('.').reduce<unknown>(
    (acc, part) =>
      acc && typeof acc === 'object' ? (acc as Record<string, unknown>)[part] : undefined,
    bundle
  );

const staticKeysIn = (file: string): string[] => {
  // Extension-agnostic: source files move .js → .jsx → .tsx during the TS migration
  const base = path.join(SRC, file.replace(/\.js$/, ''));
  const resolved = ['.js', '.jsx', '.ts', '.tsx'].map((ext) => base + ext).find(fs.existsSync);
  if (!resolved) throw new Error(`Source file not found for any extension: ${file}`);
  const source = fs.readFileSync(resolved, 'utf8');
  const keys: string[] = [];
  const pattern = /\bt\(\s*['"]([A-Za-z0-9_.-]+)['"]/g;
  let match: RegExpExecArray | null;
  while ((match = pattern.exec(source)) !== null) {
    keys.push(match[1]);
  }
  return keys;
};

// Dynamic keys built from the data catalogs at runtime.
const dynamicKeys = [
  ...poiTypes.map((type) => `poi.types.${toCamelCase(type.label)}`),
  ...geocodingProviderTypes.map((type) => `geocodingProviders.types.${toCamelCase(type.label)}`),
  ...colors.map((color) => `colors.${color.label.toLowerCase()}`),
];

describe('POI and Geocoding Providers panel translations', () => {
  const referencedKeys = [
    ...new Set([...PANEL_SOURCES.flatMap(staticKeysIn), ...dynamicKeys]),
  ];

  test('panels reference at least the known key surface', () => {
    expect(referencedKeys.length).toBeGreaterThan(30);
  });

  test.each([
    ['en', en],
    ['es', es],
  ])('every referenced key resolves in %s', (_, bundle) => {
    const missing = referencedKeys.filter(
      (key) => typeof resolveKey(bundle, key) !== 'string'
    );
    expect(missing).toEqual([]);
  });

  test('en and es keep key parity for the poi and geocodingProviders namespaces', () => {
    const flatten = (obj: Record<string, unknown>, prefix = ''): string[] =>
      Object.entries(obj).flatMap(([key, value]) =>
        value && typeof value === 'object'
          ? flatten(value as Record<string, unknown>, `${prefix}${key}.`)
          : [`${prefix}${key}`]
      );
    const enNs = en as Record<string, unknown>;
    const esNs = es as Record<string, unknown>;
    for (const ns of ['poi', 'geocodingProviders']) {
      expect(flatten((esNs[ns] ?? {}) as Record<string, unknown>).sort()).toEqual(
        flatten((enNs[ns] ?? {}) as Record<string, unknown>).sort()
      );
    }
  });
});
