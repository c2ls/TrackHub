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
 * Driver API (Manager backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer (src/queries).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  DriverItemFragment as DriverItemType,
  DriverDtoInput,
} from './generated/graphql';
import {
  GetDriversByAccountDocument,
  CreateDriverDocument,
  UpdateDriverDocument,
  DeactivateDriverDocument,
} from './driverOperations';

export type Driver = DriverItemType;
export type { DriverDtoInput };

/** Builds a clean DriverDtoInput, dropping any extra fields carried by the form. */
function toDriverDto(driver: DriverDtoInput): DriverDtoInput {
  return {
    accountId: driver.accountId,
    name: driver.name,
    phone: driver.phone ?? null,
    documentType: driver.documentType ?? null,
    documentNumber: driver.documentNumber ?? null,
    active: driver.active !== false,
    employeeCode: driver.employeeCode ?? null,
    licenseNumber: driver.licenseNumber ?? null,
    licenseExpiresAt: driver.licenseExpiresAt ?? null,
    defaultTransporterId: driver.defaultTransporterId ?? null,
  };
}

export async function getDriversByAccount(
  accountId: string,
  skip = 0,
  take = 50
): Promise<Driver[]> {
  const data = await executeGraphQL('manager', GetDriversByAccountDocument, {
    accountId,
    skip,
    take,
  });
  return data.driversByAccount;
}

export async function createDriver(driver: DriverDtoInput): Promise<Driver> {
  const data = await executeGraphQL('manager', CreateDriverDocument, { driver: toDriverDto(driver) });
  return data.createDriver;
}

export async function updateDriver(driverId: string, driver: DriverDtoInput): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateDriverDocument, {
    driverId,
    driver: toDriverDto(driver),
  });
  return data.updateDriver;
}

export async function deactivateDriver(driverId: string): Promise<boolean> {
  const data = await executeGraphQL('manager', DeactivateDriverDocument, { driverId });
  return data.deactivateDriver;
}
