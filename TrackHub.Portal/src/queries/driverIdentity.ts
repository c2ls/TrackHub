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
 * Driver credential/device hooks (Security backend). Components consume these —
 * not the api layer directly. Credential and device administration is CORE
 * platform surface: it is never gated behind an account feature.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/security/driverIdentity';
import type { DriverCredentialDtoInput } from 'api/security/driverIdentity';

// 'all-pages' pins the page dimension into the keys: these entries hold the
// exhaustively-paged result, never a single clamped page.
export const driverCredentialKeys = {
  all: ['driverCredentials'] as const,
  list: (accountId: string, driverId: string | null) =>
    [...driverCredentialKeys.all, accountId, driverId ?? '', 'all-pages'] as const,
};

export const driverDeviceKeys = {
  all: ['driverDevices'] as const,
  list: (accountId: string, driverId: string | null) =>
    [...driverDeviceKeys.all, accountId, driverId ?? '', 'all-pages'] as const,
};

export function useDriverCredentials(
  accountId: string | undefined,
  driverId: string | null = null,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: driverCredentialKeys.list(accountId ?? '', driverId),
    queryFn: () => api.getAllDriverCredentials(accountId as string, driverId),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

export function useDriverDevices(
  accountId: string | undefined,
  driverId: string | null = null,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: driverDeviceKeys.list(accountId ?? '', driverId),
    queryFn: () => api.getAllDriverDevices(accountId as string, driverId),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

export function useCreateDriverCredential() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (credential: DriverCredentialDtoInput) => api.createDriverCredential(credential),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverCredentialKeys.all }),
  });
}

export function useActivateDriverCredential() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      driverCredentialId,
      password,
    }: {
      driverCredentialId: string;
      password: string;
    }) => api.activateDriverCredential(driverCredentialId, password),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverCredentialKeys.all }),
  });
}

export function useLockDriverCredential() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      driverCredentialId,
      lockedUntil,
    }: {
      driverCredentialId: string;
      lockedUntil: string;
    }) => api.lockDriverCredential(driverCredentialId, lockedUntil),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverCredentialKeys.all }),
  });
}

export function useResetDriverCredential() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      driverCredentialId,
      password,
      resetRequired,
    }: {
      driverCredentialId: string;
      password: string;
      resetRequired: boolean;
    }) => api.resetDriverCredential(driverCredentialId, password, resetRequired),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverCredentialKeys.all }),
  });
}

export function useRevokeDriverCredential() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (driverCredentialId: string) => api.revokeDriverCredential(driverCredentialId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverCredentialKeys.all }),
  });
}

export function useRevokeDriverDevice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      driverDeviceRegistrationId,
      revokedBy,
    }: {
      driverDeviceRegistrationId: string;
      revokedBy: string;
    }) => api.revokeDriverDevice(driverDeviceRegistrationId, revokedBy),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: driverDeviceKeys.all }),
  });
}
