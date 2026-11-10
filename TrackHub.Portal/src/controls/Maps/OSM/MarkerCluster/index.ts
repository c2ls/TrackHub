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
import 'controls/Maps/css/markerCluster.css';
import { formatDateTime } from "utils/dateUtils";
import { escapeHtml } from 'utils/htmlUtils';

import { useCallback, useEffect, useRef } from 'react';
import { useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet.markercluster';
import { useTranslation } from 'react-i18next';
import type { TFunction } from 'i18next';
import { createUnitIcon } from 'controls/Maps/utils/imageUtils';
import { getRelativeTimeText } from 'controls/Maps/utils/popupUtils';
import type { MapMarker } from 'controls/Maps/core/mapTypes';

const ANIMATION_DURATION_MS = 1000;
// Above this rendered-marker count position changes are applied without animation.
const MAX_ANIMATED_MARKERS = 500;

interface MarkerEntry {
    marker: L.Marker;
    data: MapMarker;
    selected: boolean;
    animationFrame: number | null;
}

const markerId = (marker: MapMarker): string => marker.id ?? marker.name;

const easeInOut = (x: number): number => (x < 0.5 ? 2 * x * x : 1 - Math.pow(-2 * x + 2, 2) / 2);

const buildIcon = (data: MapMarker, selected: boolean): L.DivIcon => L.divIcon({
    className: 'th-unit-icon',
    html: `${createUnitIcon(data.status ?? 'offline', data.rotation, selected)}<span class="th-unit-label">${escapeHtml(data.name)}</span>`,
    iconSize: [48, 48],
    iconAnchor: [24, 24],
});

const buildClusterIcon = (cluster: L.MarkerCluster): L.DivIcon => L.divIcon({
    className: 'th-cluster',
    html: `<span>${cluster.getChildCount()}</span>`,
    iconSize: L.point(42, 42),
});

const buildTooltipContent = (data: MapMarker, t: TFunction): string => `
    <div style="font-size: 12px;">
        <strong>${escapeHtml(data.name)}</strong><br/>
        ${t('transporterMap.speed')}: ${escapeHtml(data.speed)} km/h<br/>
        ${t('transporterMap.lastReport')}: ${formatDateTime(data.dateTime)}
        ${getRelativeTimeText(data.dateTime, t) ? `(${getRelativeTimeText(data.dateTime, t)})` : ''}
    </div>`;

const iconChanged = (prev: MapMarker, next: MapMarker): boolean =>
    prev.rotation !== next.rotation || prev.status !== next.status || prev.name !== next.name;

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
    const entriesRef = useRef<Map<string, MarkerEntry>>(new Map());

    // Keeps clusters coherent after a marker moved: markers hidden inside a
    // cluster are re-added so cluster membership is recomputed; visible
    // markers keep their instance (no flicker).
    const refreshClusterMembership = useCallback((entry: MarkerEntry) => {
        const group = groupRef.current;
        if (!group || !group.hasLayer(entry.marker)) return;
        const visibleParent = group.getVisibleParent(entry.marker);
        if (visibleParent && visibleParent !== entry.marker) {
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
        const markerGroup = L.markerClusterGroup({
            chunkedLoading: true,
            showCoverageOnHover: false,
            iconCreateFunction: buildClusterIcon,
        });
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
    // icon/tooltip content refreshed), new ones are added and stale
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
            const selected = data.name === selectedMarker;
            const existing = entriesRef.current.get(id);
            if (!existing) {
                const leafletMarker = L.marker([data.lat, data.lng], {
                    icon: buildIcon(data, selected),
                    zIndexOffset: selected ? 1000 : 0,
                });
                leafletMarker.bindTooltip(buildTooltipContent(data, t), {
                    direction: 'top',
                    offset: [0, -18]
                });
                if (handleSelected) {
                    leafletMarker.on('click', () => {
                        handleSelected(data.name);
                    });
                }
                entriesRef.current.set(id, { marker: leafletMarker, data, selected, animationFrame: null });
                toAdd.push(leafletMarker);
            } else {
                const previous = existing.data;
                existing.data = data;
                if (iconChanged(previous, data) || existing.selected !== selected) {
                    existing.selected = selected;
                    existing.marker.setIcon(buildIcon(data, selected));
                    existing.marker.setZIndexOffset(selected ? 1000 : 0);
                }
                if (previous.lat !== data.lat || previous.lng !== data.lng) {
                    if (animate) {
                        animateMarkerTo(existing, data.lat, data.lng);
                    } else {
                        existing.marker.setLatLng([data.lat, data.lng]);
                        refreshClusterMembership(existing);
                    }
                }
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

    // Bring the selected unit into view (selection change only, so refresh
    // cycles do not recenter the map).
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
            });
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [selectedMarker]);

    return null;
};

export default MarkerCluster;
