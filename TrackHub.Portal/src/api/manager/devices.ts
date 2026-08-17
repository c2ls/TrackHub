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
 * Device API (Manager backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer (src/queries for
 * components; the GPS-integration screens call these imperatively).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  DeviceItemFragment as DeviceItemType,
  SynchronizedDeviceFragment as SynchronizedDeviceType,
  DetectedStatus,
} from './generated/graphql';
import {
  GetDevicesByAccountDocument,
  DeleteDeviceDocument,
  GetSynchronizedDevicesDocument,
  GetUnassignedSynchronizedDevicesDocument,
  SetSynchronizedDeviceIgnoredDocument,
} from './deviceOperations';

export type Device = DeviceItemType;
export type SynchronizedDevice = SynchronizedDeviceType;

export async function getDevicesByAccount(): Promise<Device[]> {
  const data = await executeGraphQL('manager', GetDevicesByAccountDocument);
  return data.devicesByAccount;
}

/** Returns the id of the deleted device (schema: `deleteDevice: UUID!`). */
export async function deleteDevice(deviceId: string): Promise<string> {
  const data = await executeGraphQL('manager', DeleteDeviceDocument, { deviceId });
  return data.deleteDevice;
}

export async function getSynchronizedDevices(
  accountId: string,
  detectedStatus: DetectedStatus | null = null,
  operatorId: string | null = null
): Promise<SynchronizedDevice[]> {
  const data = await executeGraphQL('manager', GetSynchronizedDevicesDocument, {
    accountId,
    detectedStatus,
    operatorId,
  });
  return data.synchronizedDevices;
}

export async function getUnassignedSynchronizedDevices(
  accountId: string
): Promise<SynchronizedDevice[]> {
  const data = await executeGraphQL('manager', GetUnassignedSynchronizedDevicesDocument, {
    accountId,
  });
  return data.unassignedSynchronizedDevices;
}

export async function setSynchronizedDeviceIgnored(
  deviceId: string,
  ignored: boolean
): Promise<boolean> {
  const data = await executeGraphQL('manager', SetSynchronizedDeviceIgnoredDocument, {
    deviceId,
    ignored: !!ignored,
  });
  return data.setSynchronizedDeviceIgnored;
}
