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

/**
 * Get marker color based on status and speed
 */
export const getMarkerColor = (speed: number, isOnline = true): string => {
    if (!isOnline) return '#808080'; // Gray for offline
    if (speed > 0) return '#00FF00'; // Green for moving
    return '#FF0000'; // Red for stopped
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
