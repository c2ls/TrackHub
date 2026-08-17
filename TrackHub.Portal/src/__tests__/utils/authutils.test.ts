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

import { generateCodeVerifier, generateCodeChallenge } from 'utils/authutils';

describe('generateCodeVerifier', () => {
  test('returns a string of length 128', () => {
    const verifier = generateCodeVerifier();
    expect(verifier.length).toBe(128);
  });

  test('contains only valid characters', () => {
    const verifier = generateCodeVerifier();
    const validChars = /^[A-Za-z0-9\-._~]+$/;
    expect(verifier).toMatch(validChars);
  });

  test('generates different values on subsequent calls', () => {
    const v1 = generateCodeVerifier();
    const v2 = generateCodeVerifier();
    expect(v1).not.toBe(v2);
  });

  test('returns a string type', () => {
    expect(typeof generateCodeVerifier()).toBe('string');
  });
});

describe('generateCodeChallenge', () => {
  test('returns a non-empty string', () => {
    const challenge = generateCodeChallenge('test-verifier');
    expect(challenge.length).toBeGreaterThan(0);
  });

  test('produces URL-safe output (no +, /, =)', () => {
    const challenge = generateCodeChallenge('some-verifier-value');
    expect(challenge).not.toContain('+');
    expect(challenge).not.toContain('/');
    expect(challenge).not.toContain('=');
  });

  test('produces deterministic output for same input', () => {
    const c1 = generateCodeChallenge('same-input');
    const c2 = generateCodeChallenge('same-input');
    expect(c1).toBe(c2);
  });

  test('produces different output for different input', () => {
    const c1 = generateCodeChallenge('input-a');
    const c2 = generateCodeChallenge('input-b');
    expect(c1).not.toBe(c2);
  });

  test('handles empty string', () => {
    const challenge = generateCodeChallenge('');
    expect(challenge.length).toBeGreaterThan(0);
  });
});
