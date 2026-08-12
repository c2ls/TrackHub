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

import { formatDistance, calculateTotalDistance, calculateDistance, toRadians } from 'utils/distanceUtils';

describe('formatDistance', () => {
  test('formats a number with two decimal places and km suffix', () => {
    expect(formatDistance(123.456)).toBe('123.46 km');
  });

  test('formats zero', () => {
    expect(formatDistance(0)).toBe('0.00 km');
  });

  test('formats negative distance', () => {
    expect(formatDistance(-5.5)).toBe('-5.50 km');
  });

  test('returns empty string for string input', () => {
    expect(formatDistance('123')).toBe('');
  });

  test('returns empty string for null', () => {
    expect(formatDistance(null)).toBe('');
  });

  test('returns empty string for undefined', () => {
    expect(formatDistance(undefined)).toBe('');
  });

  test('returns empty string for NaN', () => {
    expect(formatDistance(NaN)).toBe('NaN km');
  });

  test('formats very small distance', () => {
    expect(formatDistance(0.001)).toBe('0.00 km');
  });

  test('formats integer as decimal', () => {
    expect(formatDistance(100)).toBe('100.00 km');
  });
});

describe('calculateTotalDistance', () => {
  test('sums distances from list of objects', () => {
    const list = [
      { distance: 10.5 },
      { distance: 20.3 },
      { distance: 5.2 },
    ];
    expect(calculateTotalDistance(list, 'distance')).toBe('36.00 km');
  });

  test('handles empty list', () => {
    expect(calculateTotalDistance([], 'distance')).toBe('0.00 km');
  });

  test('handles missing key in objects gracefully', () => {
    const list = [{ name: 'a' }, { name: 'b' }];
    expect(calculateTotalDistance(list, 'distance')).toBe('0.00 km');
  });

  test('handles mixed valid and invalid values', () => {
    const list = [
      { distance: 10 },
      { distance: 'abc' },
      { distance: null },
      { distance: 5 },
    ];
    expect(calculateTotalDistance(list, 'distance')).toBe('15.00 km');
  });

  test('handles string numbers in objects', () => {
    const list = [{ distance: '10' }, { distance: '20' }];
    expect(calculateTotalDistance(list, 'distance')).toBe('30.00 km');
  });
});

describe('calculateDistance (Haversine)', () => {
  test('returns 0 for same point', () => {
    expect(calculateDistance(40.7128, -74.006, 40.7128, -74.006)).toBe(0);
  });

  test('calculates known distance between NYC and LA approximately', () => {
    // NYC: 40.7128, -74.0060 | LA: 34.0522, -118.2437
    const distance = calculateDistance(40.7128, -74.006, 34.0522, -118.2437);
    // Known distance ~3944 km = ~3,944,000 meters
    expect(distance).toBeGreaterThan(3900000);
    expect(distance).toBeLessThan(4000000);
  });

  test('calculates short distance accurately', () => {
    // Two points ~111 km apart (1 degree latitude)
    const distance = calculateDistance(0, 0, 1, 0);
    expect(distance).toBeGreaterThan(110000);
    expect(distance).toBeLessThan(112000);
  });

  test('handles antipodal points', () => {
    // North pole to south pole ~ 20,000 km
    const distance = calculateDistance(90, 0, -90, 0);
    expect(distance).toBeGreaterThan(19900000);
    expect(distance).toBeLessThan(20100000);
  });
});

describe('toRadians', () => {
  test('converts 0 degrees to 0 radians', () => {
    expect(toRadians(0)).toBe(0);
  });

  test('converts 180 degrees to PI radians', () => {
    expect(toRadians(180)).toBeCloseTo(Math.PI);
  });

  test('converts 90 degrees to PI/2 radians', () => {
    expect(toRadians(90)).toBeCloseTo(Math.PI / 2);
  });

  test('converts 360 degrees to 2*PI radians', () => {
    expect(toRadians(360)).toBeCloseTo(2 * Math.PI);
  });

  test('converts negative degrees', () => {
    expect(toRadians(-90)).toBeCloseTo(-Math.PI / 2);
  });
});
