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
 * Operator GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql and emits typed document nodes — values
 * always travel as variables, never string interpolation. Operator health /
 * sync-run reads live on the Telemetry backend (see operatorHealthOperations).
 */

import { graphql } from './generated';

/** Full operator record used by the operators table (edit/list/create). */
export const OperatorDetailFragment = graphql(`
  fragment OperatorDetail on OperatorVm {
    operatorId
    name
    description
    phoneNumber
    emailAddress
    address
    contactName
    protocolType
    protocolTypeId
    enabled
    syncIntervalMinutes
    healthStatus
    lastSuccessfulSyncAt
    lastFailedSyncAt
    lastFailureCode
    lastLatencyMs
    lastDeviceSyncAt
    lastPositionSyncAt
    lastModified
  }
`);

/** Minimal operator record for pickers/filters (id + display name). */
export const OperatorSummaryFragment = graphql(`
  fragment OperatorSummary on OperatorVm {
    operatorId
    name
  }
`);

/** Operator record for the GPS-integration lists (name map + sync metadata). */
export const OperatorGpsFragment = graphql(`
  fragment OperatorGps on OperatorVm {
    operatorId
    name
    protocolType
    enabled
    lastDeviceSyncAt
    lastPositionSyncAt
    syncIntervalMinutes
  }
`);

export const GetOperatorDocument = graphql(`
  query GetOperator($id: UUID!) {
    operator(query: { id: $id }) {
      ...OperatorDetail
    }
  }
`);

export const GetOperatorsByCurrentAccountDocument = graphql(`
  query GetOperatorsByCurrentAccount {
    operatorsByCurrentAccount {
      ...OperatorDetail
    }
  }
`);

export const GetOperatorsSummaryDocument = graphql(`
  query GetOperatorsSummary {
    operatorsByCurrentAccount {
      ...OperatorSummary
    }
  }
`);

export const GetGpsOperatorsDocument = graphql(`
  query GetGpsOperators {
    operatorsByCurrentAccount {
      ...OperatorGps
    }
  }
`);

export const CreateOperatorDocument = graphql(`
  mutation CreateOperator($operator: OperatorDtoInput!) {
    createOperator(command: { operator: $operator }) {
      ...OperatorDetail
    }
  }
`);

export const UpdateOperatorDocument = graphql(`
  mutation UpdateOperator($id: UUID!, $operator: UpdateOperatorDtoInput!) {
    updateOperator(id: $id, command: { operator: $operator })
  }
`);

export const DeleteOperatorDocument = graphql(`
  mutation DeleteOperator($id: UUID!) {
    deleteOperator(id: $id)
  }
`);

export const SetOperatorEnabledDocument = graphql(`
  mutation SetOperatorEnabled($operatorId: UUID!, $enabled: Boolean!) {
    setOperatorEnabled(command: { operatorId: $operatorId, enabled: $enabled })
  }
`);

export const TriggerOperatorDeviceSyncDocument = graphql(`
  mutation TriggerOperatorDeviceSync($command: TriggerOperatorDeviceSyncCommandInput!) {
    triggerOperatorDeviceSync(command: $command)
  }
`);
