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

type SvgIconFormat = 'svg' | 'dataURL';

interface SvgIconOptions {
    width?: number;
    height?: number;
    color?: string;
}

const createSvgIcon = (
    rotation: number,
    text: string,
    format: SvgIconFormat = 'svg',
    options: SvgIconOptions = {}
): string => {
    const scaledWidth = options.width || 40;
    const scaledHeight = options.height || 40;
    const circleColor = options.color || "#1E90FF";

    // Calculate darker gradient color
    const gradientColor = darkenColor(circleColor, 0.7); // Darken by 30%

    // Define SVG content
    const svg = `
        <svg width="${scaledWidth}" height="${scaledHeight}" viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
            <!-- Circle representing the sun -->
            <circle cx="50" cy="50" r="30" fill="${circleColor}" stroke="${circleColor}" stroke-width="2.25"/>
            <defs>
                <linearGradient id="rayGradient" x1="0%" y1="100%" x2="0%" y2="0%">
                    <stop offset="0%" stop-color="${circleColor}"/>
                    <stop offset="100%" stop-color="${gradientColor}"/>
                </linearGradient>
            </defs>
            <polygon points="50,10 63.75,35 36.25,35" fill="url(#rayGradient)" stroke="${circleColor}" stroke-width="1.5" stroke-linejoin="round" transform="rotate(${rotation}, 50, 50)"/>
            <text x="50" y="60" text-anchor="middle" font-family="Arial, sans-serif" font-size="26" font-weight="bold" fill="#000000">${text}</text>
        </svg>`;

    if (format === 'dataURL') {
        return `data:image/svg+xml;utf8,${encodeURIComponent(svg)}`;
    }
    return svg;
};

function darkenColor(hexColor: string, factor: number): string {
    // Remove the '#' if present
    hexColor = hexColor.replace('#', '');

    // Parse the hex color to RGB
    const red = parseInt(hexColor.substring(0, 2), 16);
    const green = parseInt(hexColor.substring(2, 4), 16);
    const blue = parseInt(hexColor.substring(4, 6), 16);

    // Darken the RGB values
    const darkenedRed = Math.round(red * factor);
    const darkenedGreen = Math.round(green * factor);
    const darkenedBlue = Math.round(blue * factor);

    // Convert back to hexadecimal
    const darkenedHex = `#${darkenedRed.toString(16).padStart(2, '0')}${darkenedGreen.toString(16).padStart(2, '0')}${darkenedBlue.toString(16).padStart(2, '0')}`;

    return darkenedHex;
}

/**
 * Get color based on speed and status
 */
const getMarkerColor = (speed: number, isOnline = true): string => {
    if (!isOnline) return '#808080'; // Gray for offline
    if (speed > 0) return '#00FF00'; // Green for moving
    return '#FF0000'; // Red for stopped
};

/**
 * Create marker icon with custom color based on status
 */
const createMarkerIconWithStatus = (
    rotation: number,
    text: string,
    speed: number,
    isOnline = true,
    format: SvgIconFormat = 'svg'
): string => {
    const color = getMarkerColor(speed, isOnline);
    return createSvgIcon(rotation, text, format, { color });
};

export {
    createSvgIcon,
    createMarkerIconWithStatus,
    getMarkerColor
};
