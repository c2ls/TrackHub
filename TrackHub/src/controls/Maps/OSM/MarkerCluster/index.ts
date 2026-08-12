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

import 'leaflet.markercluster/dist/MarkerCluster.css';
import 'leaflet.markercluster/dist/MarkerCluster.Default.css';
import { formatDateTime } from "utils/dateUtils";

import { useCallback, useEffect, useRef } from 'react';
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet.markercluster';
import { useTranslation } from 'react-i18next';
import type { TFunction } from 'i18next';
import { createSvgIcon } from 'controls/Maps/utils/imageUtils';
import { createEnhancedPopupContent, getRelativeTimeText } from 'controls/Maps/utils/popupUtils';
import { reverseGeocode } from 'api/router/router';
import type { MapMarker } from 'controls/Maps/core/mapTypes';

const ANIMATION_DURATION_MS = 1000;
// Above this rendered-marker count position changes are applied without animation.
const MAX_ANIMATED_MARKERS = 500;

interface MarkerEntry {
    marker: L.Marker;
    data: MapMarker;
    animationFrame: number | null;
}

const markerId = (marker: MapMarker): string => marker.id ?? marker.name;

const easeInOut = (x: number): number => (x < 0.5 ? 2 * x * x : 1 - Math.pow(-2 * x + 2, 2) / 2);

const buildIcon = (data: MapMarker): L.DivIcon => L.divIcon({
    className: 'my-icon',
    html: createSvgIcon(data.rotation, data.text, 'svg', data.color ? { color: data.color } : {}),
    iconSize: [28, 30],
    iconAnchor: [14, 30],
    popupAnchor: [0, -30],
});

const buildTooltipContent = (data: MapMarker, t: TFunction): string => `
    <div style="font-size: 12px;">
        <strong>${data.name}</strong><br/>
        ${t('transporterMap.speed')}: ${data.speed} km/h<br/>
        ${t('transporterMap.lastReport')}: ${formatDateTime(data.dateTime)}
        ${getRelativeTimeText(data.dateTime, t) ? `(${getRelativeTimeText(data.dateTime, t)})` : ''}
    </div>`;

const iconChanged = (prev: MapMarker, next: MapMarker): boolean =>
    prev.rotation !== next.rotation || prev.text !== next.text || prev.color !== next.color;

export interface MarkerClusterProps {
    markers: MapMarker[];
    selectedMarker?: string | null;
    handleSelected?: (value: string | null) => void;
    viewportThreshold?: number;
}

const MarkerCluster = ({ markers, selectedMarker, handleSelected, viewportThreshold = 1000 }: MarkerClusterProps) => {
    const { t } = useTranslation();
    const map = useMap();
    const groupRef = useRef<L.MarkerClusterGroup | null>(null);
    // id -> { marker: L.Marker, data, animationFrame }
    const entriesRef = useRef<Map<string, MarkerEntry>>(new Map());

    // Keeps clusters coherent after a marker moved: markers hidden inside a
    // cluster are re-added so cluster membership is recomputed; visible
    // markers keep their instance (no flicker, popup stays open).
    const refreshClusterMembership = useCallback((entry: MarkerEntry) => {
        const group = groupRef.current;
        if (!group || !group.hasLayer(entry.marker)) return;
        const visibleParent = group.getVisibleParent(entry.marker);
        if (visibleParent && visibleParent !== entry.marker && !entry.marker.isPopupOpen()) {
            group.removeLayer(entry.marker);
            group.addLayer(entry.marker);
        }
    }, []);

    const animateMarkerTo = useCallback((entry: MarkerEntry, toLat: number, toLng: number) => {
        if (entry.animationFrame) {
            cancelAnimationFrame(entry.animationFrame);
            entry.animationFrame = null;
        }
        const from = entry.marker.getLatLng();
        if (from.lat === toLat && from.lng === toLng) return;
        const start = performance.now();
        const step = (now: number) => {
            const progress = Math.min((now - start) / ANIMATION_DURATION_MS, 1);
            const k = easeInOut(progress);
            entry.marker.setLatLng([
                from.lat + (toLat - from.lat) * k,
                from.lng + (toLng - from.lng) * k
            ]);
            if (progress < 1) {
                entry.animationFrame = requestAnimationFrame(step);
            } else {
                entry.animationFrame = null;
                refreshClusterMembership(entry);
            }
        };
        entry.animationFrame = requestAnimationFrame(step);
    }, [refreshClusterMembership]);

    // Create the cluster group once per map instance.
    useEffect(() => {
        const markerGroup = L.markerClusterGroup({ chunkedLoading: true });
        groupRef.current = markerGroup;
        map.addLayer(markerGroup);
        const entries = entriesRef.current;
        return () => {
            entries.forEach((entry) => {
                if (entry.animationFrame) cancelAnimationFrame(entry.animationFrame);
            });
            entries.clear();
            map.removeLayer(markerGroup);
            groupRef.current = null;
        };
    }, [map]);

    // Delta-sync the rendered markers with the incoming data set:
    // existing marker instances are updated in place (position animated,
    // icon/popup/tooltip content refreshed), new ones are added and stale
    // ones removed. Never tears down the whole set on refresh.
    const syncMarkers = useCallback(() => {
        const group = groupRef.current;
        if (!group) return;

        let list = markers;
        if (markers.length > viewportThreshold) {
            const bounds = map.getBounds().pad(0.2);
            list = markers.filter(marker =>
                marker.name === selectedMarker || bounds.contains([marker.lat, marker.lng] as [number, number]));
        }
        const animate = list.length <= MAX_ANIMATED_MARKERS;
        const nextIds = new Set(list.map(markerId));

        // Remove markers that left the data set (or the viewport window).
        entriesRef.current.forEach((entry, id) => {
            if (!nextIds.has(id)) {
                if (entry.animationFrame) cancelAnimationFrame(entry.animationFrame);
                group.removeLayer(entry.marker);
                entriesRef.current.delete(id);
            }
        });

        const toAdd: L.Marker[] = [];
        list.forEach((data) => {
            const id = markerId(data);
            const existing = entriesRef.current.get(id);
            if (!existing) {
                const leafletMarker = L.marker([data.lat, data.lng], { icon: buildIcon(data) });
                const popup = leafletMarker.bindPopup(createEnhancedPopupContent(data, t), {
                    maxWidth: 260,
                    minWidth: 240
                }).getPopup();
                leafletMarker.bindTooltip(buildTooltipContent(data, t), {
                    direction: 'top',
                    offset: [0, -28]
                });
                if (handleSelected) {
                    popup?.on('remove', () => {
                        handleSelected(null);
                    });
                    leafletMarker.on('click', () => {
                        handleSelected(data.name);
                    });
                }
                entriesRef.current.set(id, { marker: leafletMarker, data, animationFrame: null });
                toAdd.push(leafletMarker);
            } else {
                const previous = existing.data;
                existing.data = data;
                if (iconChanged(previous, data)) {
                    existing.marker.setIcon(buildIcon(data));
                }
                if (previous.lat !== data.lat || previous.lng !== data.lng) {
                    if (animate) {
                        animateMarkerTo(existing, data.lat, data.lng);
                    } else {
                        existing.marker.setLatLng([data.lat, data.lng]);
                        refreshClusterMembership(existing);
                    }
                }
                existing.marker.setPopupContent(createEnhancedPopupContent(data, t));
                existing.marker.setTooltipContent(buildTooltipContent(data, t));
            }
        });
        if (toAdd.length > 0) {
            group.addLayers(toAdd);
        }
    }, [map, markers, selectedMarker, handleSelected, t, viewportThreshold, animateMarkerTo, refreshClusterMembership]);

    useEffect(() => {
        syncMarkers();
    }, [syncMarkers]);

    // With viewport-based rendering active, recompute the visible subset on pan/zoom.
    useEffect(() => {
        if (markers.length <= viewportThreshold) return undefined;
        const handler = () => syncMarkers();
        map.on('moveend', handler);
        return () => {
            map.off('moveend', handler);
        };
    }, [map, markers.length, viewportThreshold, syncMarkers]);

    // Delegated click handler for the "resolve address" popup action. The
    // popup content is an HTML string, so a delegated listener on the map
    // container survives popup content updates on refresh cycles.
    useEffect(() => {
        const container = map.getContainer();
        const onClick = async (event: Event) => {
            const target = event.target as Element | null;
            const button = target && target.closest
                ? target.closest<HTMLButtonElement>('.th-resolve-address')
                : null;
            if (!button || button.disabled) return;
            event.preventDefault();
            event.stopPropagation();
            let entry: MarkerEntry | null = null;
            entriesRef.current.forEach((candidate) => {
                if (!entry && candidate.marker.isPopupOpen()) entry = candidate;
            });
            if (!entry) return;
            const resolved: MarkerEntry = entry;
            button.disabled = true;
            button.textContent = t('transporterMap.resolvingAddress');
            // Silent op: a failed reverse-geocode must not toast, so call the
            // api function directly and swallow the error.
            let result = null;
            try {
                result = await reverseGeocode(resolved.data.lat, resolved.data.lng, resolved.data.id ?? null);
            } catch (error) {
                if (process.env.NODE_ENV !== 'production') console.error(error);
            }
            if (!entriesRef.current.has(markerId(resolved.data))) return;
            if (result && (result.address || result.city || result.state || result.country)) {
                resolved.data = {
                    ...resolved.data,
                    address: result.address,
                    city: result.city,
                    state: result.state,
                    country: result.country
                };
                resolved.marker.setPopupContent(createEnhancedPopupContent(resolved.data, t));
            } else {
                button.disabled = false;
                button.textContent = t('transporterMap.addressUnavailable');
            }
        };
        container.addEventListener('click', onClick);
        return () => {
            container.removeEventListener('click', onClick);
        };
    }, [map, t]);

    // Center and open the popup of the selected unit (selection change only,
    // so refresh cycles do not recenter the map).
    useEffect(() => {
        if (!selectedMarker || !groupRef.current) return;
        const group = groupRef.current;
        let entry: MarkerEntry | null = null;
        entriesRef.current.forEach((candidate) => {
            if (!entry && candidate.data.name === selectedMarker) entry = candidate;
        });
        if (entry) {
            const found: MarkerEntry = entry;
            group.zoomToShowLayer(found.marker, () => {
                map.setView(found.marker.getLatLng());
                found.marker.openPopup();
            });
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedMarker]);

    return null;
};

export default MarkerCluster;
