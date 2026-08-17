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
import { Polyline, Polygon, Marker, useMapEvents } from 'react-leaflet';
import L from 'leaflet';
import type { LatLngExpression } from 'leaflet';
import type { RoutePoint, RouteStop, RouteTollStation } from 'controls/Maps/core/mapTypes';
import {
  ROUTE_COLOR,
  CORRIDOR_COLOR,
  stopColor,
  createStopPinSvg,
  createTollPinSvg,
} from './routeStyles';

interface OSMRouteLayerProps {
  route?: RoutePoint[];
  corridor?: RoutePoint[];
  stops?: RouteStop[];
  tollStations?: RouteTollStation[];
  onMapClick?: (point: RoutePoint) => void;
  onStopClick?: (stopId: string) => void;
}

/** Registers the map click handler only while the planner wants one. */
const MapClickHandler = ({ onMapClick }: { onMapClick: (point: RoutePoint) => void }) => {
  const map = useMapEvents({
    click: (event) => onMapClick({ lat: event.latlng.lat, lng: event.latlng.lng }),
  });
  // Placing a stop is a pointing gesture; make the cursor say so.
  useEffect(() => {
    const container = map.getContainer();
    const previous = container.style.cursor;
    container.style.cursor = 'crosshair';
    return () => {
      container.style.cursor = previous;
    };
  }, [map]);
  return null;
};

const OSMRouteLayer = ({
  route = [],
  corridor = [],
  stops = [],
  tollStations = [],
  onMapClick,
  onStopClick,
}: OSMRouteLayerProps) => {
  const routePositions: LatLngExpression[] = route.map((point) => [point.lat, point.lng]);
  const corridorPositions: LatLngExpression[] = corridor.map((point) => [point.lat, point.lng]);

  return (
    <>
      {onMapClick && <MapClickHandler onMapClick={onMapClick} />}
      {corridorPositions.length > 2 && (
        <Polygon
          positions={corridorPositions}
          pathOptions={{
            color: CORRIDOR_COLOR,
            weight: 1,
            opacity: 0.9,
            fillColor: CORRIDOR_COLOR,
            fillOpacity: 0.15,
          }}
        />
      )}
      {routePositions.length > 1 && (
        <Polyline positions={routePositions} pathOptions={{ color: ROUTE_COLOR, weight: 5, opacity: 0.85 }} />
      )}
      {stops.map((stop) => (
        <Marker
          key={stop.id ?? `${stop.sequence}-${stop.lat}-${stop.lng}`}
          position={[stop.lat, stop.lng]}
          icon={L.divIcon({
            className: 'route-stop-icon',
            html: createStopPinSvg(stop.sequence, stopColor(stop.status)),
            iconSize: [28, 36],
            iconAnchor: [14, 34],
            tooltipAnchor: [0, -30],
          })}
          title={stop.name}
          eventHandlers={
            onStopClick && stop.id ? { click: () => onStopClick(stop.id as string) } : undefined
          }
        />
      ))}
      {tollStations.map((station) => (
        <Marker
          key={station.id ?? `${station.name}-${station.lat}-${station.lng}`}
          position={[station.lat, station.lng]}
          icon={L.divIcon({
            className: 'route-toll-icon',
            html: createTollPinSvg(station.hasTariff),
            iconSize: [22, 22],
            iconAnchor: [11, 11],
          })}
          title={station.name}
        />
      ))}
    </>
  );
};

export default OSMRouteLayer;
