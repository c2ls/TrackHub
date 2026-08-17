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
 * Client query/mutation hooks (Security backend). Components consume these —
 * not the api layer directly. Loading/error state comes from the hooks;
 * failures also surface in the global toast via the query client's handlers.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/security/clients';
import type { ClientDtoInput, ClientUserDtoInput } from 'api/security/clients';

export const clientKeys = {
  all: ['clients'] as const,
  list: () => [...clientKeys.all, 'list'] as const,
};

export function useClients(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: clientKeys.list(),
    queryFn: () => api.getClients(),
    enabled: options.enabled ?? true,
  });
}

export function useCreateClient() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (client: ClientDtoInput) => api.createClient(client),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: clientKeys.all }),
  });
}

export function useUpdateClient() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      clientId,
      ...client
    }: Omit<ClientUserDtoInput, 'clientId'> & { clientId: string }) =>
      api.updateClient(clientId, client),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: clientKeys.all }),
  });
}

export function useDeleteClient() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (clientId: string) => api.deleteClient(clientId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: clientKeys.all }),
  });
}
