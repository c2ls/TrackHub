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
 * Operator API (Manager backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer (src/queries).
 * Operator health / sync-run reads are a separate Telemetry module
 * (api/telemetry/operatorHealth).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import { fetchAllPages } from 'api/core/paging';
import type { ListParams, Page } from 'api/core/paging';
import type {
  OperatorDetailFragment as OperatorDetailType,
  OperatorGpsFragment as OperatorGpsType,
  GetOperatorLookupQuery,
  OperatorDtoInput,
  UpdateOperatorDtoInput,
  TriggerOperatorDeviceSyncCommandInput,
} from './generated/graphql';
import {
  GetOperatorsByCurrentAccountDocument,
  GetOperatorLookupDocument,
  GetGpsOperatorsDocument,
  CreateOperatorDocument,
  UpdateOperatorDocument,
  DeleteOperatorDocument,
  SetOperatorEnabledDocument,
  TriggerOperatorDeviceSyncDocument,
} from './operatorOperations';

export type Operator = OperatorDetailType;
export type OperatorsPage = Page<Operator>;
export type OperatorLookup = GetOperatorLookupQuery['operatorLookup'][number];
export type GpsOperator = OperatorGpsType;
export type GpsOperatorsPage = Page<GpsOperator>;
export type { OperatorDtoInput, UpdateOperatorDtoInput, TriggerOperatorDeviceSyncCommandInput };

const DEFAULT_SYNC_INTERVAL_MINUTES = 30;

/** Legacy `formatPositiveInteger`: keep a positive int, otherwise the fallback. */
function positiveIntOr(value: unknown, fallback: number): number {
  const parsed = Number.parseInt(value as string, 10);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback;
}

export async function getOperatorsByCurrentAccount(
  params: ListParams = {}
): Promise<OperatorsPage> {
  const data = await executeGraphQL('manager', GetOperatorsByCurrentAccountDocument, {
    skip: params.skip ?? null,
    take: params.take ?? null,
    search: params.search ?? null,
  });
  return data.operatorsByCurrentAccount;
}

/**
 * Minimal id + name list for the dashboard and report operator filters. Unpaged
 * by design — the server raises past its own ceiling rather than truncating.
 */
export async function getOperatorLookup(): Promise<OperatorLookup[]> {
  const data = await executeGraphQL('manager', GetOperatorLookupDocument);
  return data.operatorLookup;
}

/** Operator page (name map + sync metadata) for the GPS-integration screens. */
export async function getGpsOperators(params: ListParams = {}): Promise<GpsOperatorsPage> {
  const data = await executeGraphQL('manager', GetGpsOperatorsDocument, {
    skip: params.skip ?? null,
    take: params.take ?? null,
    search: params.search ?? null,
  });
  return data.operatorsByCurrentAccount;
}

/**
 * Every operator with its sync metadata, all server pages drained. The GPS
 * screens filter and label rows by operator, and the lookup carries only
 * id + name, so the full record set is genuinely required here.
 */
export async function getAllGpsOperators(): Promise<GpsOperator[]> {
  return fetchAllPages(async (skip, take) => (await getGpsOperators({ skip, take })).items);
}

export async function createOperator(operator: OperatorDtoInput): Promise<Operator> {
  const input: OperatorDtoInput = {
    name: operator.name,
    description: operator.description ?? null,
    phoneNumber: operator.phoneNumber ?? null,
    emailAddress: operator.emailAddress ?? null,
    address: operator.address ?? null,
    contactName: operator.contactName ?? null,
    protocolTypeId: operator.protocolTypeId,
    syncIntervalMinutes: positiveIntOr(operator.syncIntervalMinutes, DEFAULT_SYNC_INTERVAL_MINUTES),
  };
  const data = await executeGraphQL('manager', CreateOperatorDocument, { operator: input });
  return data.createOperator;
}

export async function updateOperator(
  operatorId: string,
  operator: Omit<UpdateOperatorDtoInput, 'operatorId'>
): Promise<boolean> {
  const input: UpdateOperatorDtoInput = {
    operatorId,
    name: operator.name,
    description: operator.description ?? null,
    phoneNumber: operator.phoneNumber ?? null,
    emailAddress: operator.emailAddress ?? null,
    address: operator.address ?? null,
    contactName: operator.contactName ?? null,
    protocolTypeId: operator.protocolTypeId,
    syncIntervalMinutes: positiveIntOr(operator.syncIntervalMinutes, DEFAULT_SYNC_INTERVAL_MINUTES),
  };
  const data = await executeGraphQL('manager', UpdateOperatorDocument, { id: operatorId, operator: input });
  return data.updateOperator;
}

/** Returns the id of the deleted operator (schema: `deleteOperator: UUID!`). */
export async function deleteOperator(operatorId: string): Promise<string> {
  const data = await executeGraphQL('manager', DeleteOperatorDocument, { id: operatorId });
  return data.deleteOperator;
}

export async function setOperatorEnabled(operatorId: string, enabled: boolean): Promise<boolean> {
  const data = await executeGraphQL('manager', SetOperatorEnabledDocument, { operatorId, enabled });
  return data.setOperatorEnabled;
}

export async function triggerOperatorDeviceSync(
  operatorId: string,
  resetDeviceCatalog = false,
  autoAssignNewDevices = true
): Promise<boolean> {
  const command: TriggerOperatorDeviceSyncCommandInput = {
    operatorId,
    resetDeviceCatalog: !!resetDeviceCatalog,
    autoAssignNewDevices: !!autoAssignNewDevices,
  };
  const data = await executeGraphQL('manager', TriggerOperatorDeviceSyncDocument, { command });
  return data.triggerOperatorDeviceSync;
}
