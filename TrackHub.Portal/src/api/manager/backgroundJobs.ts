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
 * Background-job API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — consumed imperatively (load-on-expand), caller owns try/catch.
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type { BackgroundJobRunItemFragment as BackgroundJobRunItemType } from './generated/graphql';
import { GetBackgroundJobRunsDocument } from './backgroundJobsOperations';

export type BackgroundJobRun = BackgroundJobRunItemType;

export async function getBackgroundJobRuns(
  accountId: string,
  skip = 0,
  take = 50
): Promise<BackgroundJobRun[]> {
  const data = await executeGraphQL('manager', GetBackgroundJobRunsDocument, {
    accountId,
    skip,
    take,
  });
  return data.backgroundJobRuns;
}
