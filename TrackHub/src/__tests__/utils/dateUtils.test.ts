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

import { formatDate, formatDateTime, toISOStringWithTimezone, formatDurationString } from 'utils/dateUtils';

describe('formatDate', () => {
  test('formats a valid date correctly', () => {
    const result = formatDate(new Date(2025, 0, 15));
    expect(result).toBe('01/15/2025');
  });

  test('pads single-digit month and day', () => {
    const result = formatDate(new Date(2025, 2, 5));
    expect(result).toBe('03/05/2025');
  });

  test('handles December 31 correctly', () => {
    const result = formatDate(new Date(2025, 11, 31));
    expect(result).toBe('12/31/2025');
  });

  test('returns empty string for invalid date string', () => {
    expect(formatDate('not-a-date')).toBe('');
  });

  test('returns empty string for undefined', () => {
    expect(formatDate(undefined)).toBe('');
  });

  test('handles date from ISO string', () => {
    const result = formatDate('2025-06-15T10:30:00Z');
    expect(result).toMatch(/^\d{2}\/\d{2}\/2025$/);
  });

  test('returns empty string for null', () => {
    expect(formatDate(null)).toBe('');
  });

  test('handles epoch timestamp', () => {
    // Epoch 0 may render as 12/31/1969 or 01/01/1970 depending on timezone
    const result = formatDate(0);
    expect(result).toMatch(/^\d{2}\/\d{2}\/\d{4}$/);
  });
});

describe('formatDateTime', () => {
  test('formats date and time correctly', () => {
    const result = formatDateTime(new Date(2025, 0, 15, 14, 30, 45));
    expect(result).toBe('01/15/2025 14:30:45');
  });

  test('pads single-digit hours, minutes, seconds', () => {
    const result = formatDateTime(new Date(2025, 0, 1, 3, 5, 7));
    expect(result).toBe('01/01/2025 03:05:07');
  });

  test('handles midnight correctly', () => {
    const result = formatDateTime(new Date(2025, 0, 1, 0, 0, 0));
    expect(result).toBe('01/01/2025 00:00:00');
  });

  test('returns empty string for invalid date', () => {
    expect(formatDateTime('garbage')).toBe('');
  });

  test('returns empty string for null', () => {
    expect(formatDateTime(null)).toBe('');
  });
});

describe('toISOStringWithTimezone', () => {
  test('produces ISO format with timezone offset', () => {
    const date = new Date(2025, 5, 15, 10, 30, 45);
    const result = toISOStringWithTimezone(date);
    expect(result).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{2}:\d{2}$/);
  });

  test('includes correct year, month, day', () => {
    const date = new Date(2025, 0, 1, 0, 0, 0);
    const result = toISOStringWithTimezone(date);
    expect(result).toContain('2025-01-01');
  });

  test('includes correct time components', () => {
    const date = new Date(2025, 5, 15, 14, 30, 45);
    const result = toISOStringWithTimezone(date);
    expect(result).toContain('T14:30:45');
  });
});

describe('formatDurationString', () => {
  test('formats hours and minutes', () => {
    expect(formatDurationString(5580)).toBe('1h 33m');
  });

  test('formats zero seconds', () => {
    expect(formatDurationString(0)).toBe('0h 0m');
  });

  test('formats null', () => {
    expect(formatDurationString(null)).toBe('0h 0m');
  });

  test('formats undefined', () => {
    expect(formatDurationString(undefined)).toBe('0h 0m');
  });

  test('formats negative seconds', () => {
    expect(formatDurationString(-100)).toBe('0h 0m');
  });

  test('formats exact hours', () => {
    expect(formatDurationString(7200)).toBe('2h 0m');
  });

  test('formats seconds less than a minute', () => {
    expect(formatDurationString(45)).toBe('0h 0m');
  });

  test('formats exactly one minute', () => {
    expect(formatDurationString(60)).toBe('0h 1m');
  });

  test('formats large duration', () => {
    expect(formatDurationString(86400)).toBe('24h 0m');
  });
});
