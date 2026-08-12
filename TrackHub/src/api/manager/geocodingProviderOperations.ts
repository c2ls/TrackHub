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
 * Geocoding-provider GraphQL documents (Manager backend). Codegen validates these
 * against schemas/manager.graphql and emits typed document nodes — values always
 * travel as variables, never string interpolation.
 */

import { graphql } from './generated';

export const GeocodingProviderItemFragment = graphql(`
  fragment GeocodingProviderItem on GeocodingProviderVm {
    geocodingProviderId
    name
    type
    endpointUri
    apiKey
    requestsPerSecond
    timeoutSeconds
    configurationJson
    active
  }
`);

export const GetGeocodingProvidersDocument = graphql(`
  query GetGeocodingProviders {
    geocodingProviders {
      ...GeocodingProviderItem
    }
  }
`);

export const CreateGeocodingProviderDocument = graphql(`
  mutation CreateGeocodingProvider($geocodingProvider: GeocodingProviderDtoInput!) {
    createGeocodingProvider(command: { geocodingProvider: $geocodingProvider }) {
      ...GeocodingProviderItem
    }
  }
`);

export const UpdateGeocodingProviderDocument = graphql(`
  mutation UpdateGeocodingProvider($id: UUID!, $geocodingProvider: UpdateGeocodingProviderDtoInput!) {
    updateGeocodingProvider(id: $id, command: { id: $id, geocodingProvider: $geocodingProvider })
  }
`);

export const DeleteGeocodingProviderDocument = graphql(`
  mutation DeleteGeocodingProvider($id: UUID!) {
    deleteGeocodingProvider(id: $id)
  }
`);

export const SetActiveGeocodingProviderDocument = graphql(`
  mutation SetActiveGeocodingProvider($id: UUID!) {
    setActiveGeocodingProvider(id: $id)
  }
`);
