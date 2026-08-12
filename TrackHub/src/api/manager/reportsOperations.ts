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
 * Report-catalog GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql — values always travel as variables. The
 * generated .xlsx itself is produced by the Reporting REST service (see
 * api/reporting/reports.ts); this file only manages the report catalog.
 */

import { graphql } from './generated';

export const ReportFieldsFragment = graphql(`
  fragment ReportFields on ReportVm {
    reportId
    code
    description
    type
    typeId
    active
    category
    requiredFeatureKey
    managerOnly
    supportsPdf
    sortOrder
  }
`);

export const GetReportsDocument = graphql(`
  query GetReports {
    reports {
      ...ReportFields
    }
  }
`);

export const UpdateReportDocument = graphql(`
  mutation UpdateReport($id: UUID!, $report: UpdateReportDtoInput!) {
    updateReport(id: $id, command: { report: $report })
  }
`);
