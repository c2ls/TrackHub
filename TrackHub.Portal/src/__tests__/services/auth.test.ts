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

import axios from 'axios';
import { exchangeAuthorizationCode, refreshAccessToken, revokeAccessToken, logout } from 'services/auth';

vi.mock('axios');

const MOCK_TOKEN_ENDPOINT = 'https://auth.example.com/token';
const MOCK_REVOKE_ENDPOINT = 'https://auth.example.com/revoke';
const MOCK_LOGOUT_ENDPOINT = 'https://auth.example.com/logout';
const MOCK_CLIENT_ID = 'test-client-id';
const MOCK_CALLBACK = 'https://app.example.com/callback';

beforeAll(() => {
  // process.env fields are typed readonly (src/types/env.d.ts); assign via
  // Object.assign so the runtime override the auth service reads still works.
  Object.assign(process.env, {
    REACT_APP_TOKEN_ENDPOINT: MOCK_TOKEN_ENDPOINT,
    REACT_APP_REVOKE_TOKEN_ENDPOINT: MOCK_REVOKE_ENDPOINT,
    REACT_APP_LOGOUT_ENDPOINT: MOCK_LOGOUT_ENDPOINT,
    REACT_APP_CLIENT_ID: MOCK_CLIENT_ID,
    REACT_APP_CALLBACK_ENDPOINT: MOCK_CALLBACK,
  });
});

beforeEach(() => {
  vi.clearAllMocks();
  vi.spyOn(console, 'error').mockImplementation(() => {});
  vi.spyOn(console, 'log').mockImplementation(() => {});
});

afterEach(() => {
  vi.restoreAllMocks();
});

describe('exchangeAuthorizationCode', () => {
  test('sends correct request and returns token data', async () => {
    const mockResponse = {
      status: 200,
      data: { access_token: 'abc123', refresh_token: 'ref456' },
    };
    vi.mocked(axios.post).mockResolvedValue(mockResponse);
    sessionStorage.setItem('code_verifier', 'test-verifier');

    const result = await exchangeAuthorizationCode('auth-code-123');

    expect(axios.post).toHaveBeenCalledWith(
      MOCK_TOKEN_ENDPOINT,
      expect.objectContaining({
        grant_type: 'authorization_code',
        code: 'auth-code-123',
        redirect_uri: MOCK_CALLBACK,
        code_verifier: 'test-verifier',
        client_id: MOCK_CLIENT_ID,
      }),
      expect.objectContaining({
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      })
    );
    expect(result).toEqual({ access_token: 'abc123', refresh_token: 'ref456' });
  });

  test('throws error on non-200 status', async () => {
    vi.mocked(axios.post).mockResolvedValue({ status: 400, data: {} });

    await expect(exchangeAuthorizationCode('bad-code')).rejects.toThrow();
  });

  test('throws error on network failure', async () => {
    vi.mocked(axios.post).mockRejectedValue(new Error('Network error'));

    await expect(exchangeAuthorizationCode('code')).rejects.toThrow('Network error');
  });
});

describe('refreshAccessToken', () => {
  test('sends refresh token and returns new tokens', async () => {
    const mockData = { access_token: 'new-access', refresh_token: 'new-refresh' };
    vi.mocked(axios.post).mockResolvedValue({ data: mockData });

    const result = await refreshAccessToken('old-refresh-token');

    expect(axios.post).toHaveBeenCalledWith(
      MOCK_TOKEN_ENDPOINT,
      expect.any(URLSearchParams),
      expect.objectContaining({
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      })
    );
    expect(result).toEqual(mockData);
  });

  test('throws wrapped error on failure', async () => {
    vi.mocked(axios.post).mockRejectedValue(new Error('Bad request'));

    await expect(refreshAccessToken('bad-token')).rejects.toThrow('Failed to refresh access token');
  });
});

describe('revokeAccessToken', () => {
  test('sends revoke request with token', async () => {
    vi.mocked(axios.post).mockResolvedValue({ data: 'ok' });

    await revokeAccessToken('my-access-token');

    expect(axios.post).toHaveBeenCalledWith(
      MOCK_REVOKE_ENDPOINT,
      expect.any(String),
      expect.objectContaining({
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      })
    );
  });

  test('does not throw on failure, logs error instead', async () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    vi.mocked(axios.post).mockRejectedValue(new Error('Revoke failed'));

    await expect(revokeAccessToken('token')).resolves.not.toThrow();
    expect(consoleSpy).toHaveBeenCalled();
  });
});

describe('logout', () => {
  test('sends logout request with credentials', async () => {
    vi.mocked(axios.post).mockResolvedValue({});

    await logout();

    expect(axios.post).toHaveBeenCalledWith(
      MOCK_LOGOUT_ENDPOINT,
      {},
      expect.objectContaining({
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        withCredentials: true,
      })
    );
  });

  test('does not throw on failure, logs error instead', async () => {
    const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    vi.mocked(axios.post).mockRejectedValue(new Error('Logout failed'));

    await expect(logout()).resolves.not.toThrow();
    expect(consoleSpy).toHaveBeenCalled();
  });
});
