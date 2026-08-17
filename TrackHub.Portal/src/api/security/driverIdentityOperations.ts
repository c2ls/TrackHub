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
 * Driver identity GraphQL documents (Security backend) — driver credentials and
 * device registrations. Codegen validates these against schemas/security.graphql;
 * values always travel as variables.
 *
 * Device registration itself (registerDriverDevice / updateDriverDevicePushToken)
 * belongs to the driver session (spec 10) and is deliberately NOT exposed here:
 * the portal only lists and revokes devices. The VMs are already redacted
 * server-side (no password hash, push token masked to its last 6 characters,
 * refresh-token family id absent from the VM entirely).
 */

import { graphql } from './generated';

export const DriverCredentialItemFragment = graphql(`
  fragment DriverCredentialItem on DriverCredentialVm {
    driverCredentialId
    driverId
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

export const DriverDeviceItemFragment = graphql(`
  fragment DriverDeviceItem on DriverDeviceRegistrationVm {
    driverDeviceRegistrationId
    driverId
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

export const GetDriverCredentialsDocument = graphql(`
  query GetDriverCredentials($accountId: UUID!, $driverId: UUID, $skip: Int!, $take: Int!) {
    driverCredentials(
      query: { accountId: $accountId, driverId: $driverId, skip: $skip, take: $take }
    ) {
      ...DriverCredentialItem
    }
  }
`);

export const GetDriverDevicesDocument = graphql(`
  query GetDriverDevices($accountId: UUID!, $driverId: UUID, $skip: Int!, $take: Int!) {
    driverDevices(query: { accountId: $accountId, driverId: $driverId, skip: $skip, take: $take }) {
      ...DriverDeviceItem
    }
  }
`);

export const CreateDriverCredentialDocument = graphql(`
  mutation CreateDriverCredential($credential: DriverCredentialDtoInput!) {
    createDriverCredential(command: { credential: $credential }) {
      ...DriverCredentialItem
    }
  }
`);

export const ActivateDriverCredentialDocument = graphql(`
  mutation ActivateDriverCredential($driverCredentialId: UUID!, $password: String!) {
    activateDriverCredential(
      command: { driverCredentialId: $driverCredentialId, password: $password }
    )
  }
`);

export const LockDriverCredentialDocument = graphql(`
  mutation LockDriverCredential($driverCredentialId: UUID!, $lockedUntil: DateTime!) {
    lockDriverCredential(
      command: { driverCredentialId: $driverCredentialId, lockedUntil: $lockedUntil }
    )
  }
`);

export const ResetDriverCredentialDocument = graphql(`
  mutation ResetDriverCredential(
    $driverCredentialId: UUID!
    $password: String!
    $resetRequired: Boolean!
  ) {
    resetDriverCredential(
      command: {
        driverCredentialId: $driverCredentialId
        password: $password
        resetRequired: $resetRequired
      }
    )
  }
`);

export const RevokeDriverCredentialDocument = graphql(`
  mutation RevokeDriverCredential($driverCredentialId: UUID!) {
    revokeDriverCredential(command: { driverCredentialId: $driverCredentialId })
  }
`);

export const RevokeDriverDeviceDocument = graphql(`
  mutation RevokeDriverDevice($driverDeviceRegistrationId: UUID!, $revokedBy: String!) {
    revokeDriverDevice(
      command: { driverDeviceRegistrationId: $driverDeviceRegistrationId, revokedBy: $revokedBy }
    )
  }
`);
