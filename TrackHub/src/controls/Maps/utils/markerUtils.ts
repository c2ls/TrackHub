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

import { calculateDistance } from '../../../utils/distanceUtils';

/**
 * Get marker color based on status and speed
 */
export const getMarkerColor = (speed: number, isOnline = true): string => {
    if (!isOnline) return '#808080'; // Gray for offline
    if (speed > 0) return '#00FF00'; // Green for moving
    return '#FF0000'; // Red for stopped
};

/**
 * Get status color based on speed and online status
 */
export const getStatusColor = (speed: number, isOnline = true): string => {
    if (!isOnline) return 'secondary';
    if (speed > 0) return 'success';
    return 'error';
};

/**
 * Get marker hex color for a computed unit status
 * ('moving' | 'stopped' | 'offline'), following the same color
 * convention as getMarkerColor.
 */
export const getStatusMarkerColor = (status: string): string => {
    if (status === 'offline') return '#808080'; // Gray for offline
    if (status === 'moving') return '#00FF00'; // Green for moving
    return '#FF0000'; // Red for stopped
};

/**
 * Get marker label for a computed unit status
 * ('moving' | 'stopped' | 'offline').
 */
export const getStatusMarkerLabel = (status: string): string => {
    if (status === 'offline') return 'O';
    if (status === 'moving') return 'M';
    return 'S';
};

/**
 * Get marker text/label based on status
 */
export const getMarkerLabel = (speed: number, isOnline = true): string => {
    if (!isOnline) return 'O'; // Offline
    if (speed > 0) return 'M'; // Moving
    return 'S'; // Stopped
};

export interface MarkerFilters {
    minSpeed?: number;
    maxSpeed?: number;
    status?: string;
    searchText?: string;
}

/**
 * Filter markers by criteria
 */
export const filterMarkers = <T extends { name?: string; speed: number }>(
    markers: T[],
    filters: MarkerFilters
): T[] => {
    let filtered = [...markers];

    if (filters.minSpeed !== undefined) {
        filtered = filtered.filter(m => m.speed >= filters.minSpeed!);
    }

    if (filters.maxSpeed !== undefined) {
        filtered = filtered.filter(m => m.speed <= filters.maxSpeed!);
    }

    if (filters.status) {
        filtered = filtered.filter(m => {
            if (filters.status === 'moving') return m.speed > 0;
            if (filters.status === 'stopped') return m.speed === 0;
            return true;
        });
    }

    if (filters.searchText) {
        const search = filters.searchText.toLowerCase();
        filtered = filtered.filter(m =>
            m.name?.toLowerCase().includes(search)
        );
    }

    return filtered;
};

/**
 * Find nearest marker to a given position
 */
export const findNearestMarker = <T extends { lat: number; lng: number }>(
    markers: T[],
    position: { lat: number; lng: number }
): T | null => {
    if (!markers || markers.length === 0) return null;

    let nearest: T | null = null;
    let minDistance = Infinity;

    markers.forEach(marker => {
        const distance = calculateDistance(
            position.lat, position.lng,
            marker.lat, marker.lng
        );
        if (distance < minDistance) {
            minDistance = distance;
            nearest = marker;
        }
    });

    return nearest;
};
