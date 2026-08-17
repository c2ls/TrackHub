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

import {
  credentialState,
  credentialActions,
} from 'layouts/manageadmin/components/drivers/credentialLifecycle';

const NOW = Date.parse('2026-07-20T12:00:00Z');

describe('credentialState', () => {
  test('a credential created with Active unchecked is Pending, not Revoked', () => {
    expect(credentialState({ active: false, verifiedAt: null }, NOW)).toBe('pending');
  });

  test('a credential created active but never activated is Pending', () => {
    expect(credentialState({ active: true, verifiedAt: null }, NOW)).toBe('pending');
  });

  test('a verified credential with Active cleared is Revoked', () => {
    expect(
      credentialState({ active: false, verifiedAt: '2026-07-01T00:00:00Z' }, NOW)
    ).toBe('revoked');
  });

  test('a future lock reads Locked', () => {
    expect(
      credentialState(
        { active: true, verifiedAt: '2026-07-01T00:00:00Z', lockedUntil: '2026-07-20T13:00:00Z' },
        NOW
      )
    ).toBe('locked');
  });

  test('an expired lock reads Active again', () => {
    expect(
      credentialState(
        { active: true, verifiedAt: '2026-07-01T00:00:00Z', lockedUntil: '2026-07-20T11:00:00Z' },
        NOW
      )
    ).toBe('active');
  });

  test('a verified, active, unlocked credential is Active', () => {
    expect(credentialState({ active: true, verifiedAt: '2026-07-01T00:00:00Z' }, NOW)).toBe(
      'active'
    );
  });
});

describe('credentialActions', () => {
  test('an inactive credential is never a dead end — activate stays available', () => {
    const actions = credentialActions({ active: false, verifiedAt: null });
    expect(actions.activate).toBe(true);
  });

  test('a revoked credential can be re-activated and nothing else', () => {
    const actions = credentialActions({ active: false, verifiedAt: '2026-07-01T00:00:00Z' });
    expect(actions).toEqual({ activate: true, lock: false, reset: false, revoke: false });
  });

  test('an active credential exposes the whole lifecycle', () => {
    const actions = credentialActions({ active: true, verifiedAt: '2026-07-01T00:00:00Z' });
    expect(actions).toEqual({ activate: true, lock: true, reset: true, revoke: true });
  });

  test('a locked credential keeps its recovery actions', () => {
    const actions = credentialActions({
      active: true,
      verifiedAt: '2026-07-01T00:00:00Z',
      lockedUntil: '2026-07-20T13:00:00Z',
    });
    expect(actions.activate).toBe(true);
    expect(actions.reset).toBe(true);
  });

  test('every state offers at least one action (AC7: the lifecycle is runnable)', () => {
    const states = [
      { active: false, verifiedAt: null },
      { active: true, verifiedAt: null },
      { active: false, verifiedAt: '2026-07-01T00:00:00Z' },
      { active: true, verifiedAt: '2026-07-01T00:00:00Z' },
      {
        active: true,
        verifiedAt: '2026-07-01T00:00:00Z',
        lockedUntil: '2026-07-20T13:00:00Z',
      },
    ];
    states.forEach((credential) => {
      expect(Object.values(credentialActions(credential)).some(Boolean)).toBe(true);
    });
  });
});
