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
 * Staff GraphQL documents (Manager backend). Codegen validates these against
 * schemas/manager.graphql and emits typed document nodes — values always travel
 * as variables, never string interpolation.
 */

import { graphql } from './generated';

export const StaffItemFragment = graphql(`
  fragment StaffItem on StaffMemberVm {
    staffId
    accountId
    name
    phone
    documentType
    documentNumber
    active
    employeeCode
    licenseNumber
    licenseExpiresAt
    defaultTransporterId
    staffType
    lastModified
    version
  }
`);

export const GetStaffMembersByAccountDocument = graphql(`
  query GetStaffMembersByAccount($accountId: UUID!, $search: String, $staffType: String, $skip: Int!, $take: Int!) {
    staffMembersByAccount(query: { accountId: $accountId, search: $search, staffType: $staffType, skip: $skip, take: $take }) {
      items {
        ...StaffItem
      }
      totalCount
    }
  }
`);

export const CreateStaffMemberDocument = graphql(`
  mutation CreateStaffMember($staff: StaffMemberDtoInput!) {
    createStaffMember(command: { staff: $staff }) {
      ...StaffItem
    }
  }
`);

export const UpdateStaffMemberDocument = graphql(`
  mutation UpdateStaffMember($staffId: UUID!, $staff: StaffMemberDtoInput!) {
    updateStaffMember(command: { staffId: $staffId, staff: $staff })
  }
`);

export const DeactivateStaffMemberDocument = graphql(`
  mutation DeactivateStaffMember($staffId: UUID!) {
    deactivateStaffMember(command: { staffId: $staffId })
  }
`);

export const StaffQualificationItemFragment = graphql(`
  fragment StaffQualificationItem on StaffQualificationVm {
    staffQualificationId
    accountId
    staffId
    staffName
    qualificationType
    category
    number
    issuedAt
    expiresAt
    issuingAuthority
    status
    documentId
    notes
    lastModified
  }
`);

export const StaffTransporterAssignmentItemFragment = graphql(`
  fragment StaffTransporterAssignmentItem on StaffTransporterAssignmentVm {
    staffTransporterAssignmentId
    accountId
    staffId
    staffName
    transporterId
    transporterName
    startsAt
    endsAt
    assignmentType
    status
    createdByPrincipal
    lastModified
  }
`);

/**
 * The authoritative "is this assignment active right now" projection (spec 09
 * §7.2): real active rows plus the synthesized default-transporter entry. It is
 * unpaged and time-aware (StartsAt <= now < EndsAt), which the paged history
 * query is not — `resourceId` carries the transporter id.
 */
export const StaffActiveAssignmentItemFragment = graphql(`
  fragment StaffActiveAssignmentItem on StaffAssignmentVm {
    staffId
    accountId
    resourceType
    resourceId
    active
    startsAt
    endsAt
    assignmentType
  }
`);

export const GetStaffAssignmentsDocument = graphql(`
  query GetStaffAssignments($staffId: UUID!) {
    staffAssignments(query: { staffId: $staffId }) {
      ...StaffActiveAssignmentItem
    }
  }
`);

export const GetStaffQualificationsDocument = graphql(`
  query GetStaffQualifications(
    $accountId: UUID!
    $staffId: UUID
    $expiringWithinDays: Int
    $skip: Int!
    $take: Int!
  ) {
    staffQualifications(
      query: {
        accountId: $accountId
        staffId: $staffId
        expiringWithinDays: $expiringWithinDays
        skip: $skip
        take: $take
      }
    ) {
      items {
        ...StaffQualificationItem
      }
      totalCount
    }
  }
`);

export const GetStaffAssignmentHistoryDocument = graphql(`
  query GetStaffAssignmentHistory(
    $accountId: UUID!
    $staffId: UUID
    $transporterId: UUID
    $from: DateTime
    $to: DateTime
    $skip: Int!
    $take: Int!
  ) {
    staffAssignmentHistory(
      query: {
        accountId: $accountId
        staffId: $staffId
        transporterId: $transporterId
        from: $from
        to: $to
        skip: $skip
        take: $take
      }
    ) {
      items {
        ...StaffTransporterAssignmentItem
      }
      totalCount
    }
  }
`);

export const CreateStaffQualificationDocument = graphql(`
  mutation CreateStaffQualification($qualification: StaffQualificationDtoInput!) {
    createStaffQualification(command: { qualification: $qualification }) {
      ...StaffQualificationItem
    }
  }
`);

export const UpdateStaffQualificationDocument = graphql(`
  mutation UpdateStaffQualification(
    $staffQualificationId: UUID!
    $qualification: StaffQualificationDtoInput!
  ) {
    updateStaffQualification(
      command: { staffQualificationId: $staffQualificationId, qualification: $qualification }
    )
  }
`);

export const DeleteStaffQualificationDocument = graphql(`
  mutation DeleteStaffQualification($staffQualificationId: UUID!) {
    deleteStaffQualification(command: { staffQualificationId: $staffQualificationId })
  }
`);

export const AssignStaffToTransporterDocument = graphql(`
  mutation AssignStaffToTransporter(
    $staffId: UUID!
    $transporterId: UUID!
    $startsAt: DateTime!
    $assignmentType: String!
  ) {
    assignStaffToTransporter(
      command: {
        staffId: $staffId
        transporterId: $transporterId
        startsAt: $startsAt
        assignmentType: $assignmentType
      }
    ) {
      ...StaffTransporterAssignmentItem
    }
  }
`);

export const EndStaffAssignmentDocument = graphql(`
  mutation EndStaffAssignment($staffTransporterAssignmentId: UUID!, $endsAt: DateTime) {
    endStaffAssignment(
      command: { staffTransporterAssignmentId: $staffTransporterAssignmentId, endsAt: $endsAt }
    )
  }
`);

export const GetStaffOptionsDocument = graphql(`
  query GetStaffOptions($search: String, $skip: Int, $take: Int) {
    staffOptions(query: { search: $search, skip: $skip, take: $take }) {
      items {
        staffId
        name
        active
      }
      totalCount
    }
  }
`);

export const GetStaffLookupDocument = graphql(`
  query GetStaffLookup($search: String, $staffIds: [UUID!]) {
    staffLookup(query: { search: $search, staffIds: $staffIds }) {
      staffId
      name
      active
    }
  }
`);
