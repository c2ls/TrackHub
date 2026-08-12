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
 * Geocoding-provider API (Manager backend): plain typed async functions. Failures
 * THROW ApiError — fallbacks and toasts belong to the caller layer (src/queries).
 */

import { executeGraphQL } from 'api/core/graphqlClient';
import type {
  GeocodingProviderItemFragment as GeocodingProviderItemType,
  GeocodingProviderDtoInput,
  UpdateGeocodingProviderDtoInput,
} from './generated/graphql';
import {
  GetGeocodingProvidersDocument,
  CreateGeocodingProviderDocument,
  UpdateGeocodingProviderDocument,
  DeleteGeocodingProviderDocument,
  SetActiveGeocodingProviderDocument,
} from './geocodingProviderOperations';

export type GeocodingProvider = GeocodingProviderItemType;
export type { GeocodingProviderDtoInput, UpdateGeocodingProviderDtoInput };

/** Shared field normalization for the geocoding-provider DTOs (excludes `active`). */
function toProviderFields(provider: UpdateGeocodingProviderDtoInput) {
  return {
    name: provider.name,
    type: Number(provider.type),
    endpointUri: provider.endpointUri,
    apiKey: provider.apiKey ?? null,
    requestsPerSecond: Number(provider.requestsPerSecond) || 1,
    timeoutSeconds: Number(provider.timeoutSeconds) || 5,
    configurationJson: provider.configurationJson ?? null,
  };
}

export async function getGeocodingProviders(): Promise<GeocodingProvider[]> {
  const data = await executeGraphQL('manager', GetGeocodingProvidersDocument);
  return data.geocodingProviders;
}

export async function createGeocodingProvider(
  provider: GeocodingProviderDtoInput
): Promise<GeocodingProvider> {
  const input: GeocodingProviderDtoInput = {
    ...toProviderFields(provider),
    active: !!provider.active,
  };
  const data = await executeGraphQL('manager', CreateGeocodingProviderDocument, {
    geocodingProvider: input,
  });
  return data.createGeocodingProvider;
}

export async function updateGeocodingProvider(
  geocodingProviderId: string,
  provider: UpdateGeocodingProviderDtoInput
): Promise<boolean> {
  const input: UpdateGeocodingProviderDtoInput = toProviderFields(provider);
  const data = await executeGraphQL('manager', UpdateGeocodingProviderDocument, {
    id: geocodingProviderId,
    geocodingProvider: input,
  });
  return data.updateGeocodingProvider;
}

/** Returns the id of the deleted provider (schema: `deleteGeocodingProvider: UUID!`). */
export async function deleteGeocodingProvider(geocodingProviderId: string): Promise<string> {
  const data = await executeGraphQL('manager', DeleteGeocodingProviderDocument, {
    id: geocodingProviderId,
  });
  return data.deleteGeocodingProvider;
}

export async function setActiveGeocodingProvider(
  geocodingProviderId: string
): Promise<boolean> {
  const data = await executeGraphQL('manager', SetActiveGeocodingProviderDocument, {
    id: geocodingProviderId,
  });
  return data.setActiveGeocodingProvider;
}
