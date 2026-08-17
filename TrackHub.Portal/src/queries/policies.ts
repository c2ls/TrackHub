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
 * Policy query/mutation hooks (Security backend). Components consume these —
 * not the api layer directly. Loading/error state comes from the hooks;
 * failures also surface in the global toast via the query client's handlers.
 * The resource/action assignment mutations stay outside this layer: their old
 * callers swallowed errors silently and drive local checkbox state directly.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/security/policies';

export const policyKeys = {
  all: ['policies'] as const,
  list: () => [...policyKeys.all, 'list'] as const,
  resources: (policyId: number) => [...policyKeys.all, 'resources', policyId] as const,
  users: (policyId: number) => [...policyKeys.all, 'users', policyId] as const,
};

export function usePolicies(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: policyKeys.list(),
    queryFn: api.getPolicies,
    enabled: options.enabled ?? true,
  });
}

export function usePolicyResources(policyId: number | undefined) {
  return useQuery({
    queryKey: policyKeys.resources(policyId ?? -1),
    queryFn: () => api.getResourcesByPolicy(policyId as number),
    enabled: policyId !== undefined && policyId > 0,
  });
}

export function useUsersByPolicy(policyId: number | undefined) {
  return useQuery({
    queryKey: policyKeys.users(policyId ?? -1),
    queryFn: () => api.getUsersByPolicy(policyId as number),
    enabled: policyId !== undefined && policyId > 0,
  });
}

export function useCreateUserPolicy() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, policyId }: { userId: string; policyId: number }) =>
      api.createUserPolicy(userId, policyId),
    onSuccess: (_data, { policyId }) =>
      queryClient.invalidateQueries({ queryKey: policyKeys.users(policyId) }),
  });
}

export function useDeleteUserPolicy() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, policyId }: { userId: string; policyId: number }) =>
      api.deleteUserPolicy(userId, policyId),
    onSuccess: (_data, { policyId }) =>
      queryClient.invalidateQueries({ queryKey: policyKeys.users(policyId) }),
  });
}
