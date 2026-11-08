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
 * Staff credential/device hooks (Security backend). Components consume these —
 * not the api layer directly. Credential and device administration is CORE
 * platform surface: it is never gated behind an account feature.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/security/staffIdentity';
import type { StaffCredentialDtoInput } from 'api/security/staffIdentity';

// 'all-pages' pins the page dimension into the keys: these entries hold the
// exhaustively-paged result, never a single clamped page.
export const staffCredentialKeys = {
  all: ['staffCredentials'] as const,
  list: (accountId: string, staffId: string | null) =>
    [...staffCredentialKeys.all, accountId, staffId ?? '', 'all-pages'] as const,
};

export const staffDeviceKeys = {
  all: ['staffDevices'] as const,
  list: (accountId: string, staffId: string | null) =>
    [...staffDeviceKeys.all, accountId, staffId ?? '', 'all-pages'] as const,
};

export function useStaffCredentials(
  accountId: string | undefined,
  staffId: string | null = null,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: staffCredentialKeys.list(accountId ?? '', staffId),
    queryFn: ({ signal }) => api.getAllStaffCredentials(accountId as string, staffId, { signal }),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

export function useStaffDevices(
  accountId: string | undefined,
  staffId: string | null = null,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: staffDeviceKeys.list(accountId ?? '', staffId),
    queryFn: ({ signal }) => api.getAllStaffDevices(accountId as string, staffId, { signal }),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

export function useCreateStaffCredential() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (credential: StaffCredentialDtoInput) => api.createStaffCredential(credential),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffCredentialKeys.all }),
  });
}

export function useActivateStaffCredential() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      staffCredentialId,
      password,
    }: {
      staffCredentialId: string;
      password: string;
    }) => api.activateStaffCredential(staffCredentialId, password),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffCredentialKeys.all }),
  });
}

export function useLockStaffCredential() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      staffCredentialId,
      lockedUntil,
    }: {
      staffCredentialId: string;
      lockedUntil: string;
    }) => api.lockStaffCredential(staffCredentialId, lockedUntil),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffCredentialKeys.all }),
  });
}

export function useResetStaffCredential() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      staffCredentialId,
      password,
      resetRequired,
    }: {
      staffCredentialId: string;
      password: string;
      resetRequired: boolean;
    }) => api.resetStaffCredential(staffCredentialId, password, resetRequired),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffCredentialKeys.all }),
  });
}

export function useRevokeStaffCredential() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (staffCredentialId: string) => api.revokeStaffCredential(staffCredentialId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffCredentialKeys.all }),
  });
}

export function useRevokeStaffDevice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      staffDeviceRegistrationId,
      revokedBy,
    }: {
      staffDeviceRegistrationId: string;
      revokedBy: string;
    }) => api.revokeStaffDevice(staffDeviceRegistrationId, revokedBy),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: staffDeviceKeys.all }),
  });
}
