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

import { getTripColor } from 'utils/colorUtils';

describe('getTripColor', () => {
  test('returns a valid hex color format', () => {
    expect(getTripColor(0)).toMatch(/^#[0-9a-f]{6}$/);
  });

  test('is stable for an index', () => {
    expect(getTripColor(3)).toBe(getTripColor(3));
  });

  test('never repeats between neighbouring trips, which sit a stop apart in the list', () => {
    for (let i = 0; i < 50; i++) {
      expect(getTripColor(i)).not.toBe(getTripColor(i + 1));
      expect(getTripColor(i)).not.toBe(getTripColor(i + 2));
    }
  });
});
