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

import type { TFunction } from 'i18next';
import { formatDateTime } from "utils/dateUtils";
import type { MapMarker } from 'controls/Maps/core/mapTypes';

/**
 * Generates WhatsApp share URL for location
 */
export const getWhatsAppShareUrl = (lat: number, lng: number, name: string): string => {
    const message = encodeURIComponent(`${name} location: https://www.google.com/maps?q=${lat},${lng}`);
    return `https://wa.me/?text=${message}`;
};

/**
 * Generates Google Street View URL
 */
export const getStreetViewUrl = (lat: number, lng: number): string => {
    return `https://www.google.com/maps/@?api=1&map_action=pano&viewpoint=${lat},${lng}`;
};

/**
 * Formats a value with unit, returns null if value is not available
 */
const formatWithUnit = (value: unknown, unit: string): string | null => {
    if (value === undefined || value === null) return null;
    return `${value} ${unit}`;
};

/**
 * Returns a localized relative time text (e.g. "5 min ago") for a date value.
 */
export const getRelativeTimeText = (
    value: string | number | Date | null | undefined,
    t: TFunction
): string => {
    if (!value) return '';
    const date = new Date(value);
    if (isNaN(date.getTime())) return '';
    const diffMs = Date.now() - date.getTime();
    if (diffMs < 0) return '';
    const minutes = Math.floor(diffMs / 60000);
    if (minutes < 1) return t('transporterMap.justNow');
    if (minutes < 60) return t('transporterMap.minutesAgo', { value: minutes });
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return t('transporterMap.hoursAgo', { value: hours });
    return t('transporterMap.daysAgo', { value: Math.floor(hours / 24) });
};

/**
 * Generates HTML content for enhanced marker popup
 */
export const createEnhancedPopupContent = (marker: MapMarker, t: TFunction): string => {
    const { id, name, dateTime, speed, attributes, address, altitude, city, country, state, transporterType, lat, lng } = marker;

    // Build the popup content with available data
    let content = `
        <div style="min-width: 240px; font-family: Arial, sans-serif; font-size: 13px;">
            <div style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 8px; margin: -15px -20px 6px -20px; border-radius: 3px 3px 0 0;">
                <h3 style="margin: 0; font-size: 15px; font-weight: bold;">${name}</h3>
                ${transporterType ? `<p style="margin: 2px 0 0 0; font-size: 12px; opacity: 0.9;">${transporterType}</p>` : ''}
            </div>

            <div style="padding: 0;">
                <div style="margin-bottom: 4px;">
                    <strong style="color: #667eea; font-size: 12px;">⏱ ${t('transporterMap.lastReport') || t('transporterMap.dateTime')}:</strong>
                    <span style="font-size: 12px;">${formatDateTime(dateTime)}</span>
                    ${getRelativeTimeText(dateTime, t) ? `<span style="font-size: 11px; color: #888;"> (${getRelativeTimeText(dateTime, t)})</span>` : ''}
                </div>

                <div style="margin-bottom: 4px;">
                    <strong style="color: #667eea; font-size: 12px;">🚗 ${t('transporterMap.speed')}:</strong>
                    <span style="font-size: 13px; font-weight: bold; color: ${speed > 0 ? '#10b981' : '#ef4444'};">${speed} km/h</span>
                    <span style="margin-left: 4px; padding: 2px 5px; background: ${speed > 0 ? '#d1fae5' : '#fee2e2'}; color: ${speed > 0 ? '#065f46' : '#991b1b'}; border-radius: 2px; font-size: 10px;">
                        ${speed > 0 ? t('transporterMap.moving') || 'Moving' : t('transporterMap.stopped') || 'Stopped'}
                    </span>
                </div>`;

    // Add attributes if available
    if (attributes) {
        if (attributes.mileage !== undefined && attributes.mileage !== null) {
            content += `
                <div style="margin-bottom: 4px;">
                    <strong style="color: #667eea; font-size: 12px;">📏 ${t('transporterMap.mileage') || 'Mileage'}:</strong>
                    <span style="font-size: 12px;">${(attributes.mileage / 1000).toFixed(2)} km</span>
                </div>`;
        }

        if (attributes.temperature !== undefined && attributes.temperature !== null) {
            content += `
                <div style="margin-bottom: 4px;">
                    <strong style="color: #667eea; font-size: 12px;">🌡 ${t('transporterMap.temperature') || 'Temperature'}:</strong>
                    <span style="font-size: 12px;">${attributes.temperature}°C</span>
                </div>`;
        }

        if (attributes.hourmeter !== undefined && attributes.hourmeter !== null) {
            const hours = Math.floor(attributes.hourmeter / 3600000);
            content += `
                <div style="margin-bottom: 4px;">
                    <strong style="color: #667eea; font-size: 12px;">⏲ ${t('transporterMap.hourmeter') || 'Hourmeter'}:</strong>
                    <span style="font-size: 12px;">${hours} hrs</span>
                </div>`;
        }

        if (attributes.ignition !== undefined && attributes.ignition !== null) {
            content += `
                <div style="margin-bottom: 4px;">
                    <strong style="color: #667eea; font-size: 12px;">🔑 ${t('transporterMap.accStatus') || 'Ignition'}:</strong>
                    <span style="color: ${attributes.ignition ? '#10b981' : '#ef4444'}; font-size: 12px;">
                        ${attributes.ignition ? (t('transporterMap.accOn') || 'ON') : (t('transporterMap.accOff') || 'OFF')}
                    </span>
                </div>`;
        }

        if (attributes.satellites !== undefined && attributes.satellites !== null) {
            content += `
                <div style="margin-bottom: 4px;">
                    <strong style="color: #667eea; font-size: 12px;">🛰 ${t('transporterMap.satellites') || 'Satellites'}:</strong>
                    <span style="font-size: 12px;">${attributes.satellites}</span>
                </div>`;
        }
    }

    // Add altitude if available
    if (altitude !== undefined && altitude !== null) {
        content += `
            <div style="margin-bottom: 4px;">
                <strong style="color: #667eea; font-size: 12px;">⛰ ${t('transporterMap.altitude') || 'Altitude'}:</strong>
                <span style="font-size: 12px;">${altitude} m</span>
            </div>`;
    }

    // Add address if available; otherwise show coordinates + on-demand resolve action
    if (address) {
        content += `
            <div style="margin-bottom: 4px;">
                <strong style="color: #667eea; font-size: 12px;">📍 ${t('transporterMap.address') || 'Address'}:</strong><br/>
                <span style="font-size: 11px;">${address}</span>
            </div>`;
    } else {
        if (city || state || country) {
            const location = [city, state, country].filter(Boolean).join(', ');
            content += `
            <div style="margin-bottom: 4px;">
                <strong style="color: #667eea; font-size: 12px;">📍 ${t('transporterMap.location') || 'Location'}:</strong><br/>
                <span style="font-size: 11px;">${location}</span>
            </div>`;
        }
        if (lat !== undefined && lat !== null && lng !== undefined && lng !== null) {
            content += `
            <div style="margin-bottom: 4px;">
                <strong style="color: #667eea; font-size: 12px;">📍 ${t('transporterMap.coordinates') || 'Coordinates'}:</strong>
                <span style="font-size: 11px;">${Number(lat).toFixed(6)}, ${Number(lng).toFixed(6)}</span><br/>
                <button type="button" class="th-resolve-address" data-lat="${lat}" data-lng="${lng}" data-transporter-id="${id ?? ''}"
                    style="margin-top: 4px; padding: 3px 8px; background: #667eea; color: white; border: none; border-radius: 4px; font-size: 11px; cursor: pointer;">
                    ${t('transporterMap.resolveAddress') || 'Resolve address'}
                </button>
            </div>`;
        }
    }

    // Add action buttons
    const whatsappUrl = getWhatsAppShareUrl(lat, lng, name);
    const streetViewUrl = getStreetViewUrl(lat, lng);

    content += `
            </div>

            <div style="display: flex; gap: 4px; margin-top: 8px; padding-top: 8px; border-top: 1px solid #e5e7eb;">
                <a href="${whatsappUrl}" target="_blank" rel="noopener noreferrer"
                   style="display: inline-flex; align-items: center; justify-content: center; background: #25D366; color: white; text-decoration: none; width: 28px; height: 28px; border-radius: 4px;" title="${t('transporterMap.shareWhatsApp') || 'Share on WhatsApp'}">
                    <svg style="width: 16px; height: 16px; fill: white;" viewBox="0 0 24 24"><path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413Z"/></svg>
                </a>
                <a href="${streetViewUrl}" target="_blank" rel="noopener noreferrer"
                   style="display: inline-flex; align-items: center; justify-content: center; background: #4285f4; color: white; text-decoration: none; width: 28px; height: 28px; border-radius: 4px;" title="${t('transporterMap.streetView') || 'Street View'}">
                    <svg style="width: 16px; height: 16px; fill: white;" viewBox="0 0 24 24"><path d="M12,11.5A2.5,2.5 0 0,1 9.5,9A2.5,2.5 0 0,1 12,6.5A2.5,2.5 0 0,1 14.5,9A2.5,2.5 0 0,1 12,11.5M12,2A7,7 0 0,0 5,9C5,14.25 12,22 12,22C12,22 19,14.25 19,9A7,7 0 0,0 12,2Z"/></svg>
                </a>
            </div>
        </div>
    `;

    return content;
};
