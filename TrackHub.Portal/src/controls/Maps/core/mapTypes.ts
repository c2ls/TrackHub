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
 * Provider-agnostic view models rendered by the dual (OSM/Google) map stack.
 * These are the shapes the map controls read from; the layout consumers build
 * them from api-layer types ({@link Position}, {@link Geofence}, …) and pass
 * them across the vendored-component boundary.
 */

/** Attribute bag carried on a live position, surfaced in marker popups. */
export interface MarkerAttributes {
  mileage?: number | null;
  temperature?: number | null;
  hourmeter?: number | null;
  ignition?: boolean | null;
  satellites?: number | null;
}

/** A rendered unit marker (derived from a live position). */
export interface MapMarker {
  id?: string;
  name: string;
  lat: number;
  lng: number;
  speed: number;
  rotation: number;
  text: string;
  color?: string;
  dateTime?: string | number | Date | null;
  transporterType?: string;
  altitude?: number | null;
  address?: string | null;
  city?: string | null;
  state?: string | null;
  country?: string | null;
  attributes?: MarkerAttributes | null;
}

/** A geofence rendered as an overlay: a true circle when the circle fields are set, else a polygon. */
export interface MapGeofence {
  geofenceId?: string;
  name: string;
  color: number;
  geom: {
    coordinates: Array<{ latitude: number; longitude: number }>;
  };
  circleCenter?: { latitude: number; longitude: number } | null;
  circleRadiusMeters?: number | null;
}

/** A trip rendered as a polyline (or single marker when `type === 1`). */
export interface MapTrip {
  id: string;
  /** Ordered `[lat, lng]` coordinate pairs. */
  coordinates: [number, number][];
  color?: string;
  type?: number;
}

/** A point of interest rendered with a colored pin, tooltip and popup. */
export interface MapPoi {
  name: string;
  latitude: number;
  longitude: number;
  color: number;
  type: number;
  description?: string | null;
  address?: string | null;
  active?: boolean;
}

/** A `{ lat, lng }` point in the short live-trail polyline. */
export interface TrailPoint {
  lat: number;
  lng: number;
}

/** The imperatively-positioned playback marker location. */
export interface PlaybackPosition {
  lat: number;
  lng: number;
  course?: number;
}

/** A `{ lat, lng }` vertex of a planned route polyline or corridor ring. */
export interface RoutePoint {
  lat: number;
  lng: number;
}

/**
 * A trip stop rendered as a numbered marker. `status` drives the pin colour so a
 * dispatcher reads progress off the map (pending / arrived / departed / skipped).
 */
export interface RouteStop {
  id?: string;
  sequence: number;
  name: string;
  lat: number;
  lng: number;
  status?: string;
  address?: string | null;
  etaAt?: string | null;
}

/**
 * A toll station matched to the planned route. `hasTariff: false` means the
 * station is on the route but is NOT priced for the trip's vehicle class — the
 * marker must show that gap rather than imply a zero cost (spec 11 §18.9).
 */
export interface RouteTollStation {
  id?: string;
  name: string;
  lat: number;
  lng: number;
  hasTariff: boolean;
  amount?: number | null;
  currency?: string | null;
  roadName?: string | null;
}
