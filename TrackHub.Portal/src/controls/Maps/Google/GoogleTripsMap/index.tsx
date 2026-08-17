import { MAP_DEFAULTS } from 'api/core/endpoints';
import { useRef, useEffect, useState } from 'react';
import UserLocation from "controls/Maps/UserLocation";
import GeofencePolygon from 'controls/Maps/Google/GeofencePolygon';
import { GoogleMap, LoadScript, Polyline, Marker } from '@react-google-maps/api';
import { GoogleScaleControl } from 'controls/Maps/shared/ScaleControl';
import { GoogleFullscreenControl } from 'controls/Maps/shared/FullscreenControl';
import { GoogleMeasurementTool } from 'controls/Maps/shared/MeasurementTool';
import { GoogleStatsToggle } from 'controls/Maps/shared/StatsToggle';
import MapProviderContext, { GOOGLE_PROVIDER } from 'controls/Maps/core/MapProviderContext';
import PlaybackMarker from 'controls/Maps/core/PlaybackMarker';
import { GOOGLE_NIGHT_STYLES } from 'controls/Maps/utils/darkMapStyles';
import type { MapTrip, MapGeofence, PlaybackPosition } from 'controls/Maps/core/mapTypes';
import startMarker from 'assets/images/markers/start_marker.svg';
import endMarker from 'assets/images/markers/end_marker.svg';
import singleMarker from 'assets/images/markers/single_marker.svg';

interface GoogleTripsMapProps {
  mapKey?: string;
  trips: MapTrip[];
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

const GoogleTripsMap = ({
  mapKey,
  trips,
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
}: GoogleTripsMapProps) => {
  const mapRef = useRef<google.maps.Map | null>(null);
  const [userLocation, setUserLocation] = useState({
    lat: MAP_DEFAULTS.lat,
    lng: MAP_DEFAULTS.lng
  });

  const handleClick = (trip: MapTrip) => {
    if (selectedTrip !== trip.id) {
      handleSelected(trip.id);
    }
  };

  const getPolylineOptions = (trip: MapTrip): google.maps.PolylineOptions => {
    return {
      strokeColor: selectedTrip === trip.id ? 'red' : trip.color,
      strokeOpacity: 1,
      strokeWeight: selectedTrip === trip.id ? 6 : 4
    };
  };

    // Center in selected polyline
  useEffect(() => {
    if (mapRef.current && selectedTrip) {
      const selectedTripData = trips.find(trip => trip.id === selectedTrip);
      if (selectedTripData) {
        const bounds = new window.google.maps.LatLngBounds();
        selectedTripData.coordinates.forEach(coord => {
            bounds.extend(new window.google.maps.LatLng(coord[0], coord[1]));
        });
        mapRef.current.fitBounds(bounds);
      }
    }
  }, [selectedTrip, trips]);

  // center the map on the bounds of the trips
  useEffect(() => {
    if (mapRef.current && trips.length > 0) {
      const bounds = new window.google.maps.LatLngBounds();
      trips.forEach(trip => {
        trip.coordinates.forEach(coord => {
          bounds.extend(new window.google.maps.LatLng(coord[0], coord[1]));
        });
      });
      mapRef.current.fitBounds(bounds);
    }
  }, [trips]);

  const getIcon = (type: 'start' | 'end' | 'single', tripId?: string): google.maps.Icon => {
    let url = '';
    const size = tripId === selectedTrip ? 30 : 20;
    switch (type) {
        case 'start': url = startMarker; break;
        case 'end': url = endMarker; break;
        case 'single': url = singleMarker; break;
        default:
          break;
      }
    return {
        url: url,
        scaledSize: new window.google.maps.Size(size, size)
    };
  };

  return (
    <LoadScript googleMapsApiKey={mapKey ?? ''}>
      <UserLocation setUserLocation={setUserLocation} />
      <MapProviderContext.Provider value={GOOGLE_PROVIDER}>
      <GoogleMap
        mapContainerStyle={{ height: height, width: "100%" }}
        zoom={6}
        center={userLocation}
        onLoad={map => { mapRef.current = map; }}
        options={{ gestureHandling: "greedy", styles: darkMode ? GOOGLE_NIGHT_STYLES : null }}>
        {trips.map((trip, index) => (
          trip.type === 1 ? (
            <Marker
              key={index}
              position={{ lat: trip.coordinates[0][0], lng: trip.coordinates[0][1] }}
              icon={getIcon('single', trip.id)}
              onClick={() => handleClick(trip)}
            />
          ) : (
            <Polyline
              key={index}
              path={trip.coordinates.map(coord => ({ lat: coord[0], lng: coord[1] }))}
              options={getPolylineOptions(trip)}
              onClick={() => handleClick(trip)}/>
          )
        ))}
        {trips.length > 0 && trips.some(trip => trip.coordinates.length > 1) && (
          <>
            <Marker
              position={{ lat: trips.find(trip => trip.coordinates.length > 1)!.coordinates[0][0], lng: trips.find(trip => trip.coordinates.length > 1)!.coordinates[0][1] }}
              icon={getIcon('start')}/>
            <Marker
              position={{ lat: trips[trips.length - 1].coordinates[trips[trips.length - 1].coordinates.length - 1][0], lng: trips[trips.length - 1].coordinates[trips[trips.length - 1].coordinates.length - 1][1] }}
              icon={getIcon('end')}/>
          </>
        )}
        {showGeofence && geofences.map((geofence, index) => (
          <GeofencePolygon key={index} geofence={geofence} />
        ))}
        {playbackPosition && <PlaybackMarker position={playbackPosition} />}
        {enableScale && <GoogleScaleControl mapRef={mapRef} />}
        {enableFullscreen && <GoogleFullscreenControl mapRef={mapRef} position="TOP_LEFT" />}
        {enableMeasurement && <GoogleMeasurementTool mapRef={mapRef} position="TOP_LEFT" unit="metric" enabled={true} />}
        <GoogleStatsToggle position="TOP_LEFT" toggleStats={toggleStats} showStats={showStats} />
      </GoogleMap>
      </MapProviderContext.Provider>
    </LoadScript>
  );
};

export default GoogleTripsMap;
