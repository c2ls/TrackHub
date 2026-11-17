/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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
import type { Report } from 'api/manager/reports';
import type { ReportPreview, ReportPreviewCell } from 'api/reporting/reports';
import type { TableColumn, TableRowData } from 'controls/Tables/Table';
import { toCamelCase } from 'utils/stringUtils';
import { formatDateOnly, formatDateTime } from 'utils/dateUtils';

/**
 * The screen name of a catalog report and the sentence under it. Falls back to the
 * row's own description, so a report this build has no translation for is still
 * named rather than shown as a raw key.
 */
export function reportLabels(
  t: TFunction,
  report: Pick<Report, 'code' | 'description'>
): { name: string; description: string } {
  const key = toCamelCase(report.code);
  return {
    name: t(`reportList.${key}` as 'reportList.liveReport', {
      defaultValue: report.description || report.code,
    }),
    description: t(`reportDescriptions.${key}` as 'reportDescriptions.liveReport', {
      defaultValue: '',
    }),
  };
}

/**
 * Maps a preview payload's positional rows onto the {@link Table} shape: each
 * cell array is keyed by its column's `name`, with a stable `id` per row. Column
 * headers (already localized by the backend) become the table column titles.
 * Pure — cell display formatting is applied separately at render time.
 */
export function mapPreviewToTable(preview: ReportPreview): {
  columns: TableColumn[];
  rows: TableRowData[];
} {
  const columns: TableColumn[] = preview.columns.map((column) => ({
    name: column.name,
    title: column.header,
  }));

  const rows: TableRowData[] = preview.rows.map((cells, rowIndex) => {
    const row: TableRowData = { id: rowIndex };
    preview.columns.forEach((column, columnIndex) => {
      row[column.name] = cells[columnIndex] ?? null;
    });
    return row;
  });

  return { columns, rows };
}

const DATE_ONLY_RE = /^\d{4}-\d{2}-\d{2}$/;
const ISO_DATE_TIME_RE = /^\d{4}-\d{2}-\d{2}[T ]\d{2}:\d{2}(:\d{2})?(\.\d+)?(Z|[+-]\d{2}:?\d{2})?$/;

/**
 * Formats a preview cell for display: null → '-', booleans → yes/no glyph text,
 * ISO dates → the app's date / date-time format, everything else stringified as-is.
 */
export function formatPreviewCell(value: ReportPreviewCell): string {
  if (value === null || value === undefined) return '-';
  if (typeof value === 'boolean') return value ? '✓' : '✗';
  if (typeof value === 'string' && DATE_ONLY_RE.test(value)) return formatDateOnly(value) || value;
  if (typeof value === 'string' && ISO_DATE_TIME_RE.test(value)) return formatDateTime(value) || value;
  return String(value);
}

/**
 * Fetches a list using the provided fetch function and maps the result using the provided map function.
 */
export const fetchList = async <T, U>(
    fetchFunction: () => Promise<T[]>,
    mapFunction: (value: T, index: number, array: T[]) => U
): Promise<U[]> => {
    const result = await fetchFunction();
    return result.map(mapFunction);
};

