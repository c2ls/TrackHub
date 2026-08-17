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
 * Shared presentation config for every portal map so the dashboard overlay and
 * the geofence editor render and feel identical. Both the clustered maps
 * (dashboard) and the geofence editors consume these constants together with
 * the tile configs in {@link ../utils/darkMapStyles} and the default center
 * {@link MAP_DEFAULTS} — the only per-map differences left are the clustering vs
 * editing behaviours, never the styling.
 */

/** CSS class (controls/Maps/css/map.css) that rounds the corners and clips the map viewport. */
export const MAP_CONTAINER_CLASS = 'map-container';

/** Initial OSM/Leaflet zoom used before the viewport is fit to loaded data. */
export const DEFAULT_OSM_ZOOM = 13;

/** Initial Google Maps zoom used before the viewport is fit to loaded data. */
export const DEFAULT_GOOGLE_ZOOM = 6;

/** Padding (px) kept around loaded geofences when fitting the viewport to them. */
export const GEOFENCE_FIT_PADDING = 40;
