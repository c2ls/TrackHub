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

import { escapeHtml } from 'utils/htmlUtils';

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
    // The markup below is parsed as HTML by L.divIcon, so the two values that can come from account
    // data are escaped here rather than at the (currently constant) call sites.
    const circleColor = escapeHtml(options.color || "#1E90FF");
    const label = escapeHtml(text);

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
            <text x="50" y="60" text-anchor="middle" font-family="Arial, sans-serif" font-size="26" font-weight="bold" fill="#000000">${label}</text>
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

type UnitIconStatus = 'moving' | 'stopped' | 'offline';

const UNIT_STATUS_COLOR: Record<UnitIconStatus, string> = {
    moving: '#157F45',
    stopped: '#B45309',
    offline: '#64748B',
};
const SELECTION_COLOR = '#0B7499';

/**
 * A unit marker: a solid status-coloured badge with a white outline, so it cannot be mistaken for
 * the base map's own place pins. The status is carried by shape as well as colour — a teardrop
 * pointing along the heading when moving, a rounded square when stopped, a barred disc when offline.
 */
const createUnitIcon = (
    status: UnitIconStatus,
    rotation: number,
    selected = false,
    format: SvgIconFormat = 'svg'
): string => {
    const color = UNIT_STATUS_COLOR[status];
    const outline = 'stroke="#FFFFFF" stroke-width="2.5" stroke-linejoin="round"';
    const halo = selected
        ? `<circle cx="24" cy="24" r="22" fill="${SELECTION_COLOR}" fill-opacity="0.28" stroke="${SELECTION_COLOR}" stroke-width="1.5"/>`
        : '';
    let badge = `<circle cx="24" cy="24" r="12" fill="${color}" ${outline}/><rect x="18" y="22.25" width="12" height="3.5" rx="1.75" fill="#FFFFFF"/>`;
    if (status === 'moving') {
        badge = `<g transform="rotate(${Number(rotation) || 0} 24 24)"><path d="M24 5 L33.4 17.6 A12 12 0 1 1 14.6 17.6 Z" fill="${color}" ${outline}/><path d="M24 15.5 L29.5 28 L24 25 L18.5 28 Z" fill="#FFFFFF"/></g>`;
    } else if (status === 'stopped') {
        badge = `<rect x="12.5" y="12.5" width="23" height="23" rx="6.5" fill="${color}" ${outline}/><rect x="19.5" y="19.5" width="9" height="9" rx="1.5" fill="#FFFFFF"/>`;
    }
    const svg = `<svg width="48" height="48" viewBox="0 0 48 48" xmlns="http://www.w3.org/2000/svg">${halo}${badge}</svg>`;

    return format === 'dataURL' ? `data:image/svg+xml;utf8,${encodeURIComponent(svg)}` : svg;
};

/** The cluster bubble both map providers draw, so a cluster looks the same on either. */
const CLUSTER_ICON_URL = `data:image/svg+xml;utf8,${encodeURIComponent(
    `<svg width="42" height="42" viewBox="0 0 42 42" xmlns="http://www.w3.org/2000/svg"><circle cx="21" cy="21" r="18.5" fill="#FFFFFF" stroke="${SELECTION_COLOR}" stroke-width="3"/></svg>`
)}`;

export {
    createSvgIcon,
    createUnitIcon,
    CLUSTER_ICON_URL
};