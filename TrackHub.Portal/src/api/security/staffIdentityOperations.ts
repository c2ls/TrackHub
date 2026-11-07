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
 * Staff identity GraphQL documents (Security backend) — staff credentials and
 * device registrations. Codegen validates these against schemas/security.graphql;
 * values always travel as variables.
 *
 * Device registration itself (registerStaffDevice / updateStaffDevicePushToken)
 * belongs to the staff session (spec 10) and is deliberately NOT exposed here:
 * the portal only lists and revokes devices. The VMs are already redacted
 * server-side (no password hash, push token masked to its last 6 characters,
 * refresh-token family id absent from the VM entirely).
 */

import { graphql } from './generated';

export const StaffCredentialItemFragment = graphql(`
  fragment StaffCredentialItem on StaffCredentialVm {
    staffCredentialId
    staffId
    accountId
    normalizedLogin
    failedAttempts
    lockedUntil
    verifiedAt
    lastLoginAt
    active
    resetRequired
    lastModified
  }
`);

export const StaffDeviceItemFragment = graphql(`
  fragment StaffDeviceItem on StaffDeviceRegistrationVm {
    staffDeviceRegistrationId
    staffId
    accountId
    deviceId
    deviceName
    platform
    appVersion
    pushToken
    active
    registeredAt
    lastSeenAt
    revokedAt
    revokedBy
    lastModified
  }
`);

export const GetStaffCredentialsDocument = graphql(`
  query GetStaffCredentials($accountId: UUID!, $staffId: UUID, $skip: Int!, $take: Int!) {
    staffCredentials(
      query: { accountId: $accountId, staffId: $staffId, skip: $skip, take: $take }
    ) {
      ...StaffCredentialItem
    }
  }
`);

export const GetStaffDevicesDocument = graphql(`
  query GetStaffDevices($accountId: UUID!, $staffId: UUID, $skip: Int!, $take: Int!) {
    staffDevices(query: { accountId: $accountId, staffId: $staffId, skip: $skip, take: $take }) {
      ...StaffDeviceItem
    }
  }
`);

export const CreateStaffCredentialDocument = graphql(`
  mutation CreateStaffCredential($credential: StaffCredentialDtoInput!) {
    createStaffCredential(command: { credential: $credential }) {
      ...StaffCredentialItem
    }
  }
`);

export const ActivateStaffCredentialDocument = graphql(`
  mutation ActivateStaffCredential($staffCredentialId: UUID!, $password: String!) {
    activateStaffCredential(
      command: { staffCredentialId: $staffCredentialId, password: $password }
    )
  }
`);

export const LockStaffCredentialDocument = graphql(`
  mutation LockStaffCredential($staffCredentialId: UUID!, $lockedUntil: DateTime!) {
    lockStaffCredential(
      command: { staffCredentialId: $staffCredentialId, lockedUntil: $lockedUntil }
    )
  }
`);

export const ResetStaffCredentialDocument = graphql(`
  mutation ResetStaffCredential(
    $staffCredentialId: UUID!
    $password: String!
    $resetRequired: Boolean!
  ) {
    resetStaffCredential(
      command: {
        staffCredentialId: $staffCredentialId
        password: $password
        resetRequired: $resetRequired
      }
    )
  }
`);

export const RevokeStaffCredentialDocument = graphql(`
  mutation RevokeStaffCredential($staffCredentialId: UUID!) {
    revokeStaffCredential(command: { staffCredentialId: $staffCredentialId })
  }
`);

export const RevokeStaffDeviceDocument = graphql(`
  mutation RevokeStaffDevice($staffDeviceRegistrationId: UUID!, $revokedBy: String!) {
    revokeStaffDevice(
      command: { staffDeviceRegistrationId: $staffDeviceRegistrationId, revokedBy: $revokedBy }
    )
  }
`);
