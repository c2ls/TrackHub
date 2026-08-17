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

import { describe, test, expect } from 'vitest';
import { escapeHtml } from 'utils/htmlUtils';
import { createEnhancedPopupContent } from 'controls/Maps/utils/popupUtils';
import type { MapMarker } from 'controls/Maps/core/mapTypes';

/**
 * The map layer builds HTML **strings** that Leaflet (`bindPopup`/`bindTooltip`) and the Google
 * InfoWindow assign via innerHTML — React's automatic escaping does not apply there. Transporter
 * names are account-editable free text and addresses come from a third-party geocoder, so both are
 * untrusted. These tests pin the escaping so it cannot silently regress.
 */

const t = ((key: string) => key) as unknown as Parameters<typeof createEnhancedPopupContent>[1];

const PAYLOAD = '<img src=x onerror="alert(1)">';

const marker = (overrides: Partial<MapMarker> = {}): MapMarker =>
  ({
    id: 'a1',
    name: 'Truck 1',
    dateTime: '2026-07-21T08:30:00.000Z',
    speed: 42,
    lat: 4.65,
    lng: -74.05,
    ...overrides,
  }) as MapMarker;

describe('escapeHtml', () => {
  test('neutralises the characters that break out of a body or an attribute', () => {
    expect(escapeHtml('<script>')).toBe('&lt;script&gt;');
    expect(escapeHtml('a "quoted" & \'single\'')).toBe('a &quot;quoted&quot; &amp; &#39;single&#39;');
  });

  test('renders null and undefined as an empty string rather than the literal text', () => {
    expect(escapeHtml(null)).toBe('');
    expect(escapeHtml(undefined)).toBe('');
  });

  test('escapes the ampersand first so entities are not double-broken', () => {
    expect(escapeHtml('&lt;')).toBe('&amp;lt;');
  });
});

describe('createEnhancedPopupContent escaping', () => {
  test('a script payload in the transporter name cannot open a tag', () => {
    const html = createEnhancedPopupContent(marker({ name: PAYLOAD }), t);
    expect(html).not.toContain(PAYLOAD);
    expect(html).toContain('&lt;img src=x onerror=&quot;alert(1)&quot;&gt;');
  });

  test('a payload in the reverse-geocoded address is escaped', () => {
    // Not account-controlled input — this arrives from the third-party geocoding provider.
    const html = createEnhancedPopupContent(marker({ address: PAYLOAD }), t);
    expect(html).not.toContain(PAYLOAD);
  });

  test('a payload in transporterType is escaped', () => {
    const html = createEnhancedPopupContent(marker({ transporterType: PAYLOAD }), t);
    expect(html).not.toContain(PAYLOAD);
  });

  test('city/state/country are escaped when no address is present', () => {
    const html = createEnhancedPopupContent(marker({ city: PAYLOAD }), t);
    expect(html).not.toContain(PAYLOAD);
  });

  test('a quote in an id cannot break out of the data-transporter-id attribute', () => {
    const html = createEnhancedPopupContent(marker({ id: '" onmouseover="alert(1)' }), t);
    expect(html).not.toContain('" onmouseover="alert(1)"');
    expect(html).toContain('data-transporter-id="&quot; onmouseover=&quot;alert(1)"');
  });
});

describe('hourmeter unit', () => {
  // hourmeter is already in HOURS (AttributesVm.Hourmeter; sole producer is CommandTrack's
  // `hobbsMeter`, a decimal engine-hour reading). Dividing by 3_600_000 rendered "0 hrs" for
  // every real-world value.
  test('a realistic engine-hour reading renders as hours, not zero', () => {
    const html = createEnhancedPopupContent(
      marker({ attributes: { hourmeter: 1234.5 } as MapMarker['attributes'] }),
      t
    );
    expect(html).toContain('1234.5 hrs');
    expect(html).not.toContain('0 hrs');
  });
});
