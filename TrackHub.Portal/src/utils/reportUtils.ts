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

import type { ReportPreview, ReportPreviewCell } from 'api/reporting/reports';
import type { TableColumn, TableRowData } from 'controls/Tables/Table';

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

/** ISO-8601 date/date-time detector for {@link formatPreviewCell}. */
const ISO_DATE_RE = /^\d{4}-\d{2}-\d{2}([T ]\d{2}:\d{2}(:\d{2})?(\.\d+)?(Z|[+-]\d{2}:?\d{2})?)?$/;

/**
 * Formats a preview cell for display: null → '-', booleans → yes/no glyph text,
 * ISO date strings → locale date-time, everything else stringified as-is.
 */
export function formatPreviewCell(value: ReportPreviewCell): string {
  if (value === null || value === undefined) return '-';
  if (typeof value === 'boolean') return value ? '✓' : '✗';
  if (typeof value === 'string' && ISO_DATE_RE.test(value)) {
    const parsed = new Date(value);
    if (!Number.isNaN(parsed.getTime())) return parsed.toLocaleString();
  }
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

/** Parameters accepted by {@link buildTableData}. */
interface BuildTableDataParams {
    list1?: unknown[];
    list2?: unknown[];
    list3?: unknown[];
    visibility?: boolean[];
    labels?: string[];
}

export interface VisibleDataFilter {
    visible: boolean;
    data: unknown[];
    label: string;
}

export interface VisibleFilter {
    visible: boolean;
    label: string;
}

/** The table data with visibility settings produced by {@link buildTableData}. */
export interface TableData {
    stringFilter1: VisibleDataFilter;
    stringFilter2: VisibleDataFilter;
    stringFilter3: VisibleDataFilter;
    dateTimeFilter1: VisibleFilter;
    dateTimeFilter2: VisibleFilter;
    dateTimeFilter3: VisibleFilter;
    numericFilter1: VisibleFilter;
    numericFilter2: VisibleFilter;
    numericFilter3: VisibleFilter;
}

/**
 * Builds table data with visibility settings for different filters.
 */
export const buildTableData = ({
    list1 = [],
    list2 = [],
    list3 = [],
    visibility = [false, false, false, false, false, false, false, false, false],
    labels = ['', '', '', '', '', '', '', '', '']
  }: BuildTableDataParams): TableData => ({
    stringFilter1: { visible: visibility[0], data: list1, label: labels[0] },
    stringFilter2: { visible: visibility[1], data: list2, label: labels[1] },
    stringFilter3: { visible: visibility[2], data: list3, label: labels[2] },
    dateTimeFilter1: { visible: visibility[3], label: labels[3] },
    dateTimeFilter2: { visible: visibility[4], label: labels[4] },
    dateTimeFilter3: { visible: visibility[5], label: labels[5] },
    numericFilter1: { visible: visibility[6], label: labels[6] },
    numericFilter2: { visible: visibility[7], label: labels[7] },
    numericFilter3: { visible: visibility[8], label: labels[8] }
});
