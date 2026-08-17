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

import { useEffect, useRef, useState } from 'react';
import type { RefObject } from 'react';
import { calculatePathDistance, formatDistance } from 'controls/Maps/utils/measurementUtils';
import { useTranslation } from 'react-i18next';

type ControlCorner = 'TOP_LEFT' | 'TOP_RIGHT' | 'BOTTOM_LEFT' | 'BOTTOM_RIGHT';
type MeasurementUnit = 'metric' | 'imperial';

interface MeasurementToolProps {
    mapRef: RefObject<google.maps.Map | null>;
    position?: ControlCorner;
    unit?: MeasurementUnit;
    enabled?: boolean;
}

const MeasurementTool = ({ mapRef, position = 'TOP_RIGHT', unit = 'metric', enabled = true }: MeasurementToolProps) => {
    const { t } = useTranslation();
    const [, setIsActive] = useState(false);
    const pointsRef = useRef<{ lat: number; lng: number }[]>([]);
    const markersRef = useRef<google.maps.Marker[]>([]);
    const polylineRef = useRef<google.maps.Polyline | null>(null);
    const buttonRef = useRef<HTMLButtonElement | null>(null);
    const listenerRef = useRef<google.maps.MapsEventListener | null>(null);
    const infoWindowRef = useRef<google.maps.InfoWindow | null>(null);

    useEffect(() => {
        if (!mapRef || !mapRef.current || !window.google || !enabled) return;

        const map = mapRef.current;

        // Create measurement button
        const measureButton = document.createElement('button');
        measureButton.textContent = '📏';
        measureButton.title = t('utilsmap.measureDistance');
        measureButton.style.cssText = `
            background: white;
            border: 2px solid rgba(0,0,0,.2);
            border-radius: 3px;
            box-shadow: rgba(0,0,0,.3) 0 1px 4px -1px;
            cursor: pointer;
            margin: 10px;
            padding: 0;
            width: 40px;
            height: 40px;
            font-size: 18px;
        `;

        buttonRef.current = measureButton;

        const clearMeasurement = () => {
            // Remove markers
            markersRef.current.forEach(marker => marker.setMap(null));
            markersRef.current = [];

            // Remove polyline
            if (polylineRef.current) {
                polylineRef.current.setMap(null);
                polylineRef.current = null;
            }

            // Close info window
            if (infoWindowRef.current) {
                infoWindowRef.current.close();
                infoWindowRef.current = null;
            }

            // Clear points
            pointsRef.current = [];

            // Reset cursor
            map.setOptions({ draggableCursor: null });

            // Remove listener
            if (listenerRef.current) {
                window.google.maps.event.removeListener(listenerRef.current);
                listenerRef.current = null;
            }
        };

        const startMeasurement = () => {
            pointsRef.current = [];
            markersRef.current = [];
            map.setOptions({ draggableCursor: 'crosshair' });

            listenerRef.current = map.addListener('click', (e: google.maps.MapMouseEvent) => {
                const latlng = { lat: e.latLng!.lat(), lng: e.latLng!.lng() };
                pointsRef.current.push(latlng);

                // Add marker
                const marker = new window.google.maps.Marker({
                    position: latlng,
                    map: map,
                    icon: {
                        path: window.google.maps.SymbolPath.CIRCLE,
                        scale: 5,
                        fillColor: '#ff0000',
                        fillOpacity: 1,
                        strokeColor: '#ff0000',
                        strokeWeight: 2
                    }
                });
                markersRef.current.push(marker);

                // Draw polyline
                if (pointsRef.current.length > 1) {
                    if (polylineRef.current) {
                        polylineRef.current.setMap(null);
                    }

                    polylineRef.current = new window.google.maps.Polyline({
                        path: pointsRef.current,
                        geodesic: true,
                        strokeColor: '#ff0000',
                        strokeOpacity: 1.0,
                        strokeWeight: 2,
                        map: map
                    });

                    // Calculate and show distance
                    const distance = calculatePathDistance(pointsRef.current);
                    const formattedDistance = formatDistance(distance, unit);

                    // Close previous info window
                    if (infoWindowRef.current) {
                        infoWindowRef.current.close();
                    }

                    infoWindowRef.current = new window.google.maps.InfoWindow({
                        content: `<div style="color: #333; font-weight: bold; font-size: 13px;">Total: ${formattedDistance}</div>`,
                        position: latlng
                    });
                    infoWindowRef.current.open(map);
                }
            });
        };

        const toggleMeasurement = () => {
            setIsActive(prev => {
                const newState = !prev;
                if (newState) {
                    startMeasurement();
                    measureButton.style.background = '#e0e0e0';
                } else {
                    clearMeasurement();
                    measureButton.style.background = 'white';
                }
                return newState;
            });
        };

        measureButton.addEventListener('click', toggleMeasurement);
        map.controls[window.google.maps.ControlPosition[position]].push(measureButton);

        return () => {
            clearMeasurement();
            measureButton.removeEventListener('click', toggleMeasurement);
            if (buttonRef.current && buttonRef.current.parentNode) {
                buttonRef.current.parentNode.removeChild(buttonRef.current);
            }
        };
    }, [mapRef, position, unit, enabled]);

    return null;
};

export default MeasurementTool;
