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
 * Operator health / sync-run GraphQL documents (Telemetry backend). These
 * reads were split out of the former dual-backend operator service. Codegen validates them against schemas/telemetry.graphql; values
 * always travel as variables.
 */

import { graphql } from './generated';

export const GetOperatorSyncRunsDocument = graphql(`
  query GetOperatorSyncRuns($accountId: UUID, $operatorId: UUID, $take: Int!) {
    operatorSyncRuns(query: { accountId: $accountId, operatorId: $operatorId, take: $take }) {
      operatorSyncRunId
      accountId
      operatorId
      triggerType
      result
      startedAt
      completedAt
      devicesSeen
      devicesAdded
      devicesUpdated
      devicesRemoved
      devicesIgnored
      positionsRead
      positionsAccepted
      positionsRejected
      errorCode
      errorMessage
      correlationId
    }
  }
`);

export const GetOperatorHealthDocument = graphql(`
  query GetOperatorHealth($operatorId: UUID!) {
    operatorHealth(query: { operatorId: $operatorId }) {
      operatorId
      healthStatus
      lastSuccessfulSyncAt
      lastFailedSyncAt
      lastDeviceSyncAt
      lastPositionSyncAt
      lastFailureCode
      lastFailureMessage
      lastLatencyMs
    }
  }
`);

export const GetOperatorHealthHistoryDocument = graphql(`
  query GetOperatorHealthHistory($operatorId: UUID!, $take: Int!) {
    operatorHealthHistory(query: { operatorId: $operatorId, take: $take }) {
      operatorHealthCheckId
      operatorId
      checkType
      status
      latencyMs
      startedAt
      completedAt
      errorCode
      errorMessage
      retryCount
      correlationId
    }
  }
`);
