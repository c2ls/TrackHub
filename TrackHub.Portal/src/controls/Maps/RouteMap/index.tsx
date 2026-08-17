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

import OSMRouteMap from 'controls/Maps/OSM/OSMRouteMap';
import GoogleRouteMap from 'controls/Maps/Google/GoogleRouteMap';
import { MAP_PROVIDERS } from 'controls/Maps/core/MapProviderContext';
import type { MapProvider } from 'controls/Maps/core/MapProviderContext';
import type { RouteMapProps } from './routeMapProps';

export type { RouteMapProps };

export interface RouteMapDispatcherProps extends RouteMapProps {
  /** Account map preference ('OSM' | 'Google'), from account settings. */
  mapType?: MapProvider;
}

/**
 * Picks the route-map host for the account's configured provider. Consumers
 * render this, never a host directly — the provider choice is account
 * configuration, not a screen decision.
 */
const RouteMap = ({ mapType = MAP_PROVIDERS.OSM, ...props }: RouteMapDispatcherProps) =>
  mapType === MAP_PROVIDERS.GOOGLE ? <GoogleRouteMap {...props} /> : <OSMRouteMap {...props} />;

export default RouteMap;
