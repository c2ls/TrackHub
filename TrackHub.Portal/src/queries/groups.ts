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
import type { ListParams } from 'api/core/paging';

export const groupKeys = {
  all: ['groups'] as const,
  byAccount: (params: ListParams = {}) => [...groupKeys.all, 'byAccount', params] as const,
  lookup: () => [...groupKeys.all, 'lookup'] as const,
  usersByGroup: (groupId: number) => [...groupKeys.all, 'users', groupId] as const,
};

/** One server page of groups (`{ items, totalCount }`) for the group list. */
export function useGroups(params: ListParams = {}, options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: groupKeys.byAccount(params),
    queryFn: () => api.getGroups(params),
    enabled: options.enabled ?? true,
  });
}

/**
 * The account's groups as id + name, for pickers and groupId→name maps. Keyed
 * under {@link groupKeys.all} so a group mutation refreshes it alongside the
 * paged list.
 */
export function useGroupLookup(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: groupKeys.lookup(),
    queryFn: api.getGroupLookup,
    enabled: options.enabled ?? true,
  });
}

/**
 * A group's complete membership (every server page drained). The allocator
 * dialog subtracts it from the account's users, so a partial list would offer
 * already-assigned users again.
 */
export function useUsersByGroup(groupId: number | undefined) {
  return useQuery({
    queryKey: groupKeys.usersByGroup(groupId ?? -1),
    queryFn: () => api.getAllUsersByGroup(groupId as number),
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
