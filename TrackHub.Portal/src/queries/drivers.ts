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
 * Driver query/mutation hooks. Components consume these — not the api layer
 * directly. The account id (required to list/create drivers) is still sourced
 * from the account service by the screen.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/drivers';
import type {
  DriverDtoInput,
  DriverQualificationDtoInput,
  DriverAssignmentHistoryFilters,
} from 'api/manager/drivers';

export const driverKeys = {
  all: ['drivers'] as const,
  // 'all-pages' pins the page dimension into the key: the entry holds the
  // exhaustively-paged account list, never a single clamped page.
  byAccount: (accountId: string) => [...driverKeys.all, 'byAccount', accountId, 'all-pages'] as const,
};

export const driverQualificationKeys = {
  all: ['driverQualifications'] as const,
  list: (accountId: string, driverId: string | null, expiringWithinDays: number | null) =>
    [
      ...driverQualificationKeys.all,
      accountId,
      driverId ?? '',
      expiringWithinDays ?? '',
      'all-pages',
    ] as const,
};

export const driverAssignmentKeys = {
  all: ['driverAssignments'] as const,
  history: (accountId: string, filters: DriverAssignmentHistoryFilters) =>
    [
      ...driverAssignmentKeys.all,
      'history',
      accountId,
      filters.driverId ?? '',
      filters.transporterId ?? '',
      filters.from ?? '',
      filters.to ?? '',
      'all-pages',
    ] as const,
  active: (driverId: string) => [...driverAssignmentKeys.all, 'active', driverId] as const,
};

/**
 * Every driver on the account (paged to exhaustion in the api layer). The
 * pickers on every workforce surface depend on completeness — a clamped single
 * page leaves drivers past the boundary unadministrable.
 */
export function useDriversByAccount(
  accountId: string | undefined,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: driverKeys.byAccount(accountId ?? ''),
    queryFn: () => api.getAllDriversByAccount(accountId as string),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

export function useCreateDriver() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (driver: DriverDtoInput) => api.createDriver(driver),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverKeys.all }),
  });
}

export function useUpdateDriver() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ driverId, driver }: { driverId: string; driver: DriverDtoInput }) =>
      api.updateDriver(driverId, driver),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverKeys.all }),
  });
}

export function useDeactivateDriver() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (driverId: string) => api.deactivateDriver(driverId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverKeys.all }),
  });
}

/**
 * Driver qualifications. `driverId` null lists the whole account (the
 * expirations view combines that with `expiringWithinDays`).
 */
export function useDriverQualifications(
  accountId: string | undefined,
  driverId: string | null = null,
  expiringWithinDays: number | null = null,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: driverQualificationKeys.list(accountId ?? '', driverId, expiringWithinDays),
    queryFn: () =>
      api.getAllDriverQualifications(accountId as string, driverId, expiringWithinDays),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

export function useCreateDriverQualification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (qualification: DriverQualificationDtoInput) =>
      api.createDriverQualification(qualification),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: driverQualificationKeys.all }),
  });
}

export function useUpdateDriverQualification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      driverQualificationId,
      qualification,
    }: {
      driverQualificationId: string;
      qualification: DriverQualificationDtoInput;
    }) => api.updateDriverQualification(driverQualificationId, qualification),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: driverQualificationKeys.all }),
  });
}

export function useDeleteDriverQualification() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (driverQualificationId: string) =>
      api.deleteDriverQualification(driverQualificationId),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: driverQualificationKeys.all }),
  });
}

export function useDriverAssignmentHistory(
  accountId: string | undefined,
  filters: DriverAssignmentHistoryFilters = {},
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: driverAssignmentKeys.history(accountId ?? '', filters),
    queryFn: () => api.getAllDriverAssignmentHistory(accountId as string, filters),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

/**
 * The driver's currently-active assignments straight from the server-side
 * time-aware query (spec 09 §7.2) — never a client-side filter over a page of
 * history.
 */
export function useDriverActiveAssignments(
  driverId: string | null | undefined,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: driverAssignmentKeys.active(driverId ?? ''),
    queryFn: () => api.getDriverAssignments(driverId as string),
    enabled: (options.enabled ?? true) && !!driverId,
  });
}

export function useAssignDriverToTransporter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      driverId,
      transporterId,
      startsAt,
      assignmentType,
    }: {
      driverId: string;
      transporterId: string;
      startsAt: string;
      assignmentType: string;
    }) => api.assignDriverToTransporter(driverId, transporterId, startsAt, assignmentType),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverAssignmentKeys.all }),
  });
}

export function useEndDriverAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      driverTransporterAssignmentId,
      endsAt,
    }: {
      driverTransporterAssignmentId: string;
      endsAt?: string | null;
    }) => api.endDriverAssignment(driverTransporterAssignmentId, endsAt ?? null),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverAssignmentKeys.all }),
  });
}
