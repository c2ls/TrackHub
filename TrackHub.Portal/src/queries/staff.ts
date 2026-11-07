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
 * Staff query/mutation hooks. Components consume these — not the api layer
 * directly. The account id (required to list/create staff) is still sourced
 * from the account service by the screen.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/staff';
import type { SearchOptionsHook } from 'edition/SearchSelect';
import type {
  StaffMemberDtoInput,
  StaffQualificationDtoInput,
  StaffAssignmentHistoryFilters,
} from 'api/manager/staff';

export const staffKeys = {
  all: ['staff'] as const,
  // 'all-pages' pins the page dimension into the key: the entry holds the
  // exhaustively-paged account list, never a single clamped page.
  byAccount: (accountId: string) => [...staffKeys.all, 'byAccount', accountId, 'all-pages'] as const,
  lookup: (search: string) => [...staffKeys.all, 'lookup', search] as const,
  names: (staffIds: readonly string[]) => [...staffKeys.all, 'names', ...staffIds] as const,
};

export const staffQualificationKeys = {
  all: ['staffQualifications'] as const,
  list: (accountId: string, staffId: string | null, expiringWithinDays: number | null) =>
    [
      ...staffQualificationKeys.all,
      accountId,
      staffId ?? '',
      expiringWithinDays ?? '',
      'all-pages',
    ] as const,
};

export const staffAssignmentKeys = {
  all: ['staffAssignments'] as const,
  history: (accountId: string, filters: StaffAssignmentHistoryFilters) =>
    [
      ...staffAssignmentKeys.all,
      'history',
      accountId,
      filters.staffId ?? '',
      filters.transporterId ?? '',
      filters.from ?? '',
      filters.to ?? '',
      'all-pages',
    ] as const,
  active: (staffId: string) => [...staffAssignmentKeys.all, 'active', staffId] as const,
};

/**
 * Every staff on the account (paged to exhaustion in the api layer). The
 * pickers on every workforce surface depend on completeness — a clamped single
 * page leaves staff past the boundary unadministrable.
 */
export function useStaffMembersByAccount(
  accountId: string | undefined,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: staffKeys.byAccount(accountId ?? ''),
    queryFn: ({ signal }) => api.getAllStaffMembersByAccount(accountId as string, { signal }),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

/** One server page of the account's staff, searched by name, document or employee code. */
export function useStaffMembersPage(
  accountId: string | undefined,
  params: { skip: number; take: number; search: string | null; staffType?: string | null },
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: [...staffKeys.all, 'page', accountId ?? '', params] as const,
    queryFn: ({ signal }) => api.getStaffMembersByAccount(accountId as string, params.skip, params.take, params.search, params.staffType ?? null, { signal }),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

/** One page of staff ids and names for the staff-operations pickers; needs StaffOperations/Read only. */
export function useStaffOptions(params: { skip: number; take: number; search: string | null }) {
  return useQuery({
    queryKey: [...staffKeys.all, 'options', params] as const,
    queryFn: ({ signal }) => api.getStaffOptions(params, { signal }),
  });
}

export function useCreateStaffMember() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (staff: StaffMemberDtoInput) => api.createStaffMember(staff),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffKeys.all }),
  });
}

export function useUpdateStaffMember() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ staffId, staff }: { staffId: string; staff: StaffMemberDtoInput }) =>
      api.updateStaffMember(staffId, staff),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffKeys.all }),
  });
}

export function useDeactivateStaffMember() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (staffId: string) => api.deactivateStaffMember(staffId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffKeys.all }),
  });
}

/**
 * Staff qualifications. `staffId` null lists the whole account (the
 * expirations view combines that with `expiringWithinDays`).
 */
export function useStaffQualifications(
  accountId: string | undefined,
  staffId: string | null = null,
  expiringWithinDays: number | null = null,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: staffQualificationKeys.list(accountId ?? '', staffId, expiringWithinDays),
    queryFn: ({ signal }) =>
      api.getAllStaffQualifications(accountId as string, staffId, expiringWithinDays, { signal }),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

export function useCreateStaffQualification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (qualification: StaffQualificationDtoInput) =>
      api.createStaffQualification(qualification),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: staffQualificationKeys.all }),
  });
}

export function useUpdateStaffQualification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      staffQualificationId,
      qualification,
    }: {
      staffQualificationId: string;
      qualification: StaffQualificationDtoInput;
    }) => api.updateStaffQualification(staffQualificationId, qualification),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: staffQualificationKeys.all }),
  });
}

export function useDeleteStaffQualification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (staffQualificationId: string) =>
      api.deleteStaffQualification(staffQualificationId),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: staffQualificationKeys.all }),
  });
}

export function useStaffAssignmentHistory(
  accountId: string | undefined,
  filters: StaffAssignmentHistoryFilters = {},
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: staffAssignmentKeys.history(accountId ?? '', filters),
    queryFn: ({ signal }) => api.getAllStaffAssignmentHistory(accountId as string, filters, { signal }),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

/** One server page of the account's assignment history for the filters. */
export function useStaffAssignmentHistoryPage(
  accountId: string | undefined,
  filters: StaffAssignmentHistoryFilters,
  page: { skip: number; take: number },
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: [...staffAssignmentKeys.history(accountId ?? '', filters), page.skip, page.take],
    queryFn: ({ signal }) => api.getStaffAssignmentHistory(accountId as string, filters, page.skip, page.take, { signal }),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

/**
 * The staff's currently-active assignments straight from the server-side
 * time-aware query (spec 09 §7.2) — never a client-side filter over a page of
 * history.
 */
export function useStaffActiveAssignments(
  staffId: string | null | undefined,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: staffAssignmentKeys.active(staffId ?? ''),
    queryFn: ({ signal }) => api.getStaffAssignments(staffId as string, { signal }),
    enabled: (options.enabled ?? true) && !!staffId,
  });
}

export function useAssignStaffToTransporter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      staffId,
      transporterId,
      startsAt,
      assignmentType,
    }: {
      staffId: string;
      transporterId: string;
      startsAt: string;
      assignmentType: string;
    }) => api.assignStaffToTransporter(staffId, transporterId, startsAt, assignmentType),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffAssignmentKeys.all }),
  });
}

export function useEndStaffAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      staffTransporterAssignmentId,
      endsAt,
    }: {
      staffTransporterAssignmentId: string;
      endsAt?: string | null;
    }) => api.endStaffAssignment(staffTransporterAssignmentId, endsAt ?? null),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffAssignmentKeys.all }),
  });
}


/** Active staff as picker rows (names and ids only); the server caps the set. */
export function useStaffLookup(search = '') {
  return useQuery({
    queryKey: staffKeys.lookup(search),
    queryFn: ({ signal }) => api.getStaffLookup(search, null, { signal }),
  });
}

/** Names for the staff ids a board shows, active or not. */
export function useStaffNames(staffIds: string[]) {
  return useQuery({
    queryKey: staffKeys.names(staffIds),
    queryFn: ({ signal }) => api.getStaffLookup(null, staffIds, { signal }),
    enabled: staffIds.length > 0,
  });
}

export const useStaffSearchOptions: SearchOptionsHook = (search) => {
  const { data, isFetching } = useStaffLookup(search);
  return {
    options: (data ?? []).map((staff) => ({ value: staff.staffId, label: staff.name })),
    loading: isFetching,
  };
};
