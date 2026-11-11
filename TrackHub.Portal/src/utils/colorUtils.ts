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

const TRIP_COLORS = [
    '#1f77b4', // Blue
    '#ff7f0e', // Orange
    '#2ca02c', // Green
    '#9467bd', // Purple
    '#8c564b', // Brown
    '#e377c2', // Pink
    '#17becf'  // Cyan
];

/**
 * Color of the trip at a position in the replay list; neighbouring trips never share one.
 */
export function getTripColor(index: number): string {
    return TRIP_COLORS[index % TRIP_COLORS.length];
}
