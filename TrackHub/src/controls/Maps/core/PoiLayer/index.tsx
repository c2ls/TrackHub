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

import { useMapProvider, MAP_PROVIDERS } from 'controls/Maps/core/MapProviderContext';
import type { MapPoi } from 'controls/Maps/core/mapTypes';
import OSMPoiLayer from './OSM';
import GooglePoiLayer from './Google';

export interface PoiLayerProps {
  pois?: MapPoi[];
}

/**
 * Provider-agnostic point-of-interest layer. Renders POI markers with a
 * colored pin, a name tooltip and a small popup (name/description/address/type).
 * Dispatches to the adapter of the hosting map provider.
 */
const PoiLayer = ({ pois = [] }: PoiLayerProps) => {
  const { provider } = useMapProvider();
  return provider === MAP_PROVIDERS.GOOGLE
    ? <GooglePoiLayer pois={pois} />
    : <OSMPoiLayer pois={pois} />;
};

export default PoiLayer;
