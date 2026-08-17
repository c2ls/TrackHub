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
 * Transporter-type query/mutation hooks. Components consume these — not the api
 * layer directly.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/transporterTypes';
import type { TransporterTypeDtoInput } from 'api/manager/transporterTypes';

export const transporterTypeKeys = {
  all: ['transporterTypes'] as const,
};

export function useTransporterTypes(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: transporterTypeKeys.all,
    queryFn: api.getTransporterTypes,
    enabled: options.enabled ?? true,
  });
}

export function useUpdateTransporterType() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      transporterTypeId,
      ...transporterType
    }: Omit<TransporterTypeDtoInput, 'transporterTypeId'> & { transporterTypeId: number }) =>
      api.updateTransporterType(transporterTypeId, transporterType),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: transporterTypeKeys.all }),
  });
}
