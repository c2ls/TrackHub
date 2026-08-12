/**
* Copyright (c) 2026 Sergio Hernandez. All rights reserved.
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
import { getColor } from 'data/colors';
import { getPoiType } from 'data/poiTypes';
import type { MapPoi } from 'controls/Maps/core/mapTypes';

type SvgIconFormat = 'svg' | 'dataURL';

// Hex values for the shared display-color convention in data/colors.js.
const COLOR_LABEL_HEX: Record<string, string> = {
  Red: '#e53935',
  Blue: '#1e88e5',
  Green: '#43a047',
  Yellow: '#fdd835',
  Orange: '#fb8c00',
  Purple: '#8e24aa',
  Pink: '#d81b60',
  Brown: '#6d4c41',
  Black: '#212121',
  White: '#fafafa'
};

const DEFAULT_POI_COLOR = '#1e88e5';

/**
 * Maps a POI color value (data/colors.js convention) to a hex color.
 */
export const getPoiColorHex = (colorValue: number): string =>
  COLOR_LABEL_HEX[getColor(colorValue)] || DEFAULT_POI_COLOR;

/**
 * Returns the i18n key suffix for a POI type value (e.g. 1 -> 'clientSite').
 */
export const getPoiTypeKey = (typeValue: number): string => {
  const label = getPoiType(typeValue);
  return label ? label.charAt(0).toLowerCase() + label.slice(1) : '';
};

const escapeHtml = (value: unknown): string =>
  String(value ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');

/**
 * Creates the SVG pin used for POI markers.
 */
export const createPoiPinSvg = (colorHex: string, format: SvgIconFormat = 'svg'): string => {
  const svg = `<svg width="28" height="36" viewBox="0 0 24 32" xmlns="http://www.w3.org/2000/svg">
        <path d="M12 1C6 1 1.5 5.5 1.5 11.5c0 8 10.5 19 10.5 19s10.5-11 10.5-19C22.5 5.5 18 1 12 1z" fill="${colorHex}" stroke="#ffffff" stroke-width="1.5"/>
        <circle cx="12" cy="11.5" r="4.5" fill="#ffffff"/>
    </svg>`;
  if (format === 'dataURL') {
    return `data:image/svg+xml;utf8,${encodeURIComponent(svg)}`;
  }
  return svg;
};

/**
 * Builds the shared HTML content for a POI popup/info window.
 */
export const buildPoiPopupHtml = (poi: MapPoi, t: TFunction): string => {
  const typeKey = getPoiTypeKey(poi.type);
  const typeLabel = typeKey ? t(`poi.types.${typeKey}` as 'poi.types.clientSite') : '';
  let content = `
        <div style="min-width: 200px; font-family: Arial, sans-serif; font-size: 13px;">
            <div style="display: flex; align-items: center; gap: 6px; margin-bottom: 6px;">
                <span style="display: inline-block; width: 10px; height: 10px; border-radius: 50%; background: ${getPoiColorHex(poi.color)};"></span>
                <strong style="font-size: 14px;">${escapeHtml(poi.name)}</strong>
            </div>`;
  if (typeLabel) {
    content += `
            <div style="margin-bottom: 4px;">
                <strong style="font-size: 12px;">${t('poi.type')}:</strong>
                <span style="font-size: 12px;">${escapeHtml(typeLabel)}</span>
            </div>`;
  }
  if (poi.description) {
    content += `
            <div style="margin-bottom: 4px;">
                <strong style="font-size: 12px;">${t('poi.description')}:</strong>
                <span style="font-size: 12px;">${escapeHtml(poi.description)}</span>
            </div>`;
  }
  if (poi.address) {
    content += `
            <div style="margin-bottom: 4px;">
                <strong style="font-size: 12px;">${t('poi.address')}:</strong>
                <span style="font-size: 12px;">${escapeHtml(poi.address)}</span>
            </div>`;
  }
  content += `
        </div>`;
  return content;
};
