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
 * Driver identity API (Security backend): credentials and device registrations
 * as plain typed async functions. Failures THROW ApiError — fallbacks and
 * toasts belong to the caller layer (src/queries).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import { fetchAllPages } from 'api/core/paging';
import type {
  DriverCredentialItemFragment as DriverCredentialItemType,
  DriverDeviceItemFragment as DriverDeviceItemType,
  DriverCredentialDtoInput,
} from './generated/graphql';
import {
  GetDriverCredentialsDocument,
  GetDriverDevicesDocument,
  CreateDriverCredentialDocument,
  ActivateDriverCredentialDocument,
  LockDriverCredentialDocument,
  ResetDriverCredentialDocument,
  RevokeDriverCredentialDocument,
  RevokeDriverDeviceDocument,
} from './driverIdentityOperations';

export type DriverCredential = DriverCredentialItemType;
export type DriverDevice = DriverDeviceItemType;
export type { DriverCredentialDtoInput };

export async function getDriverCredentials(
  accountId: string,
  driverId: string | null = null,
  skip = 0,
  take = 100
): Promise<DriverCredential[]> {
  const data = await executeGraphQL('security', GetDriverCredentialsDocument, {
    accountId,
    driverId,
    skip,
    take,
  });
  return data.driverCredentials;
}

export async function getDriverDevices(
  accountId: string,
  driverId: string | null = null,
  skip = 0,
  take = 100
): Promise<DriverDevice[]> {
  const data = await executeGraphQL('security', GetDriverDevicesDocument, {
    accountId,
    driverId,
    skip,
    take,
  });
  return data.driverDevices;
}

/** Every credential matching the filter, paged to exhaustion (server clamps take to 1..500). */
export async function getAllDriverCredentials(
  accountId: string,
  driverId: string | null = null
): Promise<DriverCredential[]> {
  return fetchAllPages((skip, take) => getDriverCredentials(accountId, driverId, skip, take));
}

/** Every device registration matching the filter, paged to exhaustion. */
export async function getAllDriverDevices(
  accountId: string,
  driverId: string | null = null
): Promise<DriverDevice[]> {
  return fetchAllPages((skip, take) => getDriverDevices(accountId, driverId, skip, take));
}

export async function createDriverCredential(
  credential: DriverCredentialDtoInput
): Promise<DriverCredential> {
  const data = await executeGraphQL('security', CreateDriverCredentialDocument, {
    credential: {
      driverId: credential.driverId,
      accountId: credential.accountId,
      login: credential.login,
      password: credential.password,
      active: credential.active,
      resetRequired: credential.resetRequired,
    },
  });
  return data.createDriverCredential;
}

export async function activateDriverCredential(
  driverCredentialId: string,
  password: string
): Promise<boolean> {
  const data = await executeGraphQL('security', ActivateDriverCredentialDocument, {
    driverCredentialId,
    password,
  });
  return data.activateDriverCredential;
}

export async function lockDriverCredential(
  driverCredentialId: string,
  lockedUntil: string
): Promise<boolean> {
  const data = await executeGraphQL('security', LockDriverCredentialDocument, {
    driverCredentialId,
    lockedUntil,
  });
  return data.lockDriverCredential;
}

export async function resetDriverCredential(
  driverCredentialId: string,
  password: string,
  resetRequired: boolean
): Promise<boolean> {
  const data = await executeGraphQL('security', ResetDriverCredentialDocument, {
    driverCredentialId,
    password,
    resetRequired,
  });
  return data.resetDriverCredential;
}

export async function revokeDriverCredential(driverCredentialId: string): Promise<boolean> {
  const data = await executeGraphQL('security', RevokeDriverCredentialDocument, {
    driverCredentialId,
  });
  return data.revokeDriverCredential;
}

export async function revokeDriverDevice(
  driverDeviceRegistrationId: string,
  revokedBy: string
): Promise<boolean> {
  const data = await executeGraphQL('security', RevokeDriverDeviceDocument, {
    driverDeviceRegistrationId,
    revokedBy,
  });
  return data.revokeDriverDevice;
}
