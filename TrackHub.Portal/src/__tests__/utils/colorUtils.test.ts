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

import { getRandomColor } from 'utils/colorUtils';

const VALID_COLORS = [
  '#1f77b4', '#ff7f0e', '#2ca02c', '#9467bd', '#8c564b',
  '#e377c2', '#7f7f7f', '#bcbd22', '#17becf'
];

describe('getRandomColor', () => {
  test('returns a color from the predefined list', () => {
    const color = getRandomColor();
    expect(VALID_COLORS).toContain(color);
  });

  test('returns a string', () => {
    expect(typeof getRandomColor()).toBe('string');
  });

  test('returns a valid hex color format', () => {
    const color = getRandomColor();
    expect(color).toMatch(/^#[0-9a-f]{6}$/);
  });

  test('returns colors from the full range over many calls', () => {
    const seen = new Set();
    for (let i = 0; i < 200; i++) {
      seen.add(getRandomColor());
    }
    // With 200 random picks from 9 colors, we should see at least 5
    expect(seen.size).toBeGreaterThanOrEqual(5);
  });
});
