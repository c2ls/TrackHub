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
import OSMTripsMap from 'controls/Maps/OSM/OSMTripsMap';
import GoogleTripsMap from 'controls/Maps/Google/GoogleTripsMap';
import TripStatsPanel from './TripStatsPanel';
import { getRandomColor } from "utils/colorUtils";
import type { Trip } from 'api/router/router';
import type { Geofence } from 'api/geofencing/geofencing';
import type { PlaybackPosition } from 'layouts/dashboard/utils/playback';
import 'controls/Maps/css/map.css';

/** A trip rendered as a polyline on the map. */
interface TripPolygon {
  id: string;
  type: number;
  color: string;
  coordinates: [number, number][];
}

interface TripsMapProps {
  mapType: 'OSM' | 'Google';
  mapKey?: string | null;
  trips: Trip[];
  selectedTrip?: string | null;
  showGeofence?: boolean;
  geofences?: Geofence[];
  handleSelected: (value: string | null) => void;
  playbackPosition?: PlaybackPosition | null;
  darkMode?: boolean;
  height?: string;
}

function TripsMap({
    mapType,
    mapKey,
    trips,
    selectedTrip,
    showGeofence,
    geofences,
    handleSelected,
    playbackPosition = null,
    darkMode = false,
    height = "70vh" }: TripsMapProps) {
    const [polygons, setPolygons] = useState<TripPolygon[]>([]);
    useEffect(() => {
        const fetchTrips = async () => {
            const transformedTrips: TripPolygon[] = trips.map(trip => ({
                id: trip.tripId,
                type: trip.type,
                color: getRandomColor(),
                coordinates: trip.points.map((point): [number, number] => [point.latitude, point.longitude])
            }));
            setPolygons(transformedTrips);
        };
        fetchTrips();
      }, [trips]);

    const [showStats, setShowStats] = useState(true);

    return (
        <div className="map-container" style={{ position: 'relative' }}>
            {mapType === 'OSM' ? (
                <OSMTripsMap
                    trips={polygons}
                    selectedTrip={selectedTrip}
                    showGeofence={showGeofence}
                    geofences={geofences}
                    handleSelected={handleSelected}
                    toggleStats={() => setShowStats(s => !s)}
                    showStats={showStats}
                    playbackPosition={playbackPosition}
                    darkMode={darkMode}
                    height={height} />
            ) : (
                mapType === 'Google' &&
                    <GoogleTripsMap
                        mapKey={mapKey ?? undefined}
                        trips={polygons}
                        selectedTrip={selectedTrip}
                        showGeofence={showGeofence}
                        geofences={geofences}
                        handleSelected={handleSelected}
                        toggleStats={() => setShowStats(s => !s)}
                        showStats={showStats}
                        playbackPosition={playbackPosition}
                        darkMode={darkMode}
                        height={height} />
            )}
            {showStats && <TripStatsPanel trips={trips} selectedTrip={selectedTrip} />}
        </div>
    );
}

export default TripsMap;
