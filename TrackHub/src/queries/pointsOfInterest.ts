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
 * Point-of-interest query/mutation hooks. Components consume these — not the api
 * layer directly.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/pointsOfInterest';
import type {
  PointOfInterestDtoInput,
  UpdatePointOfInterestDtoInput,
} from 'api/manager/pointsOfInterest';

export const poiKeys = {
  all: ['pointsOfInterest'] as const,
  byAccount: () => [...poiKeys.all, 'byAccount'] as const,
};

export function usePointsOfInterestByAccount(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: poiKeys.byAccount(),
    queryFn: api.getPointsOfInterestByAccount,
    enabled: options.enabled ?? true,
  });
}

export function useCreatePointOfInterest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (poi: PointOfInterestDtoInput) => api.createPointOfInterest(poi),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: poiKeys.all }),
  });
}

export function useUpdatePointOfInterest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      pointOfInterestId,
      ...poi
    }: UpdatePointOfInterestDtoInput & { pointOfInterestId: string }) =>
      api.updatePointOfInterest(pointOfInterestId, poi),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: poiKeys.all }),
  });
}

export function useDeletePointOfInterest() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (pointOfInterestId: string) => api.deletePointOfInterest(pointOfInterestId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: poiKeys.all }),
  });
}
