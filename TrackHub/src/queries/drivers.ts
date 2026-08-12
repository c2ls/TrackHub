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
import type { DriverDtoInput } from 'api/manager/drivers';

export const driverKeys = {
  all: ['drivers'] as const,
  byAccount: (accountId: string) => [...driverKeys.all, 'byAccount', accountId] as const,
};

export function useDriversByAccount(
  accountId: string | undefined,
  options: { enabled?: boolean } = {}
) {
  return useQuery({
    queryKey: driverKeys.byAccount(accountId ?? ''),
    queryFn: () => api.getDriversByAccount(accountId as string),
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
