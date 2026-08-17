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
import type { ListParams } from 'api/core/paging';

/** Manager-backed operator data (CRUD, lists, enable/sync). */
export const operatorKeys = {
  all: ['operators'] as const,
  byAccount: (params: ListParams = {}) => [...operatorKeys.all, 'byAccount', params] as const,
  lookup: () => [...operatorKeys.all, 'lookup'] as const,
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

/** One server page of operators (`{ items, totalCount }`) for the operator list. */
export function useOperatorsByCurrentAccount(
  params: ListParams = {},
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: operatorKeys.byAccount(params),
    queryFn: () => managerApi.getOperatorsByCurrentAccount(params),
    enabled: options.enabled ?? true,
  });
}

/** The account's operators as id + name, for the dashboard and report filters. */
export function useOperatorLookup(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: operatorKeys.lookup(),
    queryFn: managerApi.getOperatorLookup,
    enabled: options.enabled ?? true,
  });
}

/**
 * Every operator with its sync metadata (all server pages drained). The GPS
 * screens label and filter rows by operator and read fields the lookup does not
 * carry, so they need the complete record set.
 */
export function useGpsOperators(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: operatorKeys.gps(),
    queryFn: managerApi.getAllGpsOperators,
    enabled: options.enabled ?? true,
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
