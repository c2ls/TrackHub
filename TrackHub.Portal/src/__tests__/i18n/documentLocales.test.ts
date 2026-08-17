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

const SRC = path.join(__dirname, '..', '..');

// Every source file that renders the Document Management screens.
const PANEL_SOURCES = [
  'layouts/manageadmin/components/documents/index.js',
  'layouts/manageadmin/components/documents/DocumentPanel/index.js',
  'layouts/manageadmin/components/documents/DocumentUploadDialog/index.js',
  'layouts/manageadmin/components/documents/DocumentTypeDialog/index.js',
  'layouts/manageadmin/components/documents/ShareDialog/index.js',
];

const resolveKey = (bundle: unknown, key: string): unknown =>
  key.split('.').reduce<unknown>(
    (acc, part) =>
      acc && typeof acc === 'object' ? (acc as Record<string, unknown>)[part] : undefined,
    bundle
  );

// Static t('...') keys that are NOT passed a defaultValue (those with a defaultValue are allowed to be
// absent from the bundles).
const staticKeysIn = (file: string): string[] => {
  // Extension-agnostic: source files move .js → .jsx → .tsx during the TS migration
  const base = path.join(SRC, file.replace(/\.js$/, ''));
  const resolved = ['.js', '.jsx', '.ts', '.tsx'].map((ext) => base + ext).find(fs.existsSync);
  if (!resolved) throw new Error(`Source file not found for any extension: ${file}`);
  const source = fs.readFileSync(resolved, 'utf8');
  const keys: string[] = [];
  const pattern = /\bt\(\s*['"]([A-Za-z0-9_.-]+)['"]\s*(,\s*\{[^}]*defaultValue[^}]*\})?/g;
  let match: RegExpExecArray | null;
  while ((match = pattern.exec(source)) !== null) {
    if (!match[2]) {
      keys.push(match[1]);
    }
  }
  return keys;
};

describe('Document Management panel translations', () => {
  const referencedKeys = [...new Set(PANEL_SOURCES.flatMap(staticKeysIn))];

  test('panels reference the document key surface', () => {
    expect(referencedKeys.some((k) => k.startsWith('documentManagement.'))).toBe(true);
  });

  test.each([
    ['en', en],
    ['es', es],
  ])('every referenced key resolves in %s', (_, bundle) => {
    const missing = referencedKeys.filter((key) => typeof resolveKey(bundle, key) !== 'string');
    expect(missing).toEqual([]);
  });

  test('en and es keep key parity for the documentManagement namespace', () => {
    const flatten = (obj: Record<string, unknown>, prefix = ''): string[] =>
      Object.entries(obj).flatMap(([key, value]) =>
        value && typeof value === 'object'
          ? flatten(value as Record<string, unknown>, `${prefix}${key}.`)
          : [`${prefix}${key}`]
      );
    expect(flatten((es.documentManagement ?? {}) as Record<string, unknown>).sort()).toEqual(
      flatten((en.documentManagement ?? {}) as Record<string, unknown>).sort()
    );
  });
});
