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

interface LatLngPoint {
    lat: number;
    lng: number;
}

function closePolygon(latlngs: LatLngPoint[]): LatLngPoint[] {
    if (latlngs.length < 3) {
        throw new Error("A polygon must have at least 3 points");
    }
    const firstPoint = latlngs[0];
    const lastPoint = latlngs[latlngs.length - 1];
    // Check if the first and last points are the same
    if (firstPoint.lat !== lastPoint.lat || firstPoint.lng !== lastPoint.lng) {
        latlngs.push(firstPoint);
    }
    return latlngs;
}

const EARTH_RADIUS_M = 6378137;

/**
 * SW/NE corners (`[[south, west], [north, east]]`) of the lat/lng box that
 * encloses a circle of `radiusMeters` around (lat, lng). Provider-agnostic and
 * map-instance-free (equirectangular degree offset), so both the Leaflet and
 * Google geofence editors can extend their native bounds with a circle shape
 * when fitting the viewport to an account's geofences.
 */
function circleBoundsCorners(
    lat: number,
    lng: number,
    radiusMeters: number
): [[number, number], [number, number]] {
    const dLat = (radiusMeters / EARTH_RADIUS_M) * (180 / Math.PI);
    const cos = Math.cos((lat * Math.PI) / 180);
    const safeCos = Math.abs(cos) < 1e-12 ? 1e-12 : cos;
    const dLng = (radiusMeters / (EARTH_RADIUS_M * safeCos)) * (180 / Math.PI);
    return [
        [lat - dLat, lng - dLng],
        [lat + dLat, lng + dLng],
    ];
}

export {
    closePolygon,
    circleBoundsCorners
};
