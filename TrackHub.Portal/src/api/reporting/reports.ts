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
 * Report generation and preview (Reporting backend — REST, not GraphQL).
 * `downloadReport` streams the generated xlsx/pdf through the shared
 * authenticated REST client; `previewReport` fetches the JSON preview dataset.
 * Failures THROW ApiError (blob error envelopes are parsed in restClient) —
 * fallbacks and toasts belong to the caller layer.
 */

import i18n from 'i18next';
import { restRequest, downloadFile } from 'api/core/restClient';
import { REST_ENDPOINTS } from 'api/core/endpoints';

/** The output format the caller requested. Backend defaults to xlsx when absent. */
export type ReportFormat = 'xlsx' | 'pdf';

/**
 * Named filter values, already wire-formatted by the filter form (filter name →
 * string value or null): dates as ISO-8601 strings, numbers with a dot decimal
 * separator. The names come from the catalog row's filter definitions; the
 * Reporting factories read the same names. Null/absent means "no filter".
 */
export type ReportFilterValues = Record<string, string | null>;

/** A preview column: positional `name` key plus its already-localized header. */
export interface ReportPreviewColumn {
  name: string;
  header: string;
}

/** A single preview cell value (positional, matching {@link ReportPreviewColumn}). */
export type ReportPreviewCell = string | number | boolean | null;

/** The JSON preview payload returned by `POST /api/BasicReports/preview`. */
export interface ReportPreview {
  columns: ReportPreviewColumn[];
  rows: ReportPreviewCell[][];
  totalRows: number;
  truncated: boolean;
}

/** Builds the FilterDto payload shared by download and preview requests. */
function buildFilters(name: string, filters: ReportFilterValues) {
  return {
    name,
    language: i18n.language || 'en',
    values: filters,
  };
}

/**
 * Generates the report server-side and saves it as `<reportName>.<format>`.
 * Defaults to xlsx; `pdf` is only valid for catalog rows advertising SupportsPdf.
 */
export async function downloadReport(
  reportCode: string,
  reportName: string,
  filters: ReportFilterValues,
  format: ReportFormat = 'xlsx'
): Promise<void> {
  const requestBody = {
    reportCode,
    filters: buildFilters(reportName, filters),
    format,
  };
  await downloadFile(
    REST_ENDPOINTS.reportingBasicReports,
    requestBody,
    `${reportName}.${format}`,
    { timeout: 60000 }
  );
}

/**
 * Fetches the first N rows of the report (server-capped) for on-screen preview.
 * Headers arrive already localized per the request language.
 */
export async function previewReport(
  reportCode: string,
  reportName: string,
  filters: ReportFilterValues
): Promise<ReportPreview> {
  return restRequest<ReportPreview>({
    method: 'POST',
    url: REST_ENDPOINTS.reportingBasicReportsPreview,
    data: { reportCode, filters: buildFilters(reportName, filters) },
    timeout: 60000,
  });
}
