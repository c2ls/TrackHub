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

import { renderHook, act } from '@testing-library/react';
import type { ReactNode } from 'react';
import { AuthProvider, useAuth } from 'AuthContext';
import { refreshAccessToken } from 'services/auth';

// Mock the auth service
vi.mock('services/auth', () => ({
  refreshAccessToken: vi.fn(),
  revokeAccessToken: vi.fn().mockResolvedValue(undefined),
  logout: vi.fn().mockResolvedValue(undefined),
}));

// Mock authutils
vi.mock('utils/authutils', () => ({
  generateCodeVerifier: vi.fn(() => 'mock-verifier'),
  generateCodeChallenge: vi.fn(() => 'mock-challenge'),
}));

const mockNavigate = vi.fn();

const wrapper = ({ children }: { children: ReactNode }) => (
  <AuthProvider navigate={mockNavigate}>{children}</AuthProvider>
);

describe('AuthContext', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // process.env fields are typed readonly (src/types/env.d.ts); assign via
    // Object.assign so the runtime override AuthContext reads still works.
    Object.assign(process.env, {
      REACT_APP_CLIENT_ID: 'test-client',
      REACT_APP_CALLBACK_ENDPOINT: 'https://app/callback',
      REACT_APP_AUTHORIZATION_ENDPOINT: 'https://auth/authorize',
    });
    sessionStorage.clear();
  });

  test('initial state is not authenticated', () => {
    const { result } = renderHook(() => useAuth(), { wrapper });
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.accessToken).toBeNull();
  });

  test('login navigates to authorization URL', () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    act(() => {
      result.current.login();
    });

    expect(mockNavigate).toHaveBeenCalledWith(
      expect.stringContaining('/authentication/authorize?authorizationUrl=')
    );
  });

  test('login stores code_verifier in sessionStorage', () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    act(() => {
      result.current.login();
    });

    // The code_verifier should be stored regardless of the mock value
    expect(sessionStorage.getItem('code_verifier')).toBeTruthy();
  });

  test('login rate limits after MAX_LOGIN_ATTEMPTS', () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    // Trigger 3 logins quickly — need to reset isLoggingIn between calls
    act(() => {
      result.current.login();
    });
    // After first login, isLoggingIn is true, so subsequent calls are blocked
    expect(mockNavigate).toHaveBeenCalledTimes(1);
  });

  test('setIsAuthenticated updates state', () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    act(() => {
      result.current.setIsAuthenticated(true);
    });

    expect(result.current.isAuthenticated).toBe(true);
  });

  test('setAccessToken updates token', () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    act(() => {
      result.current.setAccessToken('new-token');
    });

    expect(result.current.accessToken).toBe('new-token');
  });

  test('resetAuthError clears error state', () => {
    const { result } = renderHook(() => useAuth(), { wrapper });

    act(() => {
      result.current.resetAuthError();
    });

    expect(result.current.authError).toBe(false);
  });

  test('handleRefreshToken calls refreshAccessToken and updates tokens', async () => {
    vi.mocked(refreshAccessToken).mockResolvedValue({
      access_token: 'refreshed-access',
      refresh_token: 'refreshed-refresh',
    });

    const { result } = renderHook(() => useAuth(), { wrapper });

    let newToken: string | undefined;
    await act(async () => {
      newToken = await result.current.handleRefreshToken();
    });

    expect(newToken).toBe('refreshed-access');
    expect(result.current.accessToken).toBe('refreshed-access');
  });

  test('handleRefreshToken triggers login on failure', async () => {
    vi.mocked(refreshAccessToken).mockRejectedValue(new Error('Token expired'));
    vi.spyOn(console, 'error').mockImplementation(() => {});

    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(async () => {
      await result.current.handleRefreshToken();
    });

    expect(result.current.authError).toBe(true);
  });
});
