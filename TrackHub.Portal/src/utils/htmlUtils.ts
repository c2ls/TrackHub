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
 * Escapes a value for interpolation into an HTML string.
 *
 * React escapes interpolated values automatically; the map layer does NOT, because Leaflet's
 * `bindPopup`/`bindTooltip` and the Google `InfoWindow` accept an HTML **string** which they assign
 * via `innerHTML`. Every dynamic value interpolated into one of those strings must pass through
 * here — transporter names are account-editable free text and reverse-geocoded addresses come from
 * a third-party provider, so neither is trustworthy.
 *
 * Escapes the five characters that can break out of either an element body or a
 * double/single-quoted attribute value, so one helper is safe in both positions.
 */
export const escapeHtml = (value: unknown): string =>
  String(value ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
