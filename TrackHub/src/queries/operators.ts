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
 * Operator query/mutation hooks. Operators live on two backends: CRUD +
 * enable/sync on Manager, health + sync-run reads on Telemetry. The cache keys
 * are namespaced accordingly (`operatorKeys` vs `operatorTelemetryKeys`) so an
 * operator mutation never blows away unrelated telemetry cache entries.
 * Components consume these — not the api layer directly.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as managerApi from 'api/manager/operators';
import * as telemetryApi from 'api/telemetry/operatorHealth';
import type {
  OperatorDtoInput,
  UpdateOperatorDtoInput,
  TriggerOperatorDeviceSyncCommandInput,
} from 'api/manager/operators';

/** Manager-backed operator data (CRUD, lists, enable/sync). */
export const operatorKeys = {
  all: ['operators'] as const,
  byAccount: () => [...operatorKeys.all, 'byAccount'] as const,
  summary: () => [...operatorKeys.all, 'summary'] as const,
  gps: () => [...operatorKeys.all, 'gps'] as const,
  detail: (id: string) => [...operatorKeys.all, 'detail', id] as const,
};

/** Telemetry-backed operator data (health + sync runs). */
export const operatorTelemetryKeys = {
  all: ['operatorTelemetry'] as const,
  syncRuns: (accountId: string | null, operatorId: string | null, take: number) =>
    [...operatorTelemetryKeys.all, 'syncRuns', accountId ?? '', operatorId ?? '', take] as const,
  health: (operatorId: string) => [...operatorTelemetryKeys.all, 'health', operatorId] as const,
  healthHistory: (operatorId: string, take: number) =>
    [...operatorTelemetryKeys.all, 'healthHistory', operatorId, take] as const,
};

// --- Manager reads -------------------------------------------------------

export function useOperatorsByCurrentAccount(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: operatorKeys.byAccount(),
    queryFn: managerApi.getOperatorsByCurrentAccount,
    enabled: options.enabled ?? true,
  });
}

export function useOperators(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: operatorKeys.summary(),
    queryFn: managerApi.getOperators,
    enabled: options.enabled ?? true,
  });
}

export function useGpsOperators(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: operatorKeys.gps(),
    queryFn: managerApi.getGpsOperators,
    enabled: options.enabled ?? true,
  });
}

export function useOperator(operatorId: string | undefined) {
  return useQuery({
    queryKey: operatorKeys.detail(operatorId ?? ''),
    queryFn: () => managerApi.getOperator(operatorId as string),
    enabled: !!operatorId,
  });
}

// --- Manager mutations ---------------------------------------------------

export function useCreateOperator() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (operator: OperatorDtoInput) => managerApi.createOperator(operator),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: operatorKeys.all }),
  });
}

export function useUpdateOperator() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      operatorId,
      ...operator
    }: Omit<UpdateOperatorDtoInput, 'operatorId'> & { operatorId: string }) =>
      managerApi.updateOperator(operatorId, operator),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: operatorKeys.all }),
  });
}

export function useDeleteOperator() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (operatorId: string) => managerApi.deleteOperator(operatorId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: operatorKeys.all }),
  });
}

export function useSetOperatorEnabled() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ operatorId, enabled }: { operatorId: string; enabled: boolean }) =>
      managerApi.setOperatorEnabled(operatorId, enabled),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: operatorKeys.all }),
  });
}

export function useTriggerOperatorDeviceSync() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      operatorId,
      resetDeviceCatalog,
      autoAssignNewDevices,
    }: Omit<TriggerOperatorDeviceSyncCommandInput, 'operatorId'> & { operatorId: string }) =>
      managerApi.triggerOperatorDeviceSync(operatorId, resetDeviceCatalog ?? false, autoAssignNewDevices ?? true),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: operatorKeys.all }),
  });
}

// --- Telemetry reads -----------------------------------------------------

export function useOperatorSyncRuns(
  accountId: string | null,
  operatorId: string | null = null,
  take = 20,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: operatorTelemetryKeys.syncRuns(accountId, operatorId, take),
    queryFn: () => telemetryApi.getOperatorSyncRuns(accountId, operatorId, take),
    enabled: (options.enabled ?? true) && !!accountId,
  });
}

export function useOperatorHealth(operatorId: string | undefined) {
  return useQuery({
    queryKey: operatorTelemetryKeys.health(operatorId ?? ''),
    queryFn: () => telemetryApi.getOperatorHealth(operatorId as string),
    enabled: !!operatorId,
  });
}

export function useOperatorHealthHistory(operatorId: string | undefined, take = 50) {
  return useQuery({
    queryKey: operatorTelemetryKeys.healthHistory(operatorId ?? '', take),
    queryFn: () => telemetryApi.getOperatorHealthHistory(operatorId as string, take),
    enabled: !!operatorId,
  });
}
