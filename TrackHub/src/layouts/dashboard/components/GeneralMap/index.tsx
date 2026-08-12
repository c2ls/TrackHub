/**
* Copyright (c) 2025 Sergio Hernandez. All rights reserved.
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

import { useState, useEffect } from 'react';
import OSMClusteredMap from 'controls/Maps/OSM/OSMClusteredMap';
import GoogleClusteredMap from 'controls/Maps/Google/GoogleClusteredMap';
import { getUnitStatus } from 'layouts/dashboard/utils/dashboard';
import { getStatusMarkerColor, getStatusMarkerLabel } from 'controls/Maps/utils/markerUtils';
import type { MapMarker, MapPoi } from 'controls/Maps/core/mapTypes';
import type { Position } from 'api/router/router';
import type { Geofence } from 'api/geofencing/geofencing';
import type { PointOfInterest } from 'api/manager/pointsOfInterest';
import { MAP_CONTAINER_CLASS } from 'controls/Maps/core/mapConfig';
import 'controls/Maps/css/map.css';

/** A trail point (client-side ring buffer entry) for the selected unit. */
export interface TrailPoint { lat: number; lng: number; dateTime: string; }

interface GeneralMapProps {
  mapType: 'OSM' | 'Google';
  positions: Position[];
  mapKey?: string | null;
  selectedMarker?: string | null;
  geofences?: Geofence[];
  showGeofence?: boolean;
  handleSelected?: (value: string | null) => void;
  onlineInterval?: number;
  pois?: PointOfInterest[];
  showPois?: boolean;
  trail?: TrailPoint[];
  showTrail?: boolean;
  followUnit?: string | null;
  onFollowDisengage?: () => void;
  darkMode?: boolean;
  viewportThreshold?: number;
  height?: string;
}

function GeneralMap({
    mapType,
    positions,
    mapKey,
    selectedMarker,
    geofences,
    showGeofence,
    handleSelected,
    onlineInterval,
    pois = [],
    showPois = false,
    trail = [],
    showTrail = false,
    followUnit = null,
    onFollowDisengage,
    darkMode = false,
    viewportThreshold = 1000,
    height = "70vh" }: GeneralMapProps) {

    const [markers, setMarkers] = useState<MapMarker[]>([]);
    useEffect(() => {
        const fetchMarkers = async () => {
            const markers: MapMarker[] = positions.map(item => {
                const status = getUnitStatus(item, onlineInterval as number);
                return {
                    id: item.transporterId,
                    lat: item.latitude,
                    lng: item.longitude,
                    rotation: item.course ?? 0,
                    name: item.deviceName,
                    dateTime: item.deviceDateTime,
                    speed: item.speed,
                    color: getStatusMarkerColor(status),
                    text: getStatusMarkerLabel(status),
                    // Additional attributes
                    attributes: item.attributes || {},
                    address: item.address,
                    altitude: item.altitude,
                    city: item.city,
                    country: item.country,
                    state: item.state,
                    transporterType: item.transporterType,
                };
            });
            setMarkers(markers);
        };
        fetchMarkers();
      }, [positions, onlineInterval]);

    return (
        <div className={MAP_CONTAINER_CLASS}>
            {mapType === 'OSM' ? (
                <OSMClusteredMap
                    markers={markers}
                    selectedMarker={selectedMarker}
                    geofences={geofences}
                    showGeofence={showGeofence}
                    handleSelected={handleSelected}
                    pois={pois as MapPoi[]}
                    showPois={showPois}
                    trail={trail}
                    showTrail={showTrail}
                    followUnit={followUnit}
                    onFollowDisengage={onFollowDisengage}
                    darkMode={darkMode}
                    viewportThreshold={viewportThreshold}
                    height={height} />
            ) : (
                mapType === 'Google' &&
                    <GoogleClusteredMap
                        markers={markers}
                        mapKey={mapKey ?? undefined}
                        selectedMarker={selectedMarker}
                        geofences={geofences}
                        showGeofence={showGeofence}
                        handleSelected={handleSelected}
                        pois={pois as MapPoi[]}
                        showPois={showPois}
                        trail={trail}
                        showTrail={showTrail}
                        followUnit={followUnit}
                        onFollowDisengage={onFollowDisengage}
                        darkMode={darkMode}
                        viewportThreshold={viewportThreshold}
                        height={height} />
            )}
        </div>
    );
}

export default GeneralMap;
