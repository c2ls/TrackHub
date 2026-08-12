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
 * SyncWorker liveness document (Telemetry backend), administrator tier only.
 * The worker has no HTTP listener, so its state is derived from the recency of
 * the rows it writes. Codegen validates this against schemas/telemetry.graphql.
 */

import { graphql } from './generated';

export const GetPlatformSyncActivityDocument = graphql(`
  query GetPlatformSyncActivity($lookbackMinutes: Int!) {
    platformSyncActivity(query: { lookbackMinutes: $lookbackMinutes }) {
      lastSyncRunAt
      lastHealthCheckAt
      syncRunsLastHour
      healthChecksLastHour
      hasEnabledGpsIntegration
    }
  }
`);
