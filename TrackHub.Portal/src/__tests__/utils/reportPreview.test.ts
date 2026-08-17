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

import { mapPreviewToTable, formatPreviewCell } from 'utils/reportUtils';
import type { ReportPreview } from 'api/reporting/reports';

describe('mapPreviewToTable', () => {
  test('maps positional rows onto column-name-keyed table rows', () => {
    const preview: ReportPreview = {
      columns: [
        { name: 'operator', header: 'Operator' },
        { name: 'count', header: 'Count' },
      ],
      rows: [
        ['Provider A', 12],
        ['Provider B', 7],
      ],
      totalRows: 2,
      truncated: false,
    };

    const { columns, rows } = mapPreviewToTable(preview);

    expect(columns).toEqual([
      { name: 'operator', title: 'Operator' },
      { name: 'count', title: 'Count' },
    ]);
    expect(rows).toEqual([
      { id: 0, operator: 'Provider A', count: 12 },
      { id: 1, operator: 'Provider B', count: 7 },
    ]);
  });

  test('fills missing positional cells with null', () => {
    const preview: ReportPreview = {
      columns: [
        { name: 'a', header: 'A' },
        { name: 'b', header: 'B' },
      ],
      rows: [['only-a']],
      totalRows: 1,
      truncated: false,
    };

    const { rows } = mapPreviewToTable(preview);
    expect(rows[0]).toEqual({ id: 0, a: 'only-a', b: null });
  });

  test('handles an empty dataset', () => {
    const preview: ReportPreview = { columns: [], rows: [], totalRows: 0, truncated: true };
    const { columns, rows } = mapPreviewToTable(preview);
    expect(columns).toEqual([]);
    expect(rows).toEqual([]);
  });
});

describe('formatPreviewCell', () => {
  test('renders null and undefined as a dash', () => {
    expect(formatPreviewCell(null)).toBe('-');
  });

  test('renders booleans as glyphs', () => {
    expect(formatPreviewCell(true)).toBe('✓');
    expect(formatPreviewCell(false)).toBe('✗');
  });

  test('passes through numbers and plain strings', () => {
    expect(formatPreviewCell(42)).toBe('42');
    expect(formatPreviewCell('Provider A')).toBe('Provider A');
  });

  test('formats ISO date strings to a locale string', () => {
    const iso = '2026-07-16T12:00:00Z';
    const formatted = formatPreviewCell(iso);
    expect(formatted).not.toBe(iso);
    expect(formatted).toBe(new Date(iso).toLocaleString());
  });
});
