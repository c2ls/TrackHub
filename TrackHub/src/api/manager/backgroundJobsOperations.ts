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

/**
 * Background-job GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql — values always travel as variables.
 */

import { graphql } from './generated';

export const BackgroundJobRunItemFragment = graphql(`
  fragment BackgroundJobRunItem on BackgroundJobRunVm {
    backgroundJobRunId
    jobKey
    accountId
    resourceKey
    idempotencyKey
    status
    attempts
    startedAt
    completedAt
    errorCode
    errorMessage
  }
`);

export const GetBackgroundJobRunsDocument = graphql(`
  query GetBackgroundJobRuns($accountId: UUID!, $skip: Int!, $take: Int!) {
    backgroundJobRuns(query: { accountId: $accountId, skip: $skip, take: $take }) {
      ...BackgroundJobRunItem
    }
  }
`);
