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
 * Device GraphQL documents (Manager backend). Codegen validates these against
 * schemas/manager.graphql and emits typed document nodes — values always travel
 * as variables, never string interpolation.
 */

import { graphql } from './generated';

/** Device record for the account device list. */
export const DeviceItemFragment = graphql(`
  fragment DeviceItem on DeviceVm {
    deviceId
    name
    serial
    description
    deviceType
    deviceTypeId
    identifier
    operatorId
  }
`);

/** Synchronized (provider-detected) device record for the GPS-integration screens. */
export const SynchronizedDeviceFragment = graphql(`
  fragment SynchronizedDevice on DeviceVm {
    deviceId
    accountId
    operatorId
    serial
    name
    identifier
    providerDisplayName
    providerStatus
    detectedStatus
    firstSeenAt
    lastSeenAt
    lastSyncedAt
    lastAssignedAt
    ignoredAt
  }
`);

export const GetDevicesByAccountDocument = graphql(`
  query GetDevicesByAccount {
    devicesByAccount {
      ...DeviceItem
    }
  }
`);

export const DeleteDeviceDocument = graphql(`
  mutation DeleteDevice($deviceId: UUID!) {
    deleteDevice(deviceId: $deviceId)
  }
`);

export const GetSynchronizedDevicesDocument = graphql(`
  query GetSynchronizedDevices($accountId: UUID!, $detectedStatus: DetectedStatus, $operatorId: UUID) {
    synchronizedDevices(
      query: { accountId: $accountId, detectedStatus: $detectedStatus, operatorId: $operatorId }
    ) {
      ...SynchronizedDevice
    }
  }
`);

export const GetUnassignedSynchronizedDevicesDocument = graphql(`
  query GetUnassignedSynchronizedDevices($accountId: UUID!) {
    unassignedSynchronizedDevices(query: { accountId: $accountId }) {
      ...SynchronizedDevice
    }
  }
`);

export const SetSynchronizedDeviceIgnoredDocument = graphql(`
  mutation SetSynchronizedDeviceIgnored($deviceId: UUID!, $ignored: Boolean!) {
    setSynchronizedDeviceIgnored(command: { deviceId: $deviceId, ignored: $ignored })
  }
`);
