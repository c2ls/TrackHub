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
 * Device query/mutation hooks. Components consume these — not the api layer
 * directly. Loading/error state comes from the hooks; failures also surface in
 * the global toast via the query client's error handlers. Synchronized-device
 * reads/mutations are called imperatively from the GPS-integration screens.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/devices';
import type { ListParams } from 'api/core/paging';

export const deviceKeys = {
  all: ['devices'] as const,
  byAccount: (params: ListParams = {}) => [...deviceKeys.all, 'byAccount', params] as const,
  lookup: () => [...deviceKeys.all, 'lookup'] as const,
};

/** One server page of devices (`{ items, totalCount }`) for the device list. */
export function useDevicesByAccount(params: ListParams = {}, options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: deviceKeys.byAccount(params),
    queryFn: () => api.getDevicesByAccount(params),
    enabled: options.enabled ?? true,
  });
}

/**
 * The account's devices as id + name, for pickers and deviceId→name maps. Keyed
 * under {@link deviceKeys.all} so a device mutation refreshes it alongside the
 * paged list.
 */
export function useDeviceLookup(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: deviceKeys.lookup(),
    queryFn: api.getDeviceLookup,
    enabled: options.enabled ?? true,
  });
}

export function useDeleteDevice() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (deviceId: string) => api.deleteDevice(deviceId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: deviceKeys.all }),
  });
}
