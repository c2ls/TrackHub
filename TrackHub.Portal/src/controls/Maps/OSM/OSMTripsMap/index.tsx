import { MAP_DEFAULTS } from 'api/core/endpoints';
import { Fragment, useEffect, useRef, useState } from 'react';
import { MapContainer, TileLayer, Polyline, Marker } from 'react-leaflet';
import UserLocation from "controls/Maps/UserLocation";
import GeofencePolygon from 'controls/Maps/OSM/GeofencePolygon';
import { OSMScaleControl } from 'controls/Maps/shared/ScaleControl';
import { OSMFullscreenControl } from 'controls/Maps/shared/FullscreenControl';
import { OSMMeasurementTool } from 'controls/Maps/shared/MeasurementTool';
import { OSMStatsToggle } from 'controls/Maps/shared/StatsToggle';
import MapProviderContext, { OSM_PROVIDER } from 'controls/Maps/core/MapProviderContext';
import PlaybackMarker from 'controls/Maps/core/PlaybackMarker';
import { OSM_LIGHT_TILE, OSM_DARK_TILE } from 'controls/Maps/utils/darkMapStyles';
import 'leaflet/dist/leaflet.css';
import startIconUrl from 'assets/images/markers/start_marker.svg';
import endIconUrl from 'assets/images/markers/end_marker.svg';
import singleIconUrl from 'assets/images/markers/single_marker.svg';
import L from 'leaflet';
import type { MapTrip, MapGeofence, PlaybackPosition } from 'controls/Maps/core/mapTypes';

const startIcon = new L.Icon({ iconUrl: startIconUrl, iconSize: [20, 20] });
const endIcon = new L.Icon({ iconUrl: endIconUrl, iconSize: [20, 20] });

interface OSMTripsMapProps {
  trips?: MapTrip[];
  selectedTrip?: string | null;
  showGeofence?: boolean;
  geofences?: MapGeofence[];
  handleSelected: (tripId: string) => void;
  toggleStats?: () => void;
  showStats?: boolean;
  enableScale?: boolean;
  enableFullscreen?: boolean;
  enableMeasurement?: boolean;
  playbackPosition?: PlaybackPosition | null;
  darkMode?: boolean;
  height?: string;
}

const OSMTripsMap = ({
  trips = [],
  selectedTrip,
  showGeofence,
  geofences = [],
  handleSelected,
  toggleStats,
  showStats,
  enableScale = true,
  enableFullscreen = true,
  enableMeasurement = true,
  playbackPosition = null,
  darkMode = false,
  height = "70vh"
}: OSMTripsMapProps) => {
  const mapRef = useRef<L.Map | null>(null);
  const [userLocation, setUserLocation] = useState({
    lat: MAP_DEFAULTS.lat,
    lng: MAP_DEFAULTS.lng
  });

  const handleClick = (tripId: string) => {
    if (selectedTrip !== tripId) {
      handleSelected(tripId);
    }
  };

  const getPolylineStyle = (tripId: string, currentcolor?: string): L.PathOptions => {
    return {
      color: tripId === selectedTrip ? 'red' : currentcolor,
      weight: tripId === selectedTrip ? 6 : 4,
    };
  };

  const getMarkerStyle = (tripId: string): L.Icon => {
    const size = tripId === selectedTrip ? 30 : 20;
    return new L.Icon({ iconUrl: singleIconUrl, iconSize: [size, size] });
  };

  // Helper function to calculate the bounding box of an array of coordinates
  const calculateBoundingBox = (coordinates: [number, number][]): [number, number][] => {
    const latitudes = coordinates.map(coord => coord[0]);
    const longitudes = coordinates.map(coord => coord[1]);
    const southWest: [number, number] = [Math.min(...latitudes), Math.min(...longitudes)];
    const northEast: [number, number] = [Math.max(...latitudes), Math.max(...longitudes)];
    return [southWest, northEast];
  };

  // Function to calculate the bounding box of all trips
  const calculateBounds = (): [number, number][] => {
    if (trips.length === 0) {
      return [[4.624335, -74.063644], [4.624335, -74.063644]];
    }
    const allCoords = trips.flatMap(trip => trip.coordinates);
    return calculateBoundingBox(allCoords);
  };

  // Function to calculate the bounding box of a specific trip
  const calculateTripBounds = (trip: MapTrip): [number, number][] => {
    return calculateBoundingBox(trip.coordinates);
  };

  // Center in selected polygon
  useEffect(() => {
    if (mapRef.current && selectedTrip) {
      const map = mapRef.current;
      const selectedTripData = trips.find(trip => trip.id === selectedTrip);
      if (selectedTripData) {
        const bounds = calculateTripBounds(selectedTripData);
        map.fitBounds(bounds);
      }
    }
  }, [selectedTrip, trips]);

  // Center the map to the bounding box of all trips
  useEffect(() => {
    if (mapRef.current) {
      const map = mapRef.current;
      const bounds = calculateBounds();
      map.fitBounds(bounds);
    }
  }, [trips]);

  const tile = darkMode ? OSM_DARK_TILE : OSM_LIGHT_TILE;

  return (
    <div>
      <UserLocation setUserLocation={setUserLocation} />
      <MapProviderContext.Provider value={OSM_PROVIDER}>
      <MapContainer ref={mapRef} center={userLocation} zoom={13} style={{ height: height, width: '100%' }}>
        <TileLayer
          key={darkMode ? 'dark' : 'light'}
          url={tile.url}
          attribution={tile.attribution}
          className={tile.className}
        />
        {trips.map((trip, index) => {
          return (
            <Fragment key={index}>
              {trip.type === 1
                ? (<Marker
                    position={trip.coordinates[0]}
                    icon={getMarkerStyle(trip.id)}
                    eventHandlers={{ click: () => {
                      handleClick(trip.id);
                      mapRef.current!.setView(trip.coordinates[0], 13);
                    }}}/>)
                : (
                  <Polyline
                    key={trip.id}
                    positions={trip.coordinates}
                    pathOptions={getPolylineStyle(trip.id, trip.color)}
                    eventHandlers={{ click: () => handleClick(trip.id) }}
                  />
              )}
            </Fragment>
          );
        })}
        {trips.length > 0 && trips.some(trip => trip.coordinates.length > 1) && (
          <>
            <Marker position={trips.find(trip => trip.coordinates.length > 1)!.coordinates[0]} icon={startIcon}/>
            <Marker position={trips[trips.length - 1].coordinates[trips[trips.length - 1].coordinates.length - 1]} icon={endIcon}/>
          </>
        )}
        {showGeofence && geofences.map((geofence, index) => (
          <GeofencePolygon key={index} geofence={geofence} />
        ))}
        {playbackPosition && <PlaybackMarker position={playbackPosition} />}
        {enableScale && <OSMScaleControl position="bottomleft" imperial={false} />}
        {enableFullscreen && <OSMFullscreenControl position="topleft" />}
        {enableMeasurement && <OSMMeasurementTool position="topleft" unit="metric" enabled={true} />}
        <OSMStatsToggle position="topleft" toggleStats={toggleStats} showStats={showStats} />
      </MapContainer>
      </MapProviderContext.Provider>
    </div>
  );
};

export default OSMTripsMap;
