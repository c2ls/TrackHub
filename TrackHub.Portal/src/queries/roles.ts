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
 * Role query/mutation hooks (Security backend). Components consume these — not
 * the api layer directly. Loading/error state comes from the hooks; failures
 * also surface in the global toast via the query client's handlers. The
 * resource/action assignment mutations stay outside this layer: their old
 * callers swallowed errors silently and drive local checkbox state directly.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/security/roles';

export const roleKeys = {
  all: ['roles'] as const,
  list: () => [...roleKeys.all, 'list'] as const,
  resources: (roleId: number) => [...roleKeys.all, 'resources', roleId] as const,
  users: (roleId: number) => [...roleKeys.all, 'users', roleId] as const,
};

export function useRoles(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: roleKeys.list(),
    queryFn: api.getRoles,
    enabled: options.enabled ?? true,
  });
}

export function useRoleResources(roleId: number | undefined) {
  return useQuery({
    queryKey: roleKeys.resources(roleId ?? -1),
    queryFn: () => api.getResourcesByRole(roleId as number),
    enabled: roleId !== undefined && roleId > 0,
  });
}

export function useUsersByRole(roleId: number | undefined) {
  return useQuery({
    queryKey: roleKeys.users(roleId ?? -1),
    queryFn: () => api.getUsersByRole(roleId as number),
    enabled: roleId !== undefined && roleId > 0,
  });
}

export function useCreateUserRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, roleId }: { userId: string; roleId: number }) =>
      api.createUserRole(userId, roleId),
    onSuccess: (_data, { roleId }) =>
      queryClient.invalidateQueries({ queryKey: roleKeys.users(roleId) }),
  });
}

export function useDeleteUserRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, roleId }: { userId: string; roleId: number }) =>
      api.deleteUserRole(userId, roleId),
    onSuccess: (_data, { roleId }) =>
      queryClient.invalidateQueries({ queryKey: roleKeys.users(roleId) }),
  });
}
