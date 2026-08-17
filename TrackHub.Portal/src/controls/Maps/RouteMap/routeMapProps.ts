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

import type {
  RoutePoint,
  RouteStop,
  RouteTollStation,
  TrailPoint,
  PlaybackPosition,
  MapPoi,
} from 'controls/Maps/core/mapTypes';

/**
 * The single prop contract both route-map hosts implement. Declared once so the
 * OSM and Google hosts cannot drift apart — the provider-agnostic rule applies
 * to the host as much as to the layers it renders.
 */
export interface RouteMapProps {
  /** Planned-route polyline. */
  route?: RoutePoint[];
  /** Corridor ring around the planned route. */
  corridor?: RoutePoint[];
  /** Numbered trip stops. */
  stops?: RouteStop[];
  /** Toll stations matched to the route. */
  tollStations?: RouteTollStation[];
  /** Recorded positions for replay, drawn as a trail. */
  trail?: TrailPoint[];
  /** Account points of interest, offered as stop-placement targets. */
  pois?: MapPoi[];
  /** Imperatively-positioned replay marker. */
  playbackPosition?: PlaybackPosition | null;
  /** Stop-placement gesture: fires with the clicked coordinate. */
  onMapClick?: (point: RoutePoint) => void;
  /** Fires with a stop's id when its pin is clicked. */
  onStopClick?: (stopId: string) => void;
  darkMode?: boolean;
  height?: string;
  /** Google Maps API key — ignored by the OSM host. */
  mapKey?: string | null;
}
