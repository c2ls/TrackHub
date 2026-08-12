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
 * Service-client-permission query/mutation hooks (Security backend).
 * Components consume these — not the api layer directly. Loading/error state
 * comes from the hooks; failures also surface in the global toast via the
 * query client's error handlers.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/security/serviceClientPermissions';
import type { ServiceClientPermissionDtoInput } from 'api/security/serviceClientPermissions';

export const serviceClientPermissionKeys = {
  all: ['serviceClientPermissions'] as const,
};

export function useServiceClientPermissions(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: serviceClientPermissionKeys.all,
    queryFn: () => api.getServiceClientPermissions(),
    enabled: options.enabled ?? true,
  });
}

export function useCreateServiceClientPermission() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (permission: ServiceClientPermissionDtoInput) =>
      api.createServiceClientPermission(permission),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: serviceClientPermissionKeys.all }),
  });
}

export function useUpdateServiceClientPermission() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      serviceClientPermissionId,
      permission,
    }: {
      serviceClientPermissionId: string;
      permission: ServiceClientPermissionDtoInput;
    }) => api.updateServiceClientPermission(serviceClientPermissionId, permission),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: serviceClientPermissionKeys.all }),
  });
}

export function useDeleteServiceClientPermission() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (serviceClientPermissionId: string) =>
      api.deleteServiceClientPermission(serviceClientPermissionId),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: serviceClientPermissionKeys.all }),
  });
}
