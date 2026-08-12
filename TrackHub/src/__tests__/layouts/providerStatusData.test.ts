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

import { pivotProviderStatus } from 'layouts/gpsintegration/data/providerStatusData';
import type { ProviderStatusItem } from 'layouts/gpsintegration/data/providerStatusData';

const item = (overrides: Partial<ProviderStatusItem>): ProviderStatusItem => ({
  operatorId: 'op-1',
  operatorName: 'Operator One',
  detectedStatus: 'AVAILABLE',
  count: 1,
  ...overrides,
});

describe('pivotProviderStatus', () => {
  test('empty and missing input produce no rows', () => {
    expect(pivotProviderStatus()).toEqual([]);
    expect(pivotProviderStatus([])).toEqual([]);
  });

  test('pivots one row per operator with per-status buckets and total', () => {
    const rows = pivotProviderStatus([
      item({ detectedStatus: 'ASSIGNED', count: 5 }),
      item({ detectedStatus: 'AVAILABLE', count: 3 }),
      item({ detectedStatus: 'IGNORED', count: 2 }),
      item({ detectedStatus: 'REMOVED', count: 1 }),
    ]);
    expect(rows).toHaveLength(1);
    expect(rows[0]).toMatchObject({
      operatorName: 'Operator One',
      assigned: 5,
      unassigned: 3,
      ignored: 2,
      removed: 1,
      total: 11,
    });
  });

  test('NEW counts as unassigned (same bucket as AVAILABLE)', () => {
    const rows = pivotProviderStatus([
      item({ detectedStatus: 'NEW', count: 2 }),
      item({ detectedStatus: 'AVAILABLE', count: 3 }),
    ]);
    expect(rows[0].unassigned).toBe(5);
    expect(rows[0].total).toBe(5);
  });

  test('sorts by total descending, then operator name', () => {
    const rows = pivotProviderStatus([
      item({ operatorId: 'a', operatorName: 'Alpha', detectedStatus: 'ASSIGNED', count: 1 }),
      item({ operatorId: 'b', operatorName: 'Bravo', detectedStatus: 'ASSIGNED', count: 9 }),
      item({ operatorId: 'c', operatorName: 'Charlie', detectedStatus: 'ASSIGNED', count: 1 }),
    ]);
    expect(rows.map((r) => r.operatorName)).toEqual(['Bravo', 'Alpha', 'Charlie']);
  });

  test('falls back to operator name, then "unknown", when ids are missing', () => {
    const rows = pivotProviderStatus([
      item({ operatorId: '', operatorName: 'Named Only', count: 2 }),
      item({ operatorId: '', operatorName: '', count: 4 }),
    ]);
    expect(rows.map((r) => r.operatorName)).toEqual(['unknown', 'Named Only']);
  });
});
