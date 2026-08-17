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

/**
 * Shared, provider-neutral styling for the route layer. Both the OSM and Google
 * adapters read these so the two renderings stay identical — the whole point of
 * the core contract is that a layer is authored once, not twice.
 */

/** Planned-route polyline colour. */
export const ROUTE_COLOR = '#2D5BFF';
/** Corridor polygon colour (the tolerance band deviation is measured against). */
export const CORRIDOR_COLOR = '#8A9BFF';
/** A toll station on the route WITH a tariff for the vehicle class. */
export const TOLL_PRICED_COLOR = '#2E7D32';
/** A matched toll station with NO tariff — the estimate is incomplete, and says so. */
export const TOLL_UNPRICED_COLOR = '#ED6C02';

/** Stop pin colour per stop status; unknown statuses fall back to the pending grey-blue. */
export const STOP_STATUS_COLORS: Record<string, string> = {
  Pending: '#5E72E4',
  Arrived: '#2DCE89',
  Departed: '#11CDEF',
  Skipped: '#8898AA',
};

export const stopColor = (status?: string): string =>
  (status && STOP_STATUS_COLORS[status]) || STOP_STATUS_COLORS.Pending;

/**
 * Numbered stop pin as an inline SVG string. Returned raw for Leaflet's
 * `divIcon` and as a `data:` URL for Google's `Icon.url`.
 */
export function createStopPinSvg(
  sequence: number,
  color: string,
  format: 'raw' | 'dataURL' = 'raw'
): string {
  // Two-digit sequences need a smaller glyph to stay inside the pin head.
  const label = String(sequence);
  const fontSize = label.length > 2 ? 9 : label.length > 1 ? 11 : 12;
  const svg =
    `<svg xmlns="http://www.w3.org/2000/svg" width="28" height="36" viewBox="0 0 28 36">` +
    `<path d="M14 0C6.8 0 1 5.8 1 13c0 9.4 11.6 21.6 12.1 22.1.5.5 1.3.5 1.8 0C15.4 34.6 27 22.4 27 13 27 5.8 21.2 0 14 0z" ` +
    `fill="${color}" stroke="#ffffff" stroke-width="2"/>` +
    `<circle cx="14" cy="13" r="8" fill="#ffffff"/>` +
    `<text x="14" y="13" text-anchor="middle" dominant-baseline="central" ` +
    `font-family="Helvetica, Arial, sans-serif" font-size="${fontSize}" font-weight="700" fill="${color}">${label}</text>` +
    `</svg>`;
  return format === 'dataURL' ? `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(svg)}` : svg;
}

/** Toll-station diamond marker; unpriced stations are hollow so the gap is visible at a glance. */
export function createTollPinSvg(
  hasTariff: boolean,
  format: 'raw' | 'dataURL' = 'raw'
): string {
  const color = hasTariff ? TOLL_PRICED_COLOR : TOLL_UNPRICED_COLOR;
  const svg =
    `<svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" viewBox="0 0 22 22">` +
    `<path d="M11 1l10 10-10 10L1 11z" fill="${hasTariff ? color : '#ffffff'}" ` +
    `stroke="${color}" stroke-width="2"/>` +
    `</svg>`;
  return format === 'dataURL' ? `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(svg)}` : svg;
}
