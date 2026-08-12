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

const geocodingProviderTypes = [
    { value: 1, label: 'Nominatim' },
    { value: 2, label: 'OpenRouteService' },
    { value: 3, label: 'Google' }
  ] as const;

export type GeocodingProviderType = (typeof geocodingProviderTypes)[number];
export type GeocodingProviderTypeValue = GeocodingProviderType['value'];

const getGeocodingProviderType = (value: number): string => {
  const providerType = geocodingProviderTypes.find(type => type.value === value);
  return providerType ? providerType.label : '';
};

export {
  geocodingProviderTypes,
  getGeocodingProviderType
};
