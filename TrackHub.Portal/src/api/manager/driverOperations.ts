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
 * Driver GraphQL documents (Manager backend). Codegen validates these against
 * schemas/manager.graphql and emits typed document nodes — values always travel
 * as variables, never string interpolation.
 */

import { graphql } from './generated';

export const DriverItemFragment = graphql(`
  fragment DriverItem on DriverVm {
    driverId
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
    lastModified
  }
`);

export const GetDriversByAccountDocument = graphql(`
  query GetDriversByAccount($accountId: UUID!, $skip: Int!, $take: Int!) {
    driversByAccount(query: { accountId: $accountId, skip: $skip, take: $take }) {
      ...DriverItem
    }
  }
`);

export const CreateDriverDocument = graphql(`
  mutation CreateDriver($driver: DriverDtoInput!) {
    createDriver(command: { driver: $driver }) {
      ...DriverItem
    }
  }
`);

export const UpdateDriverDocument = graphql(`
  mutation UpdateDriver($driverId: UUID!, $driver: DriverDtoInput!) {
    updateDriver(command: { driverId: $driverId, driver: $driver })
  }
`);

export const DeactivateDriverDocument = graphql(`
  mutation DeactivateDriver($driverId: UUID!) {
    deactivateDriver(command: { driverId: $driverId })
  }
`);

export const DriverQualificationItemFragment = graphql(`
  fragment DriverQualificationItem on DriverQualificationVm {
    driverQualificationId
    accountId
    driverId
    driverName
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

export const DriverTransporterAssignmentItemFragment = graphql(`
  fragment DriverTransporterAssignmentItem on DriverTransporterAssignmentVm {
    driverTransporterAssignmentId
    accountId
    driverId
    driverName
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
export const DriverActiveAssignmentItemFragment = graphql(`
  fragment DriverActiveAssignmentItem on DriverAssignmentVm {
    driverId
    accountId
    resourceType
    resourceId
    active
    startsAt
    endsAt
    assignmentType
  }
`);

export const GetDriverAssignmentsDocument = graphql(`
  query GetDriverAssignments($driverId: UUID!) {
    driverAssignments(query: { driverId: $driverId }) {
      ...DriverActiveAssignmentItem
    }
  }
`);

export const GetDriverQualificationsDocument = graphql(`
  query GetDriverQualifications(
    $accountId: UUID!
    $driverId: UUID
    $expiringWithinDays: Int
    $skip: Int!
    $take: Int!
  ) {
    driverQualifications(
      query: {
        accountId: $accountId
        driverId: $driverId
        expiringWithinDays: $expiringWithinDays
        skip: $skip
        take: $take
      }
    ) {
      ...DriverQualificationItem
    }
  }
`);

export const GetDriverAssignmentHistoryDocument = graphql(`
  query GetDriverAssignmentHistory(
    $accountId: UUID!
    $driverId: UUID
    $transporterId: UUID
    $from: DateTime
    $to: DateTime
    $skip: Int!
    $take: Int!
  ) {
    driverAssignmentHistory(
      query: {
        accountId: $accountId
        driverId: $driverId
        transporterId: $transporterId
        from: $from
        to: $to
        skip: $skip
        take: $take
      }
    ) {
      ...DriverTransporterAssignmentItem
    }
  }
`);

export const CreateDriverQualificationDocument = graphql(`
  mutation CreateDriverQualification($qualification: DriverQualificationDtoInput!) {
    createDriverQualification(command: { qualification: $qualification }) {
      ...DriverQualificationItem
    }
  }
`);

export const UpdateDriverQualificationDocument = graphql(`
  mutation UpdateDriverQualification(
    $driverQualificationId: UUID!
    $qualification: DriverQualificationDtoInput!
  ) {
    updateDriverQualification(
      command: { driverQualificationId: $driverQualificationId, qualification: $qualification }
    )
  }
`);

export const DeleteDriverQualificationDocument = graphql(`
  mutation DeleteDriverQualification($driverQualificationId: UUID!) {
    deleteDriverQualification(command: { driverQualificationId: $driverQualificationId })
  }
`);

export const AssignDriverToTransporterDocument = graphql(`
  mutation AssignDriverToTransporter(
    $driverId: UUID!
    $transporterId: UUID!
    $startsAt: DateTime!
    $assignmentType: String!
  ) {
    assignDriverToTransporter(
      command: {
        driverId: $driverId
        transporterId: $transporterId
        startsAt: $startsAt
        assignmentType: $assignmentType
      }
    ) {
      ...DriverTransporterAssignmentItem
    }
  }
`);

export const EndDriverAssignmentDocument = graphql(`
  mutation EndDriverAssignment($driverTransporterAssignmentId: UUID!, $endsAt: DateTime) {
    endDriverAssignment(
      command: { driverTransporterAssignmentId: $driverTransporterAssignmentId, endsAt: $endsAt }
    )
  }
`);
