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
import type { ComponentProps, ReactNode } from 'react';
import { GoogleMap, LoadScript, Marker as GoogleMarker, MarkerClusterer, InfoWindow } from '@react-google-maps/api';
import type { TFunction } from 'i18next';
import { createSvgIcon } from 'controls/Maps/utils/imageUtils';
import { formatDateTime } from "utils/dateUtils";
import { getWhatsAppShareUrl, getStreetViewUrl, getRelativeTimeText } from 'controls/Maps/utils/popupUtils';
import { useTranslation } from 'react-i18next';
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
import { reverseGeocode } from 'api/router/router';
import type { Address } from 'api/router/router';
import type { MapMarker, MapGeofence, MapPoi, TrailPoint } from 'controls/Maps/core/mapTypes';
import 'controls/Maps/css/googleMap.css';

const ANIMATION_DURATION_MS = 1000;
// Above this rendered-marker count position changes are applied without animation.
const MAX_ANIMATED_MARKERS = 300;

const markerId = (marker: MapMarker): string => marker.id ?? marker.name;

const easeInOut = (x: number): number => (x < 0.5 ? 2 * x * x : 1 - Math.pow(-2 * x + 2, 2) / 2);

type MarkerClustererArg = ComponentProps<typeof GoogleMarker>['clusterer'];

interface AnimatedGoogleMarkerProps {
    data: MapMarker;
    clusterer?: MarkerClustererArg;
    animate?: boolean;
    title?: string;
    icon?: google.maps.Icon;
    onClick?: () => void;
    children?: ReactNode | ((position: google.maps.LatLngLiteral) => ReactNode);
}

// Keeps the google.maps.Marker instance (stable key) and animates position
// changes from the previous to the new coordinate instead of teleporting.
const AnimatedGoogleMarker = ({ data, clusterer, animate, title, icon, onClick, children }: AnimatedGoogleMarkerProps) => {
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
            icon={icon}>
            {typeof children === 'function' ? children(position) : children}
        </GoogleMarker>
    );
};

type ResolveState = 'pending' | 'done' | 'failed' | undefined;

interface MarkerInfoContentProps {
    marker: MapMarker;
    t: TFunction;
    onResolveAddress?: (marker: MapMarker) => void;
    resolveState?: ResolveState;
}

const MarkerInfoContent = ({ marker, t, onResolveAddress, resolveState }: MarkerInfoContentProps) => {
    const relative = getRelativeTimeText(marker.dateTime, t);
    return (
        <div style={{ minWidth: '240px', fontFamily: 'Arial, sans-serif', fontSize: '13px' }}>
            <div style={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', color: 'white', padding: '8px', margin: '-8px -8px 6px -8px', borderRadius: '3px 3px 0 0' }}>
                <h3 style={{ margin: 0, fontSize: '15px', fontWeight: 'bold' }}>{marker.name}</h3>
                {marker.transporterType && <p style={{ margin: '2px 0 0 0', fontSize: '12px', opacity: 0.9 }}>{marker.transporterType}</p>}
            </div>

            <div style={{ padding: '0' }}>
                <div style={{ marginBottom: '4px' }}>
                    <strong style={{ color: '#667eea', fontSize: '12px' }}>⏱ {t('transporterMap.lastReport') || t('transporterMap.dateTime')}:</strong>{' '}
                    <span style={{ fontSize: '12px', color: '#333' }}>{formatDateTime(marker.dateTime)}</span>
                    {relative && <span style={{ fontSize: '11px', color: '#888' }}> ({relative})</span>}
                </div>

                <div style={{ marginBottom: '4px' }}>
                    <strong style={{ color: '#667eea', fontSize: '12px' }}>🚗 {t('transporterMap.speed')}:</strong>{' '}
                    <span style={{ fontSize: '13px', fontWeight: 'bold', color: marker.speed > 0 ? '#10b981' : '#ef4444' }}>{marker.speed} km/h</span>
                    <span style={{ marginLeft: '4px', padding: '2px 5px', background: marker.speed > 0 ? '#d1fae5' : '#fee2e2', color: marker.speed > 0 ? '#065f46' : '#991b1b', borderRadius: '2px', fontSize: '10px' }}>
                        {marker.speed > 0 ? (t('transporterMap.moving') || 'Moving') : (t('transporterMap.stopped') || 'Stopped')}
                    </span>
                </div>

                {marker.attributes?.mileage !== undefined && marker.attributes?.mileage !== null && (
                    <div style={{ marginBottom: '4px' }}>
                        <strong style={{ color: '#667eea', fontSize: '12px' }}>📏 {t('transporterMap.mileage') || 'Mileage'}:</strong>{' '}
                        <span style={{ fontSize: '12px', color: '#333' }}>{(marker.attributes.mileage / 1000).toFixed(2)} km</span>
                    </div>
                )}

                {marker.attributes?.temperature !== undefined && marker.attributes?.temperature !== null && (
                    <div style={{ marginBottom: '4px' }}>
                        <strong style={{ color: '#667eea', fontSize: '12px' }}>🌡 {t('transporterMap.temperature') || 'Temperature'}:</strong>{' '}
                        <span style={{ fontSize: '12px', color: '#333' }}>{marker.attributes.temperature}°C</span>
                    </div>
                )}

                {marker.attributes?.hourmeter !== undefined && marker.attributes?.hourmeter !== null && (
                    <div style={{ marginBottom: '4px' }}>
                        <strong style={{ color: '#667eea', fontSize: '12px' }}>⏲ {t('transporterMap.hourmeter') || 'Hourmeter'}:</strong>{' '}
                        <span style={{ fontSize: '12px', color: '#333' }}>{Number(marker.attributes.hourmeter).toFixed(1)} hrs</span>
                    </div>
                )}

                {marker.attributes?.ignition !== undefined && marker.attributes?.ignition !== null && (
                    <div style={{ marginBottom: '4px' }}>
                        <strong style={{ color: '#667eea', fontSize: '12px' }}>🔑 {t('transporterMap.accStatus') || 'Ignition'}:</strong>{' '}
                        <span style={{ color: marker.attributes.ignition ? '#10b981' : '#ef4444', fontSize: '12px', fontWeight: 'bold' }}>
                            {marker.attributes.ignition ? (t('transporterMap.accOn') || 'ON') : (t('transporterMap.accOff') || 'OFF')}
                        </span>
                    </div>
                )}

                {marker.attributes?.satellites !== undefined && marker.attributes?.satellites !== null && (
                    <div style={{ marginBottom: '4px' }}>
                        <strong style={{ color: '#667eea', fontSize: '12px' }}>🛰 {t('transporterMap.satellites') || 'Satellites'}:</strong>{' '}
                        <span style={{ fontSize: '12px', color: '#333' }}>{marker.attributes.satellites}</span>
                    </div>
                )}

                {marker.altitude !== undefined && marker.altitude !== null && (
                    <div style={{ marginBottom: '4px' }}>
                        <strong style={{ color: '#667eea', fontSize: '12px' }}>⛰ {t('transporterMap.altitude') || 'Altitude'}:</strong>{' '}
                        <span style={{ fontSize: '12px', color: '#333' }}>{marker.altitude} m</span>
                    </div>
                )}

                {marker.address && (
                    <div style={{ marginBottom: '4px' }}>
                        <strong style={{ color: '#667eea', fontSize: '12px' }}>📍 {t('transporterMap.address') || 'Address'}:</strong><br/>
                        <span style={{ fontSize: '11px', color: '#555' }}>{marker.address}</span>
                    </div>
                )}

                {!marker.address && (marker.city || marker.state || marker.country) && (
                    <div style={{ marginBottom: '4px' }}>
                        <strong style={{ color: '#667eea', fontSize: '12px' }}>📍 {t('transporterMap.location') || 'Location'}:</strong><br/>
                        <span style={{ fontSize: '11px', color: '#555' }}>{[marker.city, marker.state, marker.country].filter(Boolean).join(', ')}</span>
                    </div>
                )}

                {!marker.address && (
                    <div style={{ marginBottom: '4px' }}>
                        <strong style={{ color: '#667eea', fontSize: '12px' }}>📍 {t('transporterMap.coordinates') || 'Coordinates'}:</strong>{' '}
                        <span style={{ fontSize: '11px', color: '#555' }}>{Number(marker.lat).toFixed(6)}, {Number(marker.lng).toFixed(6)}</span><br/>
                        <button
                            type="button"
                            disabled={resolveState === 'pending'}
                            onClick={() => onResolveAddress && onResolveAddress(marker)}
                            style={{ marginTop: '4px', padding: '3px 8px', background: '#667eea', color: 'white', border: 'none', borderRadius: '4px', fontSize: '11px', cursor: 'pointer' }}>
                            {resolveState === 'pending'
                                ? (t('transporterMap.resolvingAddress') || 'Resolving...')
                                : resolveState === 'failed'
                                    ? (t('transporterMap.addressUnavailable') || 'Address unavailable')
                                    : (t('transporterMap.resolveAddress') || 'Resolve address')}
                        </button>
                    </div>
                )}
            </div>

            <div style={{ display: 'flex', gap: '4px', marginTop: '8px', paddingTop: '8px', borderTop: '1px solid #e5e7eb' }}>
                <a href={getWhatsAppShareUrl(marker.lat, marker.lng, marker.name)} target="_blank" rel="noopener noreferrer"
                   title={t('transporterMap.shareWhatsApp') || 'Share on WhatsApp'}
                   style={{ display: 'inline-flex', alignItems: 'center', justifyContent: 'center', background: '#25D366', color: 'white', textDecoration: 'none', width: '28px', height: '28px', borderRadius: '4px' }}>
                    <svg style={{ width: '16px', height: '16px', fill: 'white' }} viewBox="0 0 24 24"><path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413Z"/></svg>
                </a>
                <a href={getStreetViewUrl(marker.lat, marker.lng)} target="_blank" rel="noopener noreferrer"
                   title={t('transporterMap.streetView') || 'Street View'}
                   style={{ display: 'inline-flex', alignItems: 'center', justifyContent: 'center', background: '#4285f4', color: 'white', textDecoration: 'none', width: '28px', height: '28px', borderRadius: '4px' }}>
                    <svg style={{ width: '16px', height: '16px', fill: 'white' }} viewBox="0 0 24 24"><path d="M12,11.5A2.5,2.5 0 0,1 9.5,9A2.5,2.5 0 0,1 12,6.5A2.5,2.5 0 0,1 14.5,9A2.5,2.5 0 0,1 12,11.5M12,2A7,7 0 0,0 5,9C5,14.25 12,22 12,22C12,22 19,14.25 19,9A7,7 0 0,0 12,2Z"/></svg>
                </a>
            </div>
        </div>
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
    const [internalSelectedMarker, setInternalSelectedMarker] = useState<MapMarker | null>(null);
    const [userLocation, setUserLocation] = useState({
        lat: MAP_DEFAULTS.lat,
        lng: MAP_DEFAULTS.lng
    });
    const [mapLoaded, setMapLoaded] = useState(false);
    const [viewBounds, setViewBounds] = useState<google.maps.LatLngBounds | null>(null);
    const [resolvedAddresses, setResolvedAddresses] = useState<Record<string, Address>>({});
    const [resolveStates, setResolveStates] = useState<Record<string, ResolveState>>({});
    const { t } = useTranslation();
    const mapRef = useRef<google.maps.Map | null>(null);
    const markersRef = useRef<MapMarker[]>(markers);
    const boundsFittedRef = useRef(false);

    useEffect(() => {
        markersRef.current = markers;
    }, [markers]);

    useEffect(() => {
        if (selectedMarker) {
            const marker = markers.find(m => m.name === selectedMarker);
            if (marker) {
                setInternalSelectedMarker(marker);
                if (mapRef.current) {
                    mapRef.current.setCenter({ lat: marker.lat, lng: marker.lng });
                    mapRef.current.setZoom(16);
                }
            }
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

    const handleResolveAddress = useCallback(async (marker: MapMarker) => {
        const id = markerId(marker);
        setResolveStates(prev => ({ ...prev, [id]: 'pending' }));
        // Silent op: a failed reverse-geocode must not toast, so call the api
        // function directly and swallow the error (the button shows "failed").
        let result: Address | null = null;
        try {
            result = await reverseGeocode(marker.lat, marker.lng, marker.id ?? null);
        } catch (error) {
            if (process.env.NODE_ENV !== 'production') console.error(error);
        }
        if (result && (result.address || result.city || result.state || result.country)) {
            setResolvedAddresses(prev => ({ ...prev, [id]: result! }));
            setResolveStates(prev => ({ ...prev, [id]: 'done' }));
        } else {
            setResolveStates(prev => ({ ...prev, [id]: 'failed' }));
        }
    }, []);

    const mapOptions = useMemo<google.maps.MapOptions>(() => ({
        gestureHandling: "greedy",
        styles: darkMode ? GOOGLE_NIGHT_STYLES : null
    }), [darkMode]);

    return (
        <LoadScript googleMapsApiKey={mapKey ?? ''}>
            <UserLocation setUserLocation={setUserLocation} />
            <MapProviderContext.Provider value={GOOGLE_PROVIDER}>
                <GoogleMap mapContainerStyle={{ height: height, width: "100%" }}
                    zoom={DEFAULT_GOOGLE_ZOOM}
                    center={userLocation}
                    onLoad={map => {
                        mapRef.current = map;
                        setMapLoaded(true);
                    }}
                    onIdle={handleIdle}
                    options={mapOptions}>
                    <MarkerClusterer>
                        {(clusterer) =>
                            <>
                            {visibleMarkers.map((marker) => {
                                const id = markerId(marker);
                                const resolved = resolvedAddresses[id];
                                const displayed = resolved && !marker.address
                                    ? { ...marker, ...resolved }
                                    : marker;
                                return (
                                    <AnimatedGoogleMarker
                                        key={id}
                                        data={marker}
                                        clusterer={clusterer}
                                        animate={animate}
                                        title={`${marker.name} · ${marker.speed} km/h · ${formatDateTime(marker.dateTime)}`}
                                        onClick={() => {
                                            setInternalSelectedMarker(marker);
                                            if (mapRef.current) {
                                                mapRef.current.setCenter({ lat: marker.lat, lng: marker.lng });
                                                mapRef.current.setZoom(16);
                                            }
                                            if (handleSelected) {
                                                handleSelected(marker.name);
                                            }
                                        }}
                                        icon={{
                                            url: createSvgIcon(marker.rotation, marker.text, 'dataURL', marker.color ? { color: marker.color } : {}),
                                            scaledSize: new window.google.maps.Size(50, 50),
                                        }}>
                                        {(position) => (internalSelectedMarker && internalSelectedMarker.name === marker.name ? (
                                            <InfoWindow
                                                position={position}
                                                onCloseClick={() => {
                                                    setInternalSelectedMarker(null);
                                                    if (handleSelected) {
                                                        handleSelected(null);
                                                    }
                                                }}
                                            >
                                                <MarkerInfoContent
                                                    marker={displayed}
                                                    t={t}
                                                    onResolveAddress={handleResolveAddress}
                                                    resolveState={resolveStates[id]} />
                                            </InfoWindow>
                                        ) : null)}
                                    </AnimatedGoogleMarker>
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
                    {enableFullscreen && <GoogleFullscreenControl mapRef={mapRef} position="TOP_LEFT" />}
                    {enableMeasurement && <GoogleMeasurementTool mapRef={mapRef} position="TOP_LEFT" unit="metric" enabled={true} />}
                </GoogleMap>
            </MapProviderContext.Provider>
        </LoadScript>
    );
};

export default GoogleClusteredMap;
