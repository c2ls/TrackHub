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
 * Group query/mutation hooks. Components consume these — not the api layer
 * directly. Group CRUD and the user-membership read are on the query layer;
 * the membership-delete mutations keep their legacy silent semantics and are
 * therefore invoked directly from the allocator dialogs (see api/manager/groups),
 * with `groupKeys` used for the follow-up invalidation.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/groups';
import type { GroupDtoInput, UpdateGroupDtoInput } from 'api/manager/groups';

export const groupKeys = {
  all: ['groups'] as const,
  byAccount: () => [...groupKeys.all, 'byAccount'] as const,
  usersByGroup: (groupId: number) => [...groupKeys.all, 'users', groupId] as const,
};

export function useGroups(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: groupKeys.byAccount(),
    queryFn: api.getGroups,
    enabled: options.enabled ?? true,
  });
}

export function useUsersByGroup(groupId: number | undefined) {
  return useQuery({
    queryKey: groupKeys.usersByGroup(groupId ?? -1),
    queryFn: () => api.getUsersByGroup(groupId as number),
    enabled: groupId !== undefined,
  });
}

export function useCreateGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (group: GroupDtoInput) => api.createGroup(group),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: groupKeys.all }),
  });
}

export function useUpdateGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      groupId,
      ...group
    }: Omit<UpdateGroupDtoInput, 'groupId'> & { groupId: number }) =>
      api.updateGroup(groupId, group),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: groupKeys.all }),
  });
}

export function useDeleteGroup() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (groupId: number) => api.deleteGroup(groupId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: groupKeys.all }),
  });
}
