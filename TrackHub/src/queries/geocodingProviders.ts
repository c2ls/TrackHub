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
 * Geocoding-provider query/mutation hooks. Components consume these — not the
 * api layer directly.
 */

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import * as api from 'api/manager/geocodingProviders';
import type {
  GeocodingProviderDtoInput,
  UpdateGeocodingProviderDtoInput,
} from 'api/manager/geocodingProviders';

export const geocodingProviderKeys = {
  all: ['geocodingProviders'] as const,
};

export function useGeocodingProviders(options: { enabled?: boolean } = {}) {
  return useQuery({
    queryKey: geocodingProviderKeys.all,
    queryFn: api.getGeocodingProviders,
    enabled: options.enabled ?? true,
  });
}

export function useCreateGeocodingProvider() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (provider: GeocodingProviderDtoInput) => api.createGeocodingProvider(provider),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: geocodingProviderKeys.all }),
  });
}

export function useUpdateGeocodingProvider() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      geocodingProviderId,
      ...provider
    }: UpdateGeocodingProviderDtoInput & { geocodingProviderId: string }) =>
      api.updateGeocodingProvider(geocodingProviderId, provider),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: geocodingProviderKeys.all }),
  });
}

export function useDeleteGeocodingProvider() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (geocodingProviderId: string) => api.deleteGeocodingProvider(geocodingProviderId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: geocodingProviderKeys.all }),
  });
}

export function useSetActiveGeocodingProvider() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (geocodingProviderId: string) =>
      api.setActiveGeocodingProvider(geocodingProviderId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: geocodingProviderKeys.all }),
  });
}
