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
 * GPS-integration dashboard GraphQL document (Manager backend). Codegen
 * validates this against schemas/manager.graphql; values travel as variables.
 */

import { graphql } from './generated';

export const GetGpsIntegrationDashboardDocument = graphql(`
  query GetGpsIntegrationDashboard($accountId: UUID!) {
    gpsIntegrationDashboard(query: { accountId: $accountId }) {
      operatorsTotal
      operatorsEnabled
      operatorsHealthy
      operatorsDegraded
      operatorsOffline
      devicesTotal
      devicesNew
      devicesAvailable
      devicesAssigned
      devicesIgnored
      devicesRemoved
      recentlyAddedDevicesLast24h
      unassignedDevicesCount
      syncRunsSucceededLast24h
      syncRunsFailedLast24h
      lastAutomaticSyncAt
      lastManualSyncAt
      averageSyncDurationSeconds
      deviceCountsByProviderStatus {
        operatorId
        operatorName
        detectedStatus
        count
      }
    }
  }
`);
