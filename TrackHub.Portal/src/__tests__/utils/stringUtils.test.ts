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

import { toCamelCase, cleanString } from 'utils/stringUtils';

describe('toCamelCase', () => {
  test('converts snake_case to camelCase', () => {
    expect(toCamelCase('hello_world')).toBe('helloWorld');
  });

  test('converts kebab-case to camelCase', () => {
    expect(toCamelCase('hello-world')).toBe('helloWorld');
  });

  test('converts PascalCase to camelCase', () => {
    expect(toCamelCase('HelloWorld')).toBe('helloWorld');
  });

  test('handles already camelCase', () => {
    expect(toCamelCase('helloWorld')).toBe('helloWorld');
  });

  test('handles single word', () => {
    expect(toCamelCase('hello')).toBe('hello');
  });

  test('handles multiple separators', () => {
    expect(toCamelCase('my_long_variable_name')).toBe('myLongVariableName');
  });

  test('handles empty string', () => {
    expect(toCamelCase('')).toBe('');
  });

  test('converts mixed separators', () => {
    expect(toCamelCase('hello-world_foo')).toBe('helloWorldFoo');
  });

  test('treats dots as separators (I18N-01 feature keys)', () => {
    expect(toCamelCase('notifications.email')).toBe('notificationsEmail');
    expect(toCamelCase('gps.integration')).toBe('gpsIntegration');
    expect(toCamelCase('gps.positionHistory')).toBe('gpsPositionHistory');
  });
});

describe('cleanString', () => {
  test('removes special characters', () => {
    expect(cleanString('Hello, World!')).toBe('helloworld');
  });

  test('removes spaces', () => {
    expect(cleanString('hello world')).toBe('helloworld');
  });

  test('preserves alphanumeric characters', () => {
    expect(cleanString('abc123')).toBe('abc123');
  });

  test('handles empty string', () => {
    expect(cleanString('')).toBe('');
  });

  test('removes all non-alphanumeric', () => {
    expect(cleanString('!@#$%^&*()')).toBe('');
  });

  test('converts to lowercase', () => {
    expect(cleanString('HELLO')).toBe('hello');
  });

  test('handles string with unicode characters', () => {
    expect(cleanString('café')).toBe('caf');
  });
});
