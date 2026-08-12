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
 * Transporter query/mutation hooks. Components consume these — not the api
 * layer directly. Loading/error state comes from the hooks; failures also
 * surface in the global toast via the query client's error handlers.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/transporters';
import type {
  TransporterDtoInput,
  UpdateTransporterDtoInput,
  TransporterDeviceAssignmentDtoInput,
} from 'api/manager/transporters';

export const transporterKeys = {
  all: ['transporters'] as const,
  byAccount: () => [...transporterKeys.all, 'byAccount'] as const,
  byUser: () => [...transporterKeys.all, 'byUser'] as const,
  byGroup: (groupId: number) => [...transporterKeys.all, 'byGroup', groupId] as const,
  detail: (id: string) => [...transporterKeys.all, 'detail', id] as const,
  assignments: ['transporterDeviceAssignments'] as const,
  assignmentsByAccount: (accountId: string, activeOnly: boolean) =>
    [...transporterKeys.assignments, 'byAccount', accountId, activeOnly] as const,
  assignmentsByTransporter: (transporterId: string, activeOnly: boolean) =>
    [...transporterKeys.assignments, 'byTransporter', transporterId, activeOnly] as const,
};

export function useTransporter(transporterId: string | undefined) {
  return useQuery({
    queryKey: transporterKeys.detail(transporterId ?? ''),
    queryFn: () => api.getTransporter(transporterId as string),
    enabled: !!transporterId,
  });
}

export function useTransportersByAccount(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: transporterKeys.byAccount(),
    queryFn: api.getTransportersByAccount,
    enabled: options.enabled ?? true,
  });
}

export function useTransportersByUser(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: transporterKeys.byUser(),
    queryFn: api.getTransportersByUser,
    enabled: options.enabled ?? true,
  });
}

export function useTransportersByGroup(groupId: number | undefined) {
  return useQuery({
    queryKey: transporterKeys.byGroup(groupId ?? -1),
    queryFn: () => api.getTransportersByGroup(groupId as number),
    enabled: groupId !== undefined,
  });
}

export function useCreateTransporter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (transporter: TransporterDtoInput) => api.createTransporter(transporter),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: transporterKeys.all }),
  });
}

export function useUpdateTransporter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      transporterId,
      ...transporter
    }: Omit<UpdateTransporterDtoInput, 'transporterId'> & { transporterId: string }) =>
      api.updateTransporter(transporterId, transporter),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: transporterKeys.all }),
  });
}

export function useDeleteTransporter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (transporterId: string) => api.deleteTransporter(transporterId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: transporterKeys.all }),
  });
}

export function useTransporterDeviceAssignmentsByAccount(
  accountId: string | undefined,
  activeOnly = false
) {
  return useQuery({
    queryKey: transporterKeys.assignmentsByAccount(accountId ?? '', activeOnly),
    queryFn: () => api.getTransporterDeviceAssignmentsByAccount(accountId as string, activeOnly),
    enabled: !!accountId,
  });
}

export function useTransporterDeviceAssignmentsByTransporter(
  transporterId: string | undefined,
  activeOnly = false
) {
  return useQuery({
    queryKey: transporterKeys.assignmentsByTransporter(transporterId ?? '', activeOnly),
    queryFn: () =>
      api.getTransporterDeviceAssignmentsByTransporter(transporterId as string, activeOnly),
    enabled: !!transporterId,
  });
}

export function useAssignDeviceToTransporter() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (assignment: TransporterDeviceAssignmentDtoInput) =>
      api.assignDeviceToTransporter(assignment),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: transporterKeys.assignments }),
  });
}

export function useEndDeviceTransporterAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ assignmentId, reason }: { assignmentId: string; reason?: string | null }) =>
      api.endDeviceTransporterAssignment(assignmentId, reason ?? null),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: transporterKeys.assignments }),
  });
}
