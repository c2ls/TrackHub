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

import type { GpsIntegrationDashboard } from 'api/manager/gpsDashboard';

/** A single (operator, detectedStatus, count) tuple from the dashboard. */
export type ProviderStatusItem = GpsIntegrationDashboard['deviceCountsByProviderStatus'][number];

/**
 * One pivoted row per operator. NEW and AVAILABLE both mean "no active
 * assignment" (mirrors the Manager reader derivation and the
 * `gpsIntegration.status.*` labels, which map both to "Unassigned").
 */
export interface ProviderStatusRow {
  operatorId: string;
  operatorName: string;
  assigned: number;
  unassigned: number;
  ignored: number;
  removed: number;
  total: number;
}

/**
 * Pivots the per-(operator, status) tuples into one row per operator with a
 * column per status bucket, sorted by device total (desc) then name — the
 * biggest fleets stay visible even with a hundred providers.
 */
export function pivotProviderStatus(items: ProviderStatusItem[] = []): ProviderStatusRow[] {
  const map = new Map<string, ProviderStatusRow>();
  items.forEach((item) => {
    const key = item.operatorId || item.operatorName || 'unknown';
    let row = map.get(key);
    if (!row) {
      row = {
        operatorId: key,
        operatorName: item.operatorName || key,
        assigned: 0,
        unassigned: 0,
        ignored: 0,
        removed: 0,
        total: 0,
      };
      map.set(key, row);
    }
    const count = item.count || 0;
    switch ((item.detectedStatus || '').toUpperCase()) {
      case 'ASSIGNED': row.assigned += count; break;
      case 'IGNORED': row.ignored += count; break;
      case 'REMOVED': row.removed += count; break;
      // NEW, AVAILABLE and anything unexpected count as unassigned.
      default: row.unassigned += count; break;
    }
    row.total += count;
  });
  return Array.from(map.values()).sort(
    (a, b) => b.total - a.total || a.operatorName.localeCompare(b.operatorName)
  );
}
