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

import { getStringValue } from 'utils/booleanUtils';

describe('getStringValue', () => {
  test('returns "yes" for true', () => {
    expect(getStringValue(true)).toBe('yes');
  });

  test('returns "no" for false', () => {
    expect(getStringValue(false)).toBe('no');
  });

  test('returns "no" for null (falsy)', () => {
    expect(getStringValue(null)).toBe('no');
  });

  test('returns "no" for undefined (falsy)', () => {
    expect(getStringValue(undefined)).toBe('no');
  });

  test('returns "no" for 0 (falsy)', () => {
    expect(getStringValue(0)).toBe('no');
  });

  test('returns "no" for empty string (falsy)', () => {
    expect(getStringValue('')).toBe('no');
  });

  test('returns "yes" for truthy number', () => {
    expect(getStringValue(1)).toBe('yes');
  });

  test('returns "yes" for truthy string', () => {
    expect(getStringValue('hello')).toBe('yes');
  });
});
