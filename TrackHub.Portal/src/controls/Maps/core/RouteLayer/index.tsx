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
import type { RoutePoint, RouteStop, RouteTollStation } from 'controls/Maps/core/mapTypes';
import OSMRouteLayer from './OSM';
import GoogleRouteLayer from './Google';

export interface RouteLayerProps {
  /** Planned-route polyline, in order. */
  route?: RoutePoint[];
  /** Corridor ring — the deviation tolerance band around the planned route. */
  corridor?: RoutePoint[];
  /** Trip stops, rendered as numbered pins coloured by stop status. */
  stops?: RouteStop[];
  /** Toll stations matched to the planned route. */
  tollStations?: RouteTollStation[];
  /** Called with the clicked map coordinate — the planner's stop-placement gesture. */
  onMapClick?: (point: RoutePoint) => void;
  /** Called with a stop's id when its pin is clicked. */
  onStopClick?: (stopId: string) => void;
}

/**
 * Provider-agnostic route layer: planned-route polyline, corridor polygon,
 * numbered stop markers and toll-station markers. Written once against
 * `core/mapTypes` and dispatched to the OSM or Google adapter — consumers only
 * ever import this dispatcher, never an adapter.
 */
const RouteLayer = ({
  route = [],
  corridor = [],
  stops = [],
  tollStations = [],
  onMapClick,
  onStopClick,
}: RouteLayerProps) => {
  const { provider } = useMapProvider();
  const props = { route, corridor, stops, tollStations, onMapClick, onStopClick };
  return provider === MAP_PROVIDERS.GOOGLE ? (
    <GoogleRouteLayer {...props} />
  ) : (
    <OSMRouteLayer {...props} />
  );
};

export default RouteLayer;
