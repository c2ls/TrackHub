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
 * Transporter API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — fallbacks and toasts belong to the caller layer
 * (src/queries handles both for components).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import { fetchAllPages } from 'api/core/paging';
import type { ListParams, Page } from 'api/core/paging';
import type {
  TransporterItemFragment as TransporterItemType,
  AssignmentFieldsFragment as AssignmentFieldsType,
  TransporterDtoInput,
  UpdateTransporterDtoInput,
  TransporterDeviceAssignmentDtoInput,
  GetTransporterDeviceAssignmentsByAccountQuery,
  GetTransporterLookupByAccountQuery,
} from './generated/graphql';
import {
  GetTransportersByAccountDocument,
  GetTransportersByGroupDocument,
  GetTransporterLookupByAccountDocument,
  GetTransporterLookupByUserDocument,
  CreateTransporterDocument,
  UpdateTransporterDocument,
  DeleteTransporterDocument,
  GetTransporterDeviceAssignmentsByAccountDocument,
  AssignDeviceToTransporterDocument,
  EndDeviceTransporterAssignmentDocument,
} from './transporterOperations';

export type Transporter = TransporterItemType;
export type TransportersPage = Page<Transporter>;
export type TransporterLookup =
  GetTransporterLookupByAccountQuery['transporterLookupByAccount'][number];
export type TransporterAssignment = AssignmentFieldsType;
export type TransporterAssignmentWithAudit =
  GetTransporterDeviceAssignmentsByAccountQuery['transporterDeviceAssignmentsByAccount']['items'][number];
export type TransporterAssignmentsPage = Page<TransporterAssignmentWithAudit>;
export type { TransporterDtoInput, UpdateTransporterDtoInput, TransporterDeviceAssignmentDtoInput };

/** Paging plus the assignment-specific `activeOnly` toggle (no server-side search). */
export interface TransporterAssignmentFilters extends Omit<ListParams, 'search'> {
  activeOnly?: boolean;
}

export async function getTransportersByAccount(
  params: ListParams = {}
): Promise<TransportersPage> {
  const data = await executeGraphQL('manager', GetTransportersByAccountDocument, {
    skip: params.skip ?? null,
    take: params.take ?? null,
    search: params.search ?? null,
  });
  return data.transportersByAccount;
}

export async function getTransportersByGroup(
  groupId: number,
  params: ListParams = {}
): Promise<TransportersPage> {
  const data = await executeGraphQL('manager', GetTransportersByGroupDocument, {
    groupId,
    skip: params.skip ?? null,
    take: params.take ?? null,
    search: params.search ?? null,
  });
  return data.transportersByGroup;
}

/**
 * Every transporter on the account as id + name. The admin-side picker source:
 * unpaged by design, so an allocator dialog's "available" operand is never a
 * truncated list. Distinct from {@link getTransporterLookupByUser} on purpose.
 */
export async function getTransporterLookupByAccount(): Promise<TransporterLookup[]> {
  const data = await executeGraphQL('manager', GetTransporterLookupByAccountDocument);
  return data.transporterLookupByAccount;
}

/** The transporters the signed-in user may track, as id + name. Unpaged by design. */
export async function getTransporterLookupByUser(): Promise<TransporterLookup[]> {
  const data = await executeGraphQL('manager', GetTransporterLookupByUserDocument);
  return data.transporterLookupByUser;
}

/**
 * Every transporter in a group, all server pages drained. There is no per-group
 * transporter lookup, and both the allocator dialog's set difference and the
 * dashboard's group filter need the complete membership.
 */
export async function getAllTransportersByGroup(groupId: number): Promise<Transporter[]> {
  return fetchAllPages(
    async (skip, take) => (await getTransportersByGroup(groupId, { skip, take })).items
  );
}

export async function createTransporter(transporter: TransporterDtoInput): Promise<Transporter> {
  const data = await executeGraphQL('manager', CreateTransporterDocument, { transporter });
  return data.createTransporter;
}

export async function updateTransporter(
  transporterId: string,
  transporter: Omit<UpdateTransporterDtoInput, 'transporterId'>
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateTransporterDocument, {
    id: transporterId,
    transporter: { ...transporter, transporterId },
  });
  return data.updateTransporter;
}

export async function deleteTransporter(transporterId: string): Promise<string> {
  const data = await executeGraphQL('manager', DeleteTransporterDocument, { id: transporterId });
  return data.deleteTransporter;
}

export async function getTransporterDeviceAssignmentsByAccount(
  accountId: string,
  filters: TransporterAssignmentFilters = {}
): Promise<TransporterAssignmentsPage> {
  const data = await executeGraphQL('manager', GetTransporterDeviceAssignmentsByAccountDocument, {
    accountId,
    activeOnly: filters.activeOnly ?? false,
    skip: filters.skip ?? null,
    take: filters.take ?? null,
  });
  return data.transporterDeviceAssignmentsByAccount;
}

/**
 * Every assignment on the account, all server pages drained. The dashboard's
 * operator filter joins device→transporter across the whole active set, so a
 * single page would drop units out of the filtered map.
 */
export async function getAllTransporterDeviceAssignmentsByAccount(
  accountId: string,
  activeOnly = false
): Promise<TransporterAssignmentWithAudit[]> {
  return fetchAllPages(
    async (skip, take) =>
      (await getTransporterDeviceAssignmentsByAccount(accountId, { activeOnly, skip, take })).items
  );
}

export async function assignDeviceToTransporter(
  assignment: TransporterDeviceAssignmentDtoInput
): Promise<{ transporterDeviceAssignmentId: string }> {
  const data = await executeGraphQL('manager', AssignDeviceToTransporterDocument, { assignment });
  return data.assignDeviceToTransporter;
}

export async function endDeviceTransporterAssignment(
  assignmentId: string,
  reason: string | null = null
): Promise<boolean> {
  const data = await executeGraphQL('manager', EndDeviceTransporterAssignmentDocument, {
    assignmentId,
    reason,
  });
  return data.endDeviceTransporterAssignment;
}
