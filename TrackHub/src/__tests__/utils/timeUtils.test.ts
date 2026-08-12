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

import { formatISODuration, formatTime } from 'utils/timeUtils';

describe('formatISODuration', () => {
  test('formats hours, minutes, and seconds', () => {
    expect(formatISODuration('PT2H30M15S')).toBe('2 hr 30 min 15 s');
  });

  test('formats hours only', () => {
    expect(formatISODuration('PT5H')).toBe('5 hr');
  });

  test('formats minutes only', () => {
    expect(formatISODuration('PT45M')).toBe('45 min');
  });

  test('formats seconds only', () => {
    expect(formatISODuration('PT10S')).toBe('10 s');
  });

  test('formats hours and minutes', () => {
    expect(formatISODuration('PT1H30M')).toBe('1 hr 30 min');
  });

  test('formats hours and seconds', () => {
    expect(formatISODuration('PT2H15S')).toBe('2 hr 15 s');
  });

  test('formats minutes and seconds', () => {
    expect(formatISODuration('PT5M30S')).toBe('5 min 30 s');
  });

  test('returns empty string for invalid format', () => {
    expect(formatISODuration('invalid')).toBe('');
  });

  test('returns empty string for empty string', () => {
    expect(formatISODuration('')).toBe('');
  });

  test('handles PT0H0M0S edge case', () => {
    // PT with no values - the regex won't match any groups with digits
    expect(formatISODuration('PT')).toBe('');
  });
});

describe('formatTime', () => {
  test('formats time with padded components', () => {
    const result = formatTime(new Date(2025, 0, 1, 9, 5, 3));
    expect(result).toBe('09:05:03');
  });

  test('formats midnight', () => {
    const result = formatTime(new Date(2025, 0, 1, 0, 0, 0));
    expect(result).toBe('00:00:00');
  });

  test('formats end of day', () => {
    const result = formatTime(new Date(2025, 0, 1, 23, 59, 59));
    expect(result).toBe('23:59:59');
  });

  test('formats afternoon time', () => {
    const result = formatTime(new Date(2025, 0, 1, 14, 30, 45));
    expect(result).toBe('14:30:45');
  });

  test('handles epoch timestamp', () => {
    const result = formatTime(0);
    expect(result).toMatch(/^\d{2}:\d{2}:\d{2}$/);
  });
});
