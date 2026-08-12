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

/**
 * Escapes a single CSV cell (RFC 4180): wraps the value in double quotes
 * when it contains a comma, quote or line break and doubles inner quotes.
 */
function escapeCsvCell(value: unknown): string {
  if (value === undefined || value === null) return '';
  const text = String(value);
  return /[",\n\r]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text;
}

/**
 * Builds CSV content from a header row and data rows.
 */
export function buildCsvContent(headers: string[], rows: unknown[][]): string {
  return [headers, ...rows]
    .map(row => row.map(escapeCsvCell).join(','))
    .join('\r\n');
}

/**
 * Replaces characters that are unsafe in file names with dashes.
 */
export function sanitizeFileNamePart(value: unknown): string {
  return String(value ?? '')
    .replace(/[^\w.-]+/g, '-')
    .replace(/^-+|-+$/g, '');
}

/**
 * Triggers a client-side download of the given rows as a UTF-8 CSV file
 * (with BOM so Excel detects the encoding).
 */
export function downloadCsv(filename: string, headers: string[], rows: unknown[][]): void {
  const blob = new Blob(['﻿' + buildCsvContent(headers, rows)], {
    type: 'text/csv;charset=utf-8;'
  });
  const url = window.URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.setAttribute('download', filename);
  document.body.appendChild(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(url);
}
