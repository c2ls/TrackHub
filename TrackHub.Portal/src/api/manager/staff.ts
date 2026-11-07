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
 * Staff API (Manager backend): plain typed async functions. Failures THROW
 * ApiError — fallbacks and toasts belong to the caller layer (src/queries).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type { RequestOptions } from 'api/core/errors';
import { fetchAllPages } from 'api/core/paging';
import type { ListParams, Page } from 'api/core/paging';
import type {
  StaffItemFragment as StaffItemType,
  StaffQualificationItemFragment as StaffQualificationItemType,
  StaffTransporterAssignmentItemFragment as StaffAssignmentItemType,
  StaffActiveAssignmentItemFragment as StaffActiveAssignmentItemType,
  StaffMemberDtoInput,
  StaffQualificationDtoInput,
  GetStaffLookupQuery,
} from './generated/graphql';
import {
  GetStaffMembersByAccountDocument,
  GetStaffLookupDocument,
  GetStaffOptionsDocument,
  CreateStaffMemberDocument,
  UpdateStaffMemberDocument,
  DeactivateStaffMemberDocument,
  GetStaffAssignmentsDocument,
  GetStaffQualificationsDocument,
  CreateStaffQualificationDocument,
  UpdateStaffQualificationDocument,
  DeleteStaffQualificationDocument,
  GetStaffAssignmentHistoryDocument,
  AssignStaffToTransporterDocument,
  EndStaffAssignmentDocument,
} from './staffOperations';

export type Staff = StaffItemType;
/** Picker projection (id, name, active) served under Trips/Read: no phone, document or licence. */
export type StaffLookup = GetStaffLookupQuery['staffLookup'][number];
export type StaffQualification = StaffQualificationItemType;
export type StaffTransporterAssignment = StaffAssignmentItemType;
/** Time-aware active assignment projection (spec 09 §7.2); no assignment row id. */
export type StaffActiveAssignment = StaffActiveAssignmentItemType;
export type { StaffMemberDtoInput, StaffQualificationDtoInput };

/** Filters accepted by the account-wide assignment-history query. */
export interface StaffAssignmentHistoryFilters {
  staffId?: string | null;
  transporterId?: string | null;
  from?: string | null;
  to?: string | null;
}

/** Builds a clean StaffMemberDtoInput, dropping any extra fields carried by the form. */
function toStaffMemberDto(staff: StaffMemberDtoInput): StaffMemberDtoInput {
  return {
    accountId: staff.accountId,
    name: staff.name,
    phone: staff.phone ?? null,
    documentType: staff.documentType ?? null,
    documentNumber: staff.documentNumber ?? null,
    active: staff.active !== false,
    employeeCode: staff.employeeCode ?? null,
    licenseNumber: staff.licenseNumber ?? null,
    licenseExpiresAt: staff.licenseExpiresAt ?? null,
    defaultTransporterId: staff.defaultTransporterId || null,
    staffType: staff.staffType,
    expectedVersion: staff.expectedVersion ?? null,
  };
}

/** One page of account staff. Callers that must see EVERY staff use {@link getAllStaffMembersByAccount}. */
export async function getStaffMembersByAccount(
  accountId: string,
  skip = 0,
  take = 50,
  search: string | null = null,
  staffType: string | null = null,
  options?: RequestOptions
): Promise<Page<Staff>> {
  const data = await executeGraphQL('manager', GetStaffMembersByAccountDocument, {
    accountId,
    search,
    staffType,
    skip,
    take,
  }, options);
  return data.staffMembersByAccount;
}

/**
 * Every staff on the account, paged to exhaustion. The staff pickers on every
 * workforce surface read from this: a single clamped page would make staff
 * past the page boundary unadministrable (no credential, no qualification, no
 * assignment could ever be created for them) with no error shown.
 */
export async function getAllStaffMembersByAccount(accountId: string, options?: RequestOptions): Promise<Staff[]> {
  return fetchAllPages(async (skip, take) => (await getStaffMembersByAccount(accountId, skip, take, null, null, options)).items);
}

export async function createStaffMember(staff: StaffMemberDtoInput): Promise<Staff> {
  const data = await executeGraphQL('manager', CreateStaffMemberDocument, { staff: toStaffMemberDto(staff) });
  return data.createStaffMember;
}

export async function updateStaffMember(staffId: string, staff: StaffMemberDtoInput): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateStaffMemberDocument, {
    staffId,
    staff: toStaffMemberDto(staff),
  });
  return data.updateStaffMember;
}

export async function deactivateStaffMember(staffId: string): Promise<boolean> {
  const data = await executeGraphQL('manager', DeactivateStaffMemberDocument, { staffId });
  return data.deactivateStaffMember;
}

/** Builds a clean StaffQualificationDtoInput, dropping form-only fields. */
function toQualificationDto(q: StaffQualificationDtoInput): StaffQualificationDtoInput {
  return {
    accountId: q.accountId,
    staffId: q.staffId,
    qualificationType: q.qualificationType,
    category: q.category || null,
    number: q.number || null,
    issuedAt: q.issuedAt || null,
    expiresAt: q.expiresAt || null,
    issuingAuthority: q.issuingAuthority || null,
    status: q.status,
    documentId: q.documentId || null,
    notes: q.notes || null,
  };
}

export async function getStaffQualifications(
  accountId: string,
  staffId: string | null = null,
  expiringWithinDays: number | null = null,
  skip = 0,
  take = 100,
  options?: RequestOptions
): Promise<Page<StaffQualification>> {
  const data = await executeGraphQL('manager', GetStaffQualificationsDocument, {
    accountId,
    staffId,
    expiringWithinDays,
    skip,
    take,
  }, options);
  return data.staffQualifications;
}

/** Every qualification matching the filter, paged to exhaustion. */
export async function getAllStaffQualifications(
  accountId: string,
  staffId: string | null = null,
  expiringWithinDays: number | null = null,
  options?: RequestOptions
): Promise<StaffQualification[]> {
  return fetchAllPages(async (skip, take) =>
    (await getStaffQualifications(accountId, staffId, expiringWithinDays, skip, take, options)).items
  );
}

export async function createStaffQualification(
  qualification: StaffQualificationDtoInput
): Promise<StaffQualification> {
  const data = await executeGraphQL('manager', CreateStaffQualificationDocument, {
    qualification: toQualificationDto(qualification),
  });
  return data.createStaffQualification;
}

export async function updateStaffQualification(
  staffQualificationId: string,
  qualification: StaffQualificationDtoInput
): Promise<boolean> {
  const data = await executeGraphQL('manager', UpdateStaffQualificationDocument, {
    staffQualificationId,
    qualification: toQualificationDto(qualification),
  });
  return data.updateStaffQualification;
}

export async function deleteStaffQualification(staffQualificationId: string): Promise<string> {
  const data = await executeGraphQL('manager', DeleteStaffQualificationDocument, {
    staffQualificationId,
  });
  return data.deleteStaffQualification;
}

export async function getStaffAssignmentHistory(
  accountId: string,
  filters: StaffAssignmentHistoryFilters = {},
  skip = 0,
  take = 100,
  options?: RequestOptions
): Promise<Page<StaffTransporterAssignment>> {
  const data = await executeGraphQL('manager', GetStaffAssignmentHistoryDocument, {
    accountId,
    staffId: filters.staffId || null,
    transporterId: filters.transporterId || null,
    from: filters.from || null,
    to: filters.to || null,
    skip,
    take,
  }, options);
  return data.staffAssignmentHistory;
}

/**
 * Every history row matching the filter, paged to exhaustion (capped by
 * `MAX_FETCH_ALL_ITEMS` — the caller compares against it and asks the user to
 * narrow the filters rather than truncating silently).
 */
export async function getAllStaffAssignmentHistory(
  accountId: string,
  filters: StaffAssignmentHistoryFilters = {},
  options?: RequestOptions
): Promise<StaffTransporterAssignment[]> {
  return fetchAllPages(async (skip, take) =>
    (await getStaffAssignmentHistory(accountId, filters, skip, take, options)).items
  );
}

/**
 * The staff's currently-active assignments (spec 09 §7.2). Unpaged and
 * time-aware server-side, so it is the only correct source for an "active"
 * list — the history query is ordered `StartsAt DESC` and paged, so a long-lived
 * assignment sorts below any page boundary and would disappear.
 *
 * Note the VM carries no assignment row id: `resourceId` is the transporter id,
 * and the last entry may be the synthesized default-transporter one, which has
 * no underlying row at all.
 */
export async function getStaffAssignments(staffId: string, options?: RequestOptions): Promise<StaffActiveAssignment[]> {
  const data = await executeGraphQL('manager', GetStaffAssignmentsDocument, { staffId }, options);
  return data.staffAssignments;
}

export async function assignStaffToTransporter(
  staffId: string,
  transporterId: string,
  startsAt: string,
  assignmentType: string
): Promise<StaffTransporterAssignment> {
  const data = await executeGraphQL('manager', AssignStaffToTransporterDocument, {
    staffId,
    transporterId,
    startsAt,
    assignmentType,
  });
  return data.assignStaffToTransporter;
}

export async function endStaffAssignment(
  staffTransporterAssignmentId: string,
  endsAt: string | null = null
): Promise<boolean> {
  const data = await executeGraphQL('manager', EndStaffAssignmentDocument, {
    staffTransporterAssignmentId,
    endsAt,
  });
  return data.endStaffAssignment;
}

/**
 * Names and ids for pickers and boards. A search offers the account's active staff; a list of
 * ids names exactly those staff, active or not.
 */
export async function getStaffLookup(search?: string | null, staffIds?: string[] | null, options?: RequestOptions): Promise<StaffLookup[]> {
  const data = await executeGraphQL('manager', GetStaffLookupDocument, {
    search: search || null,
    staffIds: staffIds && staffIds.length > 0 ? staffIds : null,
  }, options);
  return data.staffLookup;
}

/** The account's staff, active or not, for the staff-operations pickers (StaffOperations/Read). */
export async function getStaffOptions(params: ListParams = {}, options?: RequestOptions): Promise<Page<StaffLookup>> {
  const data = await executeGraphQL('manager', GetStaffOptionsDocument, {
    search: params.search || null,
    skip: params.skip ?? null,
    take: params.take ?? null,
  }, options);
  return data.staffOptions;
}
