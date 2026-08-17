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
 * DateOnly rendering pinned to a NEGATIVE UTC offset (America/Bogota, UTC-5) —
 * the timezone the regression was reported from. `new Date('2027-03-04')` parses
 * as UTC midnight, so any local getter read off it lands on the previous day
 * there; these helpers must not.
 *
 * TZ is assigned before the module under test is imported, and the first test
 * asserts the process really picked it up so the suite can never pass vacuously.
 */
export {};

process.env.TZ = 'America/Bogota';

const { formatDateOnly, daysUntilDateOnly, formatDateTime } = await import('utils/dateUtils');
const { daysUntil, expiryColor } = await import(
  'layouts/manageadmin/components/drivers/workforceShared'
);

describe('negative-offset timezone fixture', () => {
  test('the process is really running at UTC-5', () => {
    expect(new Date(2027, 2, 4).getTimezoneOffset()).toBe(300);
    // The exact trap: UTC-midnight parse read through local getters.
    expect(new Date('2027-03-04').getDate()).toBe(3);
  });
});

describe('formatDateOnly', () => {
  test('renders the stored calendar day, not the day before', () => {
    expect(formatDateOnly('2027-03-04')).toBe('03/04/2027');
  });

  test('is stable for a January 1st value', () => {
    expect(formatDateOnly('2027-01-01')).toBe('01/01/2027');
  });

  test('accepts a LocalDate carrying a time part', () => {
    expect(formatDateOnly('2027-03-04T00:00:00')).toBe('03/04/2027');
  });

  test('pads single-digit month and day', () => {
    expect(formatDateOnly('2027-07-05')).toBe('07/05/2027');
  });

  test('returns empty for null, undefined, empty and unparseable values', () => {
    expect(formatDateOnly(null)).toBe('');
    expect(formatDateOnly(undefined)).toBe('');
    expect(formatDateOnly('')).toBe('');
    expect(formatDateOnly('not-a-date')).toBe('');
    expect(formatDateOnly('2027-13-04')).toBe('');
  });
});

describe('daysUntilDateOnly', () => {
  // Late evening local time on 2027-03-01 — 03:30 UTC on the 2nd, the window
  // where a UTC-based count is off by one.
  const lateEvening = new Date(2027, 2, 1, 22, 30, 0);

  test('counts whole calendar days from today', () => {
    expect(daysUntilDateOnly('2027-03-04', lateEvening)).toBe(3);
  });

  test('is zero on the expiry day itself, even late at night', () => {
    expect(daysUntilDateOnly('2027-03-01', lateEvening)).toBe(0);
  });

  test('is negative once the day has passed', () => {
    expect(daysUntilDateOnly('2027-02-27', lateEvening)).toBe(-2);
  });

  test('crosses a month boundary correctly', () => {
    expect(daysUntilDateOnly('2027-04-01', lateEvening)).toBe(31);
  });

  test('crosses a DST-style boundary without drifting', () => {
    expect(daysUntilDateOnly('2027-11-30', new Date(2027, 9, 31, 23, 59, 0))).toBe(30);
  });

  test('returns null for absent or unparseable values', () => {
    expect(daysUntilDateOnly(null)).toBeNull();
    expect(daysUntilDateOnly('nope')).toBeNull();
  });
});

describe('workforce expiry helpers at UTC-5', () => {
  test('daysUntil delegates to the DateOnly-aware count', () => {
    const today = new Date();
    const iso = (offsetDays: number) => {
      const target = new Date(today.getFullYear(), today.getMonth(), today.getDate() + offsetDays);
      return `${target.getFullYear()}-${String(target.getMonth() + 1).padStart(2, '0')}-${String(target.getDate()).padStart(2, '0')}`;
    };
    expect(daysUntil(iso(0))).toBe(0);
    expect(daysUntil(iso(8))).toBe(8);
    expect(daysUntil(iso(-1))).toBe(-1);
  });

  test('severity colour lands on the right side of the 7-day threshold', () => {
    const today = new Date();
    const iso = (offsetDays: number) => {
      const target = new Date(today.getFullYear(), today.getMonth(), today.getDate() + offsetDays);
      return `${target.getFullYear()}-${String(target.getMonth() + 1).padStart(2, '0')}-${String(target.getDate()).padStart(2, '0')}`;
    };
    expect(expiryColor(iso(7))).toBe('error');
    expect(expiryColor(iso(8))).toBe('warning');
    expect(expiryColor(iso(-1))).toBe('dark');
    expect(expiryColor(null)).toBe('secondary');
  });
});

describe('timestamp formatting is untouched', () => {
  test('formatDateTime still converts an instant to the viewer timezone', () => {
    // 2027-03-04T02:00:00Z is 2027-03-03 21:00 in Bogota — a DateTimeOffset
    // legitimately renders in local time; only DateOnly must not shift.
    expect(formatDateTime('2027-03-04T02:00:00Z')).toBe('03/03/2027 21:00:00');
  });

  test('formatDateTime keeps rendering local wall-clock Date inputs', () => {
    expect(formatDateTime(new Date(2027, 2, 4, 14, 5, 6))).toBe('03/04/2027 14:05:06');
  });
});
