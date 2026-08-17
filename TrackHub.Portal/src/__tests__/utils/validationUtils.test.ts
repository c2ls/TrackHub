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

import { validateEmail, validatePassword } from 'utils/validationUtils';

describe('validateEmail', () => {
  test('returns true for valid email', () => {
    expect(validateEmail('user@example.com')).toBe(true);
  });

  test('returns true for email with subdomain', () => {
    expect(validateEmail('user@mail.example.com')).toBe(true);
  });

  test('returns true for email with plus sign', () => {
    expect(validateEmail('user+tag@example.com')).toBe(true);
  });

  test('returns false for email without @', () => {
    expect(validateEmail('userexample.com')).toBeFalsy();
  });

  test('returns false for email without domain', () => {
    expect(validateEmail('user@')).toBeFalsy();
  });

  test('returns false for email without local part', () => {
    expect(validateEmail('@example.com')).toBeFalsy();
  });

  test('returns false for email with spaces', () => {
    expect(validateEmail('user @example.com')).toBeFalsy();
  });

  test('returns false for empty string', () => {
    expect(validateEmail('')).toBeFalsy();
  });

  test('returns false for null', () => {
    expect(validateEmail(null)).toBeFalsy();
  });

  test('returns false for undefined', () => {
    expect(validateEmail(undefined)).toBeFalsy();
  });
});

describe('validatePassword', () => {
  test('accepts a password meeting the full policy', () => {
    expect(validatePassword('Passw0rd')).toBe(true);
  });

  test('accepts a long compliant password', () => {
    expect(validatePassword('AveryLongPassword123')).toBe(true);
  });

  test('rejects fewer than 8 characters even when complex', () => {
    expect(validatePassword('Pass1ab')).toBeFalsy();
  });

  test('rejects a password without an uppercase letter', () => {
    expect(validatePassword('password1')).toBeFalsy();
  });

  test('rejects a password without a lowercase letter', () => {
    expect(validatePassword('PASSWORD1')).toBeFalsy();
  });

  test('rejects a password without a digit', () => {
    expect(validatePassword('PasswordX')).toBeFalsy();
  });

  test('rejects digits-only passwords', () => {
    expect(validatePassword('12345678')).toBeFalsy();
  });

  test('returns false for empty password', () => {
    expect(validatePassword('')).toBeFalsy();
  });

  test('returns false for null', () => {
    expect(validatePassword(null)).toBeFalsy();
  });

  test('returns false for undefined', () => {
    expect(validatePassword(undefined)).toBeFalsy();
  });
});
