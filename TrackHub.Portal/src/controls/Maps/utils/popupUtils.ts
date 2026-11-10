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
