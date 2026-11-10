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

/**
 * The map layer builds HTML **strings** that Leaflet (`bindTooltip`, `divIcon`) assigns via innerHTML —
 * React's automatic escaping does not apply there. Transporter names are account-editable free text,
 * so they are untrusted. These tests pin the escaping so it cannot silently regress.
 */

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
