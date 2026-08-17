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
import { GoogleMap, LoadScript } from '@react-google-maps/api';
import { MAP_DEFAULTS } from 'api/core/endpoints';
import MapProviderContext, { GOOGLE_PROVIDER } from 'controls/Maps/core/MapProviderContext';
import RouteLayer from 'controls/Maps/core/RouteLayer';
import TrailLayer from 'controls/Maps/core/TrailLayer';
import PlaybackMarker from 'controls/Maps/core/PlaybackMarker';
import PoiLayer from 'controls/Maps/core/PoiLayer';
import { GoogleScaleControl } from 'controls/Maps/shared/ScaleControl';
import { GoogleFullscreenControl } from 'controls/Maps/shared/FullscreenControl';
import { GOOGLE_NIGHT_STYLES } from 'controls/Maps/utils/darkMapStyles';
import type { RouteMapProps } from 'controls/Maps/RouteMap/routeMapProps';

/**
 * Google host for the trip route planner and trip detail map. Same prop
 * contract and same core layers as {@link OSMRouteMap} — only the base map and
 * the provider context value differ.
 */
const GoogleRouteMap = ({
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
  mapKey,
}: RouteMapProps) => {
  const mapRef = useRef<google.maps.Map | null>(null);

  useEffect(() => {
    const map = mapRef.current;
    if (!map || !window.google?.maps) return;
    const points = [
      ...stops.map((stop) => ({ lat: stop.lat, lng: stop.lng })),
      ...route.map((point) => ({ lat: point.lat, lng: point.lng })),
    ];
    if (points.length === 0) return;
    const bounds = new window.google.maps.LatLngBounds();
    points.forEach((point) => bounds.extend(point));
    map.fitBounds(bounds, 40);
  }, [stops, route]);

  return (
    <LoadScript googleMapsApiKey={mapKey ?? ''}>
      <MapProviderContext.Provider value={GOOGLE_PROVIDER}>
        <GoogleMap
          mapContainerStyle={{ height, width: '100%' }}
          zoom={11}
          center={{ lat: MAP_DEFAULTS.lat, lng: MAP_DEFAULTS.lng }}
          onLoad={(map) => {
            mapRef.current = map;
          }}
          options={{ gestureHandling: 'greedy', styles: darkMode ? GOOGLE_NIGHT_STYLES : null }}
        >
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
          <GoogleScaleControl mapRef={mapRef} />
          <GoogleFullscreenControl mapRef={mapRef} position="TOP_LEFT" />
        </GoogleMap>
      </MapProviderContext.Provider>
    </LoadScript>
  );
};

export default GoogleRouteMap;
