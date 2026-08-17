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
 * Transporter GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql and emits typed document nodes — values
 * always travel as variables, never string interpolation.
 */

import { graphql } from './generated';

export const TransporterItemFragment = graphql(`
  fragment TransporterItem on TransporterVm {
    transporterId
    name
    transporterType
    transporterTypeId
  }
`);

export const GetTransportersByAccountDocument = graphql(`
  query GetTransportersByAccount($skip: Int, $take: Int, $search: String) {
    transportersByAccount(query: { skip: $skip, take: $take, search: $search }) {
      items {
        ...TransporterItem
      }
      totalCount
    }
  }
`);

export const GetTransportersByGroupDocument = graphql(`
  query GetTransportersByGroup($groupId: Long!, $skip: Int, $take: Int, $search: String) {
    transportersByGroup(
      query: { groupId: $groupId, skip: $skip, take: $take, search: $search }
    ) {
      items {
        ...TransporterItem
      }
      totalCount
    }
  }
`);

/**
 * Id + name + type, unpaged by design. Two separate fields on purpose: the
 * admin screens see the whole account, the dashboard/reports/tripmanager see
 * only what the signed-in user may track. Never collapse them into one. The
 * type travels with the projection because the toll-class dialog derives its
 * whole transporter-type list from the picker feed.
 */
export const GetTransporterLookupByAccountDocument = graphql(`
  query GetTransporterLookupByAccount {
    transporterLookupByAccount {
      transporterId
      name
      transporterType
      transporterTypeId
    }
  }
`);

export const GetTransporterLookupByUserDocument = graphql(`
  query GetTransporterLookupByUser {
    transporterLookupByUser {
      transporterId
      name
      transporterType
      transporterTypeId
    }
  }
`);

export const CreateTransporterDocument = graphql(`
  mutation CreateTransporter($transporter: TransporterDtoInput!) {
    createTransporter(command: { transporter: $transporter }) {
      ...TransporterItem
    }
  }
`);

export const UpdateTransporterDocument = graphql(`
  mutation UpdateTransporter($id: UUID!, $transporter: UpdateTransporterDtoInput!) {
    updateTransporter(id: $id, command: { transporter: $transporter })
  }
`);

export const DeleteTransporterDocument = graphql(`
  mutation DeleteTransporter($id: UUID!) {
    deleteTransporter(id: $id)
  }
`);

export const AssignmentFieldsFragment = graphql(`
  fragment AssignmentFields on TransporterDeviceAssignmentVm {
    transporterDeviceAssignmentId
    accountId
    transporterId
    deviceId
    effectiveFrom
    effectiveTo
    priority
    isPrimary
    status
    assignmentReason
  }
`);

export const GetTransporterDeviceAssignmentsByAccountDocument = graphql(`
  query GetTransporterDeviceAssignmentsByAccount(
    $accountId: UUID!
    $activeOnly: Boolean!
    $skip: Int
    $take: Int
  ) {
    transporterDeviceAssignmentsByAccount(
      query: { accountId: $accountId, activeOnly: $activeOnly, skip: $skip, take: $take }
    ) {
      items {
        ...AssignmentFields
        createdByPrincipalType
        createdByPrincipalId
      }
      totalCount
    }
  }
`);

export const AssignDeviceToTransporterDocument = graphql(`
  mutation AssignDeviceToTransporter($assignment: TransporterDeviceAssignmentDtoInput!) {
    assignDeviceToTransporter(command: { assignment: $assignment }) {
      transporterDeviceAssignmentId
      deviceId
      transporterId
      effectiveFrom
      isPrimary
      status
    }
  }
`);

export const EndDeviceTransporterAssignmentDocument = graphql(`
  mutation EndDeviceTransporterAssignment($assignmentId: UUID!, $reason: String) {
    endDeviceTransporterAssignment(command: { assignmentId: $assignmentId, reason: $reason })
  }
`);
