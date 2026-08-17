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
 * Report catalog + execution hooks. The catalog is a Manager GraphQL read
 * (server-filtered to the caller's visible reports); generation/preview are
 * Reporting REST calls. Components consume these — not the api layer directly.
 * Failures surface in the global toast via the query client's error handlers.
 */

import { useMutation, useQuery } from '@tanstack/react-query';
import { getReports } from 'api/manager/reports';
import type { Report } from 'api/manager/reports';
import { downloadReport, previewReport } from 'api/reporting/reports';
import type {
  ReportFilterValues,
  ReportFormat,
  ReportPreview,
} from 'api/reporting/reports';

export const reportKeys = {
  all: ['reports'] as const,
  catalog: () => [...reportKeys.all, 'catalog'] as const,
};

/** Server-filtered report catalog for the current user (feature/role scoped). */
export function useReportCatalog(options: { enabled?: boolean } = {}) {
  return useQuery<Report[]>({
    queryKey: reportKeys.catalog(),
    queryFn: getReports,
    enabled: options.enabled ?? true,
  });
}

/** Arguments shared by the preview and download mutations. */
export interface ReportRunArgs {
  reportCode: string;
  reportName: string;
  filters: ReportFilterValues;
}

/** Fires the on-screen preview (POST /preview) for the selected report. */
export function useReportPreview() {
  return useMutation<ReportPreview, unknown, ReportRunArgs>({
    mutationFn: ({ reportCode, reportName, filters }) =>
      previewReport(reportCode, reportName, filters),
  });
}

/** Streams the generated xlsx/pdf file for the selected report. */
export function useDownloadReport() {
  return useMutation<void, unknown, ReportRunArgs & { format: ReportFormat }>({
    mutationFn: ({ reportCode, reportName, filters, format }) =>
      downloadReport(reportCode, reportName, filters, format),
  });
}
