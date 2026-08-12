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

import { jwtDecode } from 'jwt-decode';
import { ApiError } from './errors';

/**
 * Framework-agnostic access-token holder. AuthContext (React) is the writer:
 * it mirrors its token state here and registers its refresh routine. The api
 * layer is the reader, so services stay plain async functions instead of
 * hooks. Tokens live in memory only, matching the pre-existing security
 * posture (nothing persisted to storage).
 */

type RefreshHandler = () => Promise<string | undefined>;

let accessToken: string | null = null;
let refreshHandler: RefreshHandler | null = null;
/** Module-level mutex so concurrent requests trigger a single refresh. */
let refreshPromise: Promise<string> | null = null;

function isTokenValid(token: string): boolean {
  try {
    const { exp } = jwtDecode(token);
    return exp !== undefined && exp >= Date.now() / 1000;
  } catch {
    return false;
  }
}

export const tokenStore = {
  setAccessToken(token: string | null): void {
    accessToken = token;
  },

  getAccessToken(): string | null {
    return accessToken;
  },

  /** AuthContext registers its handleRefreshToken here (returns the new access token). */
  registerRefreshHandler(handler: RefreshHandler | null): void {
    refreshHandler = handler;
  },

  /**
   * Returns a currently-valid access token, refreshing (once, shared across
   * concurrent callers) when the held token is missing or expired.
   */
  async acquireValidAccessToken(): Promise<string> {
    if (accessToken && isTokenValid(accessToken)) {
      return accessToken;
    }
    if (refreshPromise) {
      return refreshPromise;
    }
    if (!refreshHandler) {
      throw new ApiError('Not authenticated: no token and no refresh handler registered.', {
        code: 'NOT_AUTHENTICATED',
      });
    }
    refreshPromise = refreshHandler()
      .then((token) => {
        if (!token) {
          throw new ApiError('Token refresh did not produce an access token.', {
            code: 'NOT_AUTHENTICATED',
          });
        }
        return token;
      })
      .finally(() => {
        refreshPromise = null;
      });
    return refreshPromise;
  },

  /** Test/logout hook: drop all state. */
  reset(): void {
    accessToken = null;
    refreshHandler = null;
    refreshPromise = null;
  },
};
