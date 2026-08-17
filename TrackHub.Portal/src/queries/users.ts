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
 * User query/mutation hooks (Security backend). Components consume these — not
 * the api layer directly. Loading/error state comes from the hooks; failures
 * also surface in the global toast via the query client's error handlers.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/security/users';
import type {
  CreateUserDtoInput,
  UpdateUserDtoInput,
  UpdateCurrentUserDtoInput,
} from 'api/security/users';

export const userKeys = {
  all: ['users'] as const,
  current: () => [...userKeys.all, 'current'] as const,
  byAccount: () => [...userKeys.all, 'byAccount'] as const,
  integration: () => [...userKeys.all, 'integration'] as const,
};

export function useCurrentUser(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: userKeys.current(),
    queryFn: api.getCurrentUser,
    enabled: options.enabled ?? true,
  });
}

export function useUsersByAccount(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: userKeys.byAccount(),
    queryFn: () => api.getUsersByAccount(),
    enabled: options.enabled ?? true,
  });
}

export function useIntegrationUsers(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: userKeys.integration(),
    queryFn: api.getUsers,
    enabled: options.enabled ?? true,
  });
}

export function useCreateUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (user: CreateUserDtoInput) => api.createUser(user),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.byAccount() }),
  });
}

export function useCreateManager() {
  return useMutation({
    mutationFn: ({ user, accountId }: { user: CreateUserDtoInput; accountId: string }) =>
      api.createManager(user, accountId),
  });
}

export function useUpdateUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      userId,
      ...user
    }: Omit<UpdateUserDtoInput, 'userId'> & { userId: string }) => api.updateUser(userId, user),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.byAccount() }),
  });
}

export function useUpdateCurrentUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (user: UpdateCurrentUserDtoInput) => api.updateCurrentUser(user),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.current() }),
  });
}

export function useUpdatePassword() {
  return useMutation({
    mutationFn: ({ userId, password }: { userId: string; password: string }) =>
      api.updatePassword(userId, password),
  });
}

export function useDeleteUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (userId: string) => api.deleteUser(userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.byAccount() }),
  });
}

export function useUnlockUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (userId: string) => api.unlockUser(userId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: userKeys.byAccount() }),
  });
}
