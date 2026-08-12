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
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import { calculatePathDistance, formatDistance } from 'controls/Maps/utils/measurementUtils';
import { useTranslation } from 'react-i18next';

type ControlCorner = 'topleft' | 'topright' | 'bottomleft' | 'bottomright';
type MeasurementUnit = 'metric' | 'imperial';

interface MeasurementToolProps {
    position?: ControlCorner;
    unit?: MeasurementUnit;
    enabled?: boolean;
}

const MeasurementTool = ({ position = 'topright', unit = 'metric', enabled = true }: MeasurementToolProps) => {
    const { t } = useTranslation();
    const map = useMap();
    const measureLayerRef = useRef<L.LayerGroup | null>(null);
    const pointsRef = useRef<{ lat: number; lng: number }[]>([]);
    const markersRef = useRef<L.CircleMarker[]>([]);
    const buttonRef = useRef<HTMLAnchorElement | null>(null);
    const [isActive, setIsActive] = useState(false);

    useEffect(() => {
        if (!map || !enabled) return;

        const handleMapClick = (e: L.LeafletMouseEvent) => {
            const latlng = e.latlng;
            pointsRef.current.push({ lat: latlng.lat, lng: latlng.lng });

            // Add marker
            const marker = L.circleMarker(latlng, {
                radius: 5,
                color: '#ff0000',
                fillColor: '#ff0000',
                fillOpacity: 1
            });
            markersRef.current.push(marker);
            measureLayerRef.current!.addLayer(marker);

            // Draw line if more than one point
            if (pointsRef.current.length > 1) {
                const polyline = L.polyline(
                    pointsRef.current.map(p => [p.lat, p.lng] as [number, number]),
                    { color: '#ff0000', weight: 2, dashArray: '5, 5' }
                );
                measureLayerRef.current!.addLayer(polyline);

                // Calculate and show distance
                const distance = calculatePathDistance(pointsRef.current);
                const formattedDistance = formatDistance(distance, unit);

                L.popup()
                    .setLatLng(latlng)
                    .setContent(`Total: ${formattedDistance}`)
                    .openOn(map);
            }
        };

        const clearMeasurement = () => {
            if (measureLayerRef.current) {
                measureLayerRef.current.clearLayers();
            }
            pointsRef.current = [];
            markersRef.current = [];
            map.getContainer().style.cursor = '';
            map.off('click', handleMapClick);
            map.closePopup();
        };

        const toggleMeasurement = () => {
            setIsActive(prev => !prev);
        };

        // Create custom control
        const MeasureControl = L.Control.extend({
            options: {
                position: position
            },
            onAdd: function () {
                const container = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
                container.style.background = 'white';
                container.style.cursor = 'pointer';
                container.style.display = 'flex';
                container.style.alignItems = 'center';
                container.style.justifyContent = 'center';
                container.style.height = '40px';
                container.style.width = '40px';

                const button = L.DomUtil.create('a', '', container) as HTMLAnchorElement;
                button.innerHTML = '📏';
                button.title = t('utilsmap.measureDistance');
                button.style.display = 'flex';
                button.style.alignItems = 'center';
                button.style.justifyContent = 'center';
                button.style.width = '100%';
                button.style.height = '100%';
                button.style.fontSize = '20px';
                button.style.textDecoration = 'none';
                button.style.color = '#000';
                button.style.background = 'transparent';
                button.style.border = 'none';
                button.style.boxShadow = 'none';

                buttonRef.current = button;

                L.DomEvent.on(button, 'click', function (e) {
                    L.DomEvent.stopPropagation(e);
                    L.DomEvent.preventDefault(e);
                    toggleMeasurement();
                });

                return container;
            }
        });

        const measureControl: L.Control = new MeasureControl();
        measureControl.addTo(map);

        return () => {
            clearMeasurement();
            measureControl.remove();
        };
    }, [map, position, enabled]);

    // Handle activation state changes
    useEffect(() => {
        if (!map || !enabled) return;

        const handleMapClick = (e: L.LeafletMouseEvent) => {
            const latlng = e.latlng;
            pointsRef.current.push({ lat: latlng.lat, lng: latlng.lng });

            // Add marker
            const marker = L.circleMarker(latlng, {
                radius: 5,
                color: '#ff0000',
                fillColor: '#ff0000',
                fillOpacity: 1
            });
            markersRef.current.push(marker);

            if (!measureLayerRef.current) {
                measureLayerRef.current = L.layerGroup().addTo(map);
            }
            measureLayerRef.current.addLayer(marker);

            // Draw line if more than one point
            if (pointsRef.current.length > 1) {
                const polyline = L.polyline(
                    pointsRef.current.map(p => [p.lat, p.lng] as [number, number]),
                    { color: '#ff0000', weight: 2, dashArray: '5, 5' }
                );
                measureLayerRef.current.addLayer(polyline);

                // Calculate and show distance
                const distance = calculatePathDistance(pointsRef.current);
                const formattedDistance = formatDistance(distance, unit);

                L.popup()
                    .setLatLng(latlng)
                    .setContent(`Total: ${formattedDistance}`)
                    .openOn(map);
            }
        };

        if (isActive) {
            // Start measuring
            pointsRef.current = [];
            markersRef.current = [];

            if (!measureLayerRef.current) {
                measureLayerRef.current = L.layerGroup().addTo(map);
            }

            map.getContainer().style.cursor = 'crosshair';
            map.on('click', handleMapClick);

            if (buttonRef.current) {
                buttonRef.current.style.background = '#e0e0e0';
            }
        } else {
            // Stop measuring
            if (measureLayerRef.current) {
                measureLayerRef.current.clearLayers();
            }
            pointsRef.current = [];
            markersRef.current = [];
            map.getContainer().style.cursor = '';
            map.off('click', handleMapClick);
            map.closePopup();

            if (buttonRef.current) {
                buttonRef.current.style.background = '';
            }
        }

        return () => {
            map.off('click', handleMapClick);
        };
    }, [isActive, map, unit, enabled]);

    return null;
};

export default MeasurementTool;
