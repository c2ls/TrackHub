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
 * Staff identity API (Security backend): credentials and device registrations
 * as plain typed async functions. Failures THROW ApiError — fallbacks and
 * toasts belong to the caller layer (src/queries).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type { RequestOptions } from 'api/core/errors';
import { fetchAllPages } from 'api/core/paging';
import type {
  StaffCredentialItemFragment as StaffCredentialItemType,
  StaffDeviceItemFragment as StaffDeviceItemType,
  StaffCredentialDtoInput,
} from './generated/graphql';
import {
  GetStaffCredentialsDocument,
  GetStaffDevicesDocument,
  CreateStaffCredentialDocument,
  ActivateStaffCredentialDocument,
  LockStaffCredentialDocument,
  ResetStaffCredentialDocument,
  RevokeStaffCredentialDocument,
  RevokeStaffDeviceDocument,
} from './staffIdentityOperations';

export type StaffCredential = StaffCredentialItemType;
export type StaffDevice = StaffDeviceItemType;
export type { StaffCredentialDtoInput };

export async function getStaffCredentials(
  accountId: string,
  staffId: string | null = null,
  skip = 0,
  take = 100,
  options?: RequestOptions
): Promise<StaffCredential[]> {
  const data = await executeGraphQL('security', GetStaffCredentialsDocument, {
    accountId,
    staffId,
    skip,
    take,
  }, options);
  return data.staffCredentials;
}

export async function getStaffDevices(
  accountId: string,
  staffId: string | null = null,
  skip = 0,
  take = 100,
  options?: RequestOptions
): Promise<StaffDevice[]> {
  const data = await executeGraphQL('security', GetStaffDevicesDocument, {
    accountId,
    staffId,
    skip,
    take,
  }, options);
  return data.staffDevices;
}

/** Every credential matching the filter, paged to exhaustion (server clamps take to 1..500). */
export async function getAllStaffCredentials(
  accountId: string,
  staffId: string | null = null,
  options?: RequestOptions
): Promise<StaffCredential[]> {
  return fetchAllPages((skip, take) => getStaffCredentials(accountId, staffId, skip, take, options));
}

/** Every device registration matching the filter, paged to exhaustion. */
export async function getAllStaffDevices(
  accountId: string,
  staffId: string | null = null,
  options?: RequestOptions
): Promise<StaffDevice[]> {
  return fetchAllPages((skip, take) => getStaffDevices(accountId, staffId, skip, take, options));
}

export async function createStaffCredential(
  credential: StaffCredentialDtoInput
): Promise<StaffCredential> {
  const data = await executeGraphQL('security', CreateStaffCredentialDocument, {
    credential: {
      staffId: credential.staffId,
      accountId: credential.accountId,
      login: credential.login,
      password: credential.password,
      active: credential.active,
      resetRequired: credential.resetRequired,
    },
  });
  return data.createStaffCredential;
}

export async function activateStaffCredential(
  staffCredentialId: string,
  password: string
): Promise<boolean> {
  const data = await executeGraphQL('security', ActivateStaffCredentialDocument, {
    staffCredentialId,
    password,
  });
  return data.activateStaffCredential;
}

export async function lockStaffCredential(
  staffCredentialId: string,
  lockedUntil: string
): Promise<boolean> {
  const data = await executeGraphQL('security', LockStaffCredentialDocument, {
    staffCredentialId,
    lockedUntil,
  });
  return data.lockStaffCredential;
}

export async function resetStaffCredential(
  staffCredentialId: string,
  password: string,
  resetRequired: boolean
): Promise<boolean> {
  const data = await executeGraphQL('security', ResetStaffCredentialDocument, {
    staffCredentialId,
    password,
    resetRequired,
  });
  return data.resetStaffCredential;
}

export async function revokeStaffCredential(staffCredentialId: string): Promise<boolean> {
  const data = await executeGraphQL('security', RevokeStaffCredentialDocument, {
    staffCredentialId,
  });
  return data.revokeStaffCredential;
}

export async function revokeStaffDevice(
  staffDeviceRegistrationId: string,
  revokedBy: string
): Promise<boolean> {
  const data = await executeGraphQL('security', RevokeStaffDeviceDocument, {
    staffDeviceRegistrationId,
    revokedBy,
  });
  return data.revokeStaffDevice;
}
