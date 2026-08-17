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

import { useEffect, useRef } from 'react';
import { MapContainer, TileLayer } from 'react-leaflet';
import L from 'leaflet';
import { MAP_DEFAULTS } from 'api/core/endpoints';
import MapProviderContext, { OSM_PROVIDER } from 'controls/Maps/core/MapProviderContext';
import RouteLayer from 'controls/Maps/core/RouteLayer';
import TrailLayer from 'controls/Maps/core/TrailLayer';
import PlaybackMarker from 'controls/Maps/core/PlaybackMarker';
import PoiLayer from 'controls/Maps/core/PoiLayer';
import { OSMScaleControl } from 'controls/Maps/shared/ScaleControl';
import { OSMFullscreenControl } from 'controls/Maps/shared/FullscreenControl';
import { OSM_LIGHT_TILE, OSM_DARK_TILE } from 'controls/Maps/utils/darkMapStyles';
import 'leaflet/dist/leaflet.css';
import type { RouteMapProps } from 'controls/Maps/RouteMap/routeMapProps';

/**
 * Leaflet host for the trip route planner and trip detail map. Wraps its
 * children in the OSM provider context so the core layers (RouteLayer,
 * TrailLayer, PlaybackMarker, PoiLayer) dispatch to their Leaflet adapters.
 */
const OSMRouteMap = ({
  route = [],
  corridor = [],
  stops = [],
  tollStations = [],
  trail = [],
  pois = [],
  playbackPosition = null,
  onMapClick,
  onStopClick,
  darkMode = false,
  height = '60vh',
}: RouteMapProps) => {
  const mapRef = useRef<L.Map | null>(null);
  const tile = darkMode ? OSM_DARK_TILE : OSM_LIGHT_TILE;

  // Fit to whatever the trip actually covers (stops + planned route). Re-runs
  // when either changes, so adding a stop keeps the whole itinerary in frame.
  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;
    const points: [number, number][] = [
      ...stops.map((stop) => [stop.lat, stop.lng] as [number, number]),
      ...route.map((point) => [point.lat, point.lng] as [number, number]),
    ];
    if (points.length === 0) return;
    map.fitBounds(L.latLngBounds(points), { padding: [40, 40], maxZoom: 15 });
  }, [stops, route]);

  return (
    <MapProviderContext.Provider value={OSM_PROVIDER}>
      <MapContainer
        ref={mapRef}
        center={{ lat: MAP_DEFAULTS.lat, lng: MAP_DEFAULTS.lng }}
        zoom={11}
        style={{ height, width: '100%' }}
      >
        <TileLayer
          key={darkMode ? 'dark' : 'light'}
          url={tile.url}
          attribution={tile.attribution}
          className={tile.className}
        />
        <PoiLayer pois={pois} />
        <RouteLayer
          route={route}
          corridor={corridor}
          stops={stops}
          tollStations={tollStations}
          onMapClick={onMapClick}
          onStopClick={onStopClick}
        />
        <TrailLayer points={trail} />
        {playbackPosition && <PlaybackMarker position={playbackPosition} />}
        <OSMScaleControl position="bottomleft" imperial={false} />
        <OSMFullscreenControl position="topleft" />
      </MapContainer>
    </MapProviderContext.Provider>
  );
};

export default OSMRouteMap;
