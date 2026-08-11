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
import { fetchAllPages } from 'api/core/paging';
import type { ListParams, Page } from 'api/core/paging';
import type {
  DeviceItemFragment as DeviceItemType,
  SynchronizedDeviceFragment as SynchronizedDeviceType,
  GetDeviceLookupQuery,
  DetectedStatus,
} from './generated/graphql';
import {
  GetDevicesByAccountDocument,
  GetDeviceLookupDocument,
  DeleteDeviceDocument,
  GetSynchronizedDevicesDocument,
  GetUnassignedSynchronizedDevicesDocument,
  SetSynchronizedDeviceIgnoredDocument,
  RegisterManualDeviceDocument,
} from './deviceOperations';

export type Device = DeviceItemType;
export type DevicesPage = Page<Device>;
export type DeviceLookup = GetDeviceLookupQuery['deviceLookup'][number];
export type SynchronizedDevice = SynchronizedDeviceType;
export type SynchronizedDevicesPage = Page<SynchronizedDevice>;

/** Server-side filters accepted by {@link getSynchronizedDevices}. */
export interface SynchronizedDeviceFilters extends ListParams {
  detectedStatus?: DetectedStatus | null;
  operatorId?: string | null;
  /** Every device except those with an active assignment (wider than the status filter). */
  unassignedOnly?: boolean | null;
  /** Only devices first seen within the server's recent window (24h). */
  recentOnly?: boolean | null;
}

export async function getDevicesByAccount(params: ListParams = {}): Promise<DevicesPage> {
  const data = await executeGraphQL('manager', GetDevicesByAccountDocument, {
    skip: params.skip ?? null,
    take: params.take ?? null,
    search: params.search ?? null,
  });
  return data.devicesByAccount;
}

/**
 * The account's devices as id + display name + owning operator. Unpaged by
 * design (the server raises past its own ceiling rather than truncating), so
 * callers binding a picker, building a deviceId→name map, or joining
 * operator→device→transporter get the whole set or a loud failure.
 */
export async function getDeviceLookup(): Promise<DeviceLookup[]> {
  const data = await executeGraphQL('manager', GetDeviceLookupDocument);
  return data.deviceLookup;
}

/** Returns the id of the deleted device (schema: `deleteDevice: UUID!`). */
export async function deleteDevice(deviceId: string): Promise<string> {
  const data = await executeGraphQL('manager', DeleteDeviceDocument, { deviceId });
  return data.deleteDevice;
}

export async function getSynchronizedDevices(
  accountId: string,
  filters: SynchronizedDeviceFilters = {}
): Promise<SynchronizedDevicesPage> {
  const data = await executeGraphQL('manager', GetSynchronizedDevicesDocument, {
    accountId,
    detectedStatus: filters.detectedStatus ?? null,
    operatorId: filters.operatorId ?? null,
    skip: filters.skip ?? null,
    take: filters.take ?? null,
    search: filters.search ?? null,
    unassignedOnly: filters.unassignedOnly ?? null,
    recentOnly: filters.recentOnly ?? null,
  });
  return data.synchronizedDevices;
}

export async function getUnassignedSynchronizedDevices(
  accountId: string,
  params: ListParams = {}
): Promise<SynchronizedDevicesPage> {
  const data = await executeGraphQL('manager', GetUnassignedSynchronizedDevicesDocument, {
    accountId,
    skip: params.skip ?? null,
    take: params.take ?? null,
    search: params.search ?? null,
  });
  return data.unassignedSynchronizedDevices;
}

/**
 * Every unassigned provider device, all server pages drained. There is no
 * lookup for the unassigned subset and the assign form's device picker must
 * offer all of them — a truncated picker hides assignable devices.
 */
export async function getAllUnassignedSynchronizedDevices(
  accountId: string
): Promise<SynchronizedDevice[]> {
  return fetchAllPages(
    async (skip, take) => (await getUnassignedSynchronizedDevices(accountId, { skip, take })).items
  );
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

/** Fields the operator supplies when registering a device by hand. */
export interface ManualDeviceInput {
  accountId: string;
  operatorId: string;
  /** For providers queried by plate (Prosegur/Rastrack) this must be the license plate. */
  name: string;
  serial: string;
  deviceTypeId: number;
  /** 0 (default) lets the server allocate the next free identifier for the operator. */
  identifier?: number;
  description?: string | null;
}

/**
 * Manual registration for providers without a device-catalog API (Prosegur).
 * With autoAssign (default) the server also creates/adopts a transporter named
 * after the device and assigns it, so positions flow without further setup.
 */
export async function registerManualDevice(
  input: ManualDeviceInput,
  autoAssign: boolean = true
): Promise<SynchronizedDevice> {
  const data = await executeGraphQL('manager', RegisterManualDeviceDocument, {
    device: {
      accountId: input.accountId,
      operatorId: input.operatorId,
      name: input.name,
      serial: input.serial,
      deviceTypeId: input.deviceTypeId,
      identifier: input.identifier ?? 0,
      description: input.description ?? null,
      providerDisplayName: null,
      providerMetadataHash: null,
      providerStatus: null,
    },
    autoAssign,
  });
  return data.registerManualDevice;
}
