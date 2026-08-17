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

import { useEffect } from 'react';
import { Polyline, Polygon, Marker, useGoogleMap } from '@react-google-maps/api';
import type { RoutePoint, RouteStop, RouteTollStation } from 'controls/Maps/core/mapTypes';
import {
  ROUTE_COLOR,
  CORRIDOR_COLOR,
  stopColor,
  createStopPinSvg,
  createTollPinSvg,
} from './routeStyles';

interface GoogleRouteLayerProps {
  route?: RoutePoint[];
  corridor?: RoutePoint[];
  stops?: RouteStop[];
  tollStations?: RouteTollStation[];
  onMapClick?: (point: RoutePoint) => void;
  onStopClick?: (stopId: string) => void;
}

/**
 * Google has no `useMapEvents`; attach the click listener imperatively and
 * remove it on cleanup so no listener survives a re-render (core README rule).
 */
const MapClickHandler = ({ onMapClick }: { onMapClick: (point: RoutePoint) => void }) => {
  const map = useGoogleMap();
  useEffect(() => {
    if (!map) return undefined;
    const listener = map.addListener('click', (event: google.maps.MapMouseEvent) => {
      if (event.latLng) onMapClick({ lat: event.latLng.lat(), lng: event.latLng.lng() });
    });
    const previous = map.get('draggableCursor') as string | undefined;
    map.setOptions({ draggableCursor: 'crosshair' });
    return () => {
      listener.remove();
      map.setOptions({ draggableCursor: previous ?? null });
    };
  }, [map, onMapClick]);
  return null;
};

const GoogleRouteLayer = ({
  route = [],
  corridor = [],
  stops = [],
  tollStations = [],
  onMapClick,
  onStopClick,
}: GoogleRouteLayerProps) => {
  const routePath = route.map((point) => ({ lat: point.lat, lng: point.lng }));
  const corridorPath = corridor.map((point) => ({ lat: point.lat, lng: point.lng }));
  // The icon factories build `window.google` Size/Point values, so they can only
  // run once the API script has loaded.
  const apiReady = typeof window !== 'undefined' && !!window.google?.maps;

  return (
    <>
      {onMapClick && <MapClickHandler onMapClick={onMapClick} />}
      {corridorPath.length > 2 && (
        <Polygon
          paths={corridorPath}
          options={{
            strokeColor: CORRIDOR_COLOR,
            strokeWeight: 1,
            strokeOpacity: 0.9,
            fillColor: CORRIDOR_COLOR,
            fillOpacity: 0.15,
            clickable: false,
          }}
        />
      )}
      {routePath.length > 1 && (
        <Polyline
          path={routePath}
          options={{ strokeColor: ROUTE_COLOR, strokeWeight: 5, strokeOpacity: 0.85 }}
        />
      )}
      {apiReady &&
        stops.map((stop) => (
          <Marker
            key={stop.id ?? `${stop.sequence}-${stop.lat}-${stop.lng}`}
            position={{ lat: stop.lat, lng: stop.lng }}
            title={stop.name}
            icon={{
              url: createStopPinSvg(stop.sequence, stopColor(stop.status), 'dataURL'),
              scaledSize: new window.google.maps.Size(28, 36),
              anchor: new window.google.maps.Point(14, 34),
            }}
            onClick={onStopClick && stop.id ? () => onStopClick(stop.id as string) : undefined}
          />
        ))}
      {apiReady &&
        tollStations.map((station) => (
          <Marker
            key={station.id ?? `${station.name}-${station.lat}-${station.lng}`}
            position={{ lat: station.lat, lng: station.lng }}
            title={station.name}
            icon={{
              url: createTollPinSvg(station.hasTariff, 'dataURL'),
              scaledSize: new window.google.maps.Size(22, 22),
              anchor: new window.google.maps.Point(11, 11),
            }}
          />
        ))}
    </>
  );
};

export default GoogleRouteLayer;
