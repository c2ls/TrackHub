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
 * Account query/mutation hooks. Components consume these — not the api layer
 * directly. Loading/error state comes from the hooks; failures also surface in
 * the global toast via the query client's error handlers.
 *
 * `useAccountByUser` is the shared "current account id" source for create flows
 * (transporters, POIs, drivers, ...). Account identity rarely changes, so it is
 * cached indefinitely (staleTime: Infinity) and refetched only on invalidation.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/accounts';
import type { AccountDtoInput, UpdateAccountDtoInput, AccountStatus } from 'api/manager/accounts';

export const accountKeys = {
  all: ['accounts'] as const,
  list: () => [...accountKeys.all, 'list'] as const,
  byUser: () => [...accountKeys.all, 'byUser'] as const,
  detail: (id: string) => [...accountKeys.all, 'detail', id] as const,
};

export function useAccountByUser(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: accountKeys.byUser(),
    queryFn: api.getAccountByUser,
    enabled: options.enabled ?? true,
    staleTime: Infinity,
  });
}

export function useAccounts(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: accountKeys.list(),
    queryFn: api.getAccounts,
    enabled: options.enabled ?? true,
  });
}

export function useAccount(accountId: string | undefined) {
  return useQuery({
    queryKey: accountKeys.detail(accountId ?? ''),
    queryFn: () => api.getAccount(accountId as string),
    enabled: !!accountId,
  });
}

export function useCreateAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (account: AccountDtoInput) => api.createAccount(account),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: accountKeys.all }),
  });
}

export function useUpdateAccount() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      accountId,
      ...account
    }: Omit<UpdateAccountDtoInput, 'accountId'> & { accountId: string }) =>
      api.updateAccount(accountId, account),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: accountKeys.all }),
  });
}

export function useChangeAccountStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      accountId,
      targetStatus,
      reason,
    }: {
      accountId: string;
      targetStatus: AccountStatus;
      reason?: string | null;
    }) => api.changeAccountStatus(accountId, targetStatus, reason ?? null),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: accountKeys.all }),
  });
}
