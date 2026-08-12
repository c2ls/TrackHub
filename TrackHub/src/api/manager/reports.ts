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
 * Report-catalog API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — fallbacks and toasts belong to the caller layer.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  ReportFieldsFragment as ReportFieldsType,
  UpdateReportDtoInput,
} from './generated/graphql';
import { GetReportsDocument, UpdateReportDocument } from './reportsOperations';

export type Report = ReportFieldsType;
export type { UpdateReportDtoInput };

export async function getReports(): Promise<Report[]> {
  const data = await executeGraphQL('manager', GetReportsDocument);
  return data.reports;
}

export async function updateReport(
  reportId: string,
  report: Omit<UpdateReportDtoInput, 'reportId'>
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateReportDocument, {
    id: reportId,
    report: { ...report, reportId },
  });
  return data.updateReport;
}
