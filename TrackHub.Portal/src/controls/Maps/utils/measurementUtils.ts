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

interface LatLngPoint {
    lat: number;
    lng: number;
}

type MeasurementUnit = 'metric' | 'imperial';

/**
 * Calculate total distance along a path of coordinates
 */
export const calculatePathDistance = (latlngs: LatLngPoint[]): number => {
    if (!latlngs || latlngs.length < 2) return 0;

    let totalDistance = 0;
    for (let i = 0; i < latlngs.length - 1; i++) {
        totalDistance += calculateDistance(
            latlngs[i].lat, latlngs[i].lng,
            latlngs[i + 1].lat, latlngs[i + 1].lng
        );
    }

    return totalDistance;
};

/**
 * Format distance to human-readable string
 */
export const formatDistance = (meters: number, unit: MeasurementUnit = 'metric'): string => {
    if (unit === 'imperial') {
        const feet = meters * 3.28084;
        if (feet < 5280) {
            return `${Math.round(feet)} ft`;
        }
        const miles = feet / 5280;
        return `${miles.toFixed(2)} mi`;
    }

    // Metric
    if (meters < 1000) {
        return `${Math.round(meters)} m`;
    }
    const km = meters / 1000;
    return `${km.toFixed(2)} km`;
};
