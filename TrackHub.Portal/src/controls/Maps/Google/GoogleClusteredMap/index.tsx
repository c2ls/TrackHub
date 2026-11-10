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

import { MAP_DEFAULTS } from 'api/core/endpoints';
import { useState, useRef, useEffect, useMemo, useCallback } from 'react';
import type { ComponentProps } from 'react';
import { GoogleMap, LoadScript, Marker as GoogleMarker, MarkerClusterer } from '@react-google-maps/api';
import { createUnitIcon, CLUSTER_ICON_URL } from 'controls/Maps/utils/imageUtils';
import { formatDateTime } from "utils/dateUtils";
import UserLocation from "controls/Maps/UserLocation";
import GeofencePolygon from 'controls/Maps/Google/GeofencePolygon';
import { GoogleScaleControl } from 'controls/Maps/shared/ScaleControl';
import { GoogleFullscreenControl } from 'controls/Maps/shared/FullscreenControl';
import { GoogleMeasurementTool } from 'controls/Maps/shared/MeasurementTool';
import MapProviderContext, { GOOGLE_PROVIDER } from 'controls/Maps/core/MapProviderContext';
import PoiLayer from 'controls/Maps/core/PoiLayer';
import TrailLayer from 'controls/Maps/core/TrailLayer';
import { GOOGLE_NIGHT_STYLES } from 'controls/Maps/utils/darkMapStyles';
import { DEFAULT_GOOGLE_ZOOM } from 'controls/Maps/core/mapConfig';
import type { MapMarker, MapGeofence, MapPoi, TrailPoint } from 'controls/Maps/core/mapTypes';
import 'controls/Maps/css/markerCluster.css';

const ANIMATION_DURATION_MS = 1000;
// Above this rendered-marker count position changes are applied without animation.
const MAX_ANIMATED_MARKERS = 300;

const CLUSTER_OPTIONS = {
    styles: [{
        url: CLUSTER_ICON_URL,
        width: 42,
        height: 42,
        textColor: '#0F1B2A',
        textSize: 12,
        fontWeight: '700',
        fontFamily: 'Inter, Helvetica, Arial, sans-serif',
    }],
};

// The base map's own place and transit pins are hidden here: beside them a unit marker reads as
// just another point of interest.
const QUIET_BASE_MAP: google.maps.MapTypeStyle[] = [
    { featureType: 'poi', elementType: 'labels', stylers: [{ visibility: 'off' }] },
    { featureType: 'transit', elementType: 'labels.icon', stylers: [{ visibility: 'off' }] },
];

const markerId = (marker: MapMarker): string => marker.id ?? marker.name;

const easeInOut = (x: number): number => (x < 0.5 ? 2 * x * x : 1 - Math.pow(-2 * x + 2, 2) / 2);

type MarkerClustererArg = ComponentProps<typeof GoogleMarker>['clusterer'];

interface AnimatedGoogleMarkerProps {
    data: MapMarker;
    clusterer?: MarkerClustererArg;
    animate?: boolean;
    title?: string;
    icon?: google.maps.Icon;
    label?: google.maps.MarkerLabel;
    zIndex?: number;
    onClick?: () => void;
}

// Keeps the google.maps.Marker instance (stable key) and animates position
// changes from the previous to the new coordinate instead of teleporting.
const AnimatedGoogleMarker = ({ data, clusterer, animate, title, icon, label, zIndex, onClick }: AnimatedGoogleMarkerProps) => {
    const [position, setPosition] = useState<google.maps.LatLngLiteral>({ lat: data.lat, lng: data.lng });
    const positionRef = useRef<google.maps.LatLngLiteral>({ lat: data.lat, lng: data.lng });
    const frameRef = useRef<number | null>(null);

    useEffect(() => {
        const target = { lat: data.lat, lng: data.lng };
        if (frameRef.current) {
            cancelAnimationFrame(frameRef.current);
            frameRef.current = null;
        }
        const from = positionRef.current;
        if (from.lat === target.lat && from.lng === target.lng) return undefined;
        if (!animate) {
            positionRef.current = target;
            setPosition(target);
            return undefined;
        }
        const start = performance.now();
        const step = (now: number) => {
            const progress = Math.min((now - start) / ANIMATION_DURATION_MS, 1);
            const k = easeInOut(progress);
            const next = {
                lat: from.lat + (target.lat - from.lat) * k,
                lng: from.lng + (target.lng - from.lng) * k
            };
            positionRef.current = next;
            setPosition(next);
            if (progress < 1) {
                frameRef.current = requestAnimationFrame(step);
            } else {
                frameRef.current = null;
            }
        };
        frameRef.current = requestAnimationFrame(step);
        return () => {
            if (frameRef.current) {
                cancelAnimationFrame(frameRef.current);
                frameRef.current = null;
            }
        };
    }, [data.lat, data.lng, animate]);

    return (
        <GoogleMarker
            position={position}
            clusterer={clusterer}
            onClick={onClick}
            title={title}
            icon={icon}
            label={label}
            zIndex={zIndex} />
    );
};

interface GoogleClusteredMapProps {
    mapKey?: string;
    markers: MapMarker[];
    selectedMarker?: string | null;
    handleSelected?: (value: string | null) => void;
    showGeofence?: boolean;
    geofences?: MapGeofence[];
    enableScale?: boolean;
    enableFullscreen?: boolean;
    enableMeasurement?: boolean;
    pois?: MapPoi[];
    showPois?: boolean;
    trail?: TrailPoint[];
    showTrail?: boolean;
    followUnit?: string | null;
    onFollowDisengage?: () => void;
    darkMode?: boolean;
    viewportThreshold?: number;
    height?: string;
}

const GoogleClusteredMap = ({
    mapKey,
    markers,
    selectedMarker,
    handleSelected,
    showGeofence,
    geofences = [],
    enableScale = true,
    enableFullscreen = true,
    enableMeasurement = true,
    pois = [],
    showPois = false,
    trail = [],
    showTrail = false,
    followUnit = null,
    onFollowDisengage,
    darkMode = false,
    viewportThreshold = 1000,
    height = "70vh"
}: GoogleClusteredMapProps) => {
    const [userLocation, setUserLocation] = useState({
        lat: MAP_DEFAULTS.lat,
        lng: MAP_DEFAULTS.lng
    });
    const [mapLoaded, setMapLoaded] = useState(false);
    const [viewBounds, setViewBounds] = useState<google.maps.LatLngBounds | null>(null);
    const mapRef = useRef<google.maps.Map | null>(null);
    const markersRef = useRef<MapMarker[]>(markers);
    const boundsFittedRef = useRef(false);

    useEffect(() => {
        markersRef.current = markers;
    }, [markers]);

    useEffect(() => {
        if (!selectedMarker || !mapRef.current) return;
        const marker = markers.find(m => m.name === selectedMarker);
        if (marker) {
            mapRef.current.setCenter({ lat: marker.lat, lng: marker.lng });
            mapRef.current.setZoom(16);
        }
        // Center on selection change only; refresh cycles must not recenter.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedMarker]);

    // Fit bounds once when data first arrives; refreshes keep the viewport.
    useEffect(() => {
        if (mapLoaded && mapRef.current && markers.length > 0 && !boundsFittedRef.current) {
            const bounds = new window.google.maps.LatLngBounds();
            markers.forEach(marker => {
                bounds.extend({ lat: marker.lat, lng: marker.lng });
            });
            mapRef.current.fitBounds(bounds);
            boundsFittedRef.current = true;
        }
    }, [markers, mapLoaded]);

    // Follow mode: keep the followed unit centered on each refresh.
    useEffect(() => {
        if (!followUnit || !mapLoaded || !mapRef.current) return;
        const marker = markers.find(m => m.name === followUnit);
        if (marker) {
            mapRef.current.panTo({ lat: marker.lat, lng: marker.lng });
        }
    }, [followUnit, markers, mapLoaded]);

    useEffect(() => {
        if (!followUnit || !mapLoaded || !mapRef.current || !onFollowDisengage) return undefined;
        const listener = mapRef.current.addListener('dragstart', () => onFollowDisengage());
        return () => {
            listener.remove();
        };
    }, [followUnit, mapLoaded, onFollowDisengage]);

    const handleIdle = useCallback(() => {
        if (mapRef.current && markersRef.current.length > viewportThreshold) {
            setViewBounds(mapRef.current.getBounds() ?? null);
        }
    }, [viewportThreshold]);

    // Render only viewport-relevant markers above the threshold; stat cards
    // and the transporters table keep using the full authorized set.
    const visibleMarkers = useMemo(() => {
        if (markers.length <= viewportThreshold) return markers;
        if (!viewBounds) return markers.slice(0, viewportThreshold);
        return markers.filter(marker =>
            marker.name === selectedMarker || viewBounds.contains({ lat: marker.lat, lng: marker.lng }));
    }, [markers, viewportThreshold, viewBounds, selectedMarker]);

    const animate = visibleMarkers.length <= MAX_ANIMATED_MARKERS;

    // The same controls the OSM map shows, in the same corner; the page provides its own
    // fullscreen button, and Street View opens from the unit card.
    const mapOptions = useMemo<google.maps.MapOptions>(() => ({
        gestureHandling: "greedy",
        styles: darkMode ? [...GOOGLE_NIGHT_STYLES, ...QUIET_BASE_MAP] : QUIET_BASE_MAP,
        zoomControl: true,
        zoomControlOptions: mapLoaded ? { position: window.google.maps.ControlPosition.RIGHT_TOP } : undefined,
        cameraControl: false,
        mapTypeControl: false,
        streetViewControl: false,
        fullscreenControl: false,
    }), [darkMode, mapLoaded]);

    return (
        <div data-selected-marker={selectedMarker ?? ""} style={{ height, width: "100%" }}>
        <LoadScript googleMapsApiKey={mapKey ?? ''}>
            <UserLocation setUserLocation={setUserLocation} />
            <MapProviderContext.Provider value={GOOGLE_PROVIDER}>
                <GoogleMap mapContainerStyle={{ height: "100%", width: "100%" }}
                    zoom={DEFAULT_GOOGLE_ZOOM}
                    center={userLocation}
                    onLoad={map => {
                        mapRef.current = map;
                        setMapLoaded(true);
                    }}
                    onIdle={handleIdle}
                    options={mapOptions}>
                    <MarkerClusterer options={CLUSTER_OPTIONS}>
                        {(clusterer) =>
                            <>
                            {visibleMarkers.map((marker) => {
                                const selected = marker.name === selectedMarker;
                                return (
                                    <AnimatedGoogleMarker
                                        key={markerId(marker)}
                                        data={marker}
                                        clusterer={clusterer}
                                        animate={animate}
                                        title={`${marker.name} · ${marker.speed} km/h · ${formatDateTime(marker.dateTime)}`}
                                        zIndex={selected ? 1000 : undefined}
                                        onClick={() => handleSelected?.(marker.name)}
                                        icon={{
                                            url: createUnitIcon(marker.status ?? 'offline', marker.rotation, selected, 'dataURL'),
                                            scaledSize: new window.google.maps.Size(48, 48),
                                            anchor: new window.google.maps.Point(24, 24),
                                            labelOrigin: new window.google.maps.Point(24, 52),
                                        }}
                                        label={{ text: marker.name, className: 'th-unit-label', color: '#0F1B2A', fontSize: '11px', fontWeight: '600' }} />
                                );
                            })}
                            </>
                        }
                    </MarkerClusterer>
                    {showGeofence && geofences.map((geofence, index) => (
                        <GeofencePolygon key={index} geofence={geofence} />
                    ))}
                    {showPois && <PoiLayer pois={pois} />}
                    {showTrail && trail.length > 1 && <TrailLayer points={trail} />}
                    {enableScale && <GoogleScaleControl mapRef={mapRef} />}
                    {enableFullscreen && <GoogleFullscreenControl mapRef={mapRef} position="RIGHT_TOP" />}
                    {enableMeasurement && <GoogleMeasurementTool mapRef={mapRef} position="RIGHT_TOP" unit="metric" enabled={true} />}
                </GoogleMap>
            </MapProviderContext.Provider>
        </LoadScript>
        </div>
    );
};

export default GoogleClusteredMap;
