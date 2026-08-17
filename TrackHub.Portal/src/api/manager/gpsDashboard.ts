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
 * GPS-integration dashboard API (Manager backend): plain typed async function.
 * Failures THROW ApiError — fallbacks and toasts belong to the caller layer
 * (src/queries).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type { GetGpsIntegrationDashboardQuery } from './generated/graphql';
import { GetGpsIntegrationDashboardDocument } from './gpsDashboardOperations';

export type GpsIntegrationDashboard =
  GetGpsIntegrationDashboardQuery['gpsIntegrationDashboard'];

export async function getGpsIntegrationDashboard(
  accountId: string
): Promise<GpsIntegrationDashboard> {
  const data = await executeGraphQL('manager', GetGpsIntegrationDashboardDocument, { accountId });
  return data.gpsIntegrationDashboard;
}
