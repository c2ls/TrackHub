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
import { OAUTH_ENDPOINTS } from 'api/core/endpoints';

/** OAuth token endpoint response (OpenIddict). */
export interface TokenResponse {
  access_token: string;
  refresh_token: string;
  token_type?: string;
  expires_in?: number;
  scope?: string;
}

/**
 * Exchanges the authorization code for an access token (PKCE: the
 * code_verifier persisted by AuthContext.login is read from sessionStorage).
 * @throws If the exchange fails.
 */
export const exchangeAuthorizationCode = async (
  authorizationCode: string
): Promise<TokenResponse> => {
  const codeVerifier = sessionStorage.getItem('code_verifier');
  const requestBody = {
    grant_type: 'authorization_code',
    code: authorizationCode,
    redirect_uri: OAUTH_ENDPOINTS.callback,
    code_verifier: codeVerifier,
    client_id: OAUTH_ENDPOINTS.clientId,
  };

  try {
    const response = await axios.post<TokenResponse>(
      OAUTH_ENDPOINTS.token,
      requestBody,
      {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
      }
    );

    if (response.status !== 200) {
      throw new Error('Failed to exchange authorization code for access token');
    }

    return response.data;
  } catch (error) {
    if (process.env.NODE_ENV !== 'production') {
      console.error(
        'Error exchanging authorization code:',
        error instanceof Error ? error.message : error
      );
    }
    throw error;
  }
};

/**
 * Refreshes the access token using the refresh token.
 * @throws If the refresh fails.
 */
export async function refreshAccessToken(refreshToken: string | null): Promise<TokenResponse> {
  try {
    const response = await axios.post<TokenResponse>(
      OAUTH_ENDPOINTS.token,
      new URLSearchParams({
        grant_type: 'refresh_token',
        refresh_token: refreshToken ?? '',
        client_id: OAUTH_ENDPOINTS.clientId,
      }),
      {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
      }
    );

    return response.data;
  } catch {
    throw new Error('Failed to refresh access token');
  }
}

/**
 * Revokes the access token. Failures are logged (non-production) and
 * swallowed — revocation is best-effort during logoff.
 */
export async function revokeAccessToken(accessToken: string | null): Promise<void> {
  const revokeBody = new URLSearchParams({
    token: accessToken ?? '',
    client_id: OAUTH_ENDPOINTS.clientId,
  });

  try {
    await axios.post(OAUTH_ENDPOINTS.revocation, revokeBody.toString(), {
      headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
      },
    });
  } catch (error) {
    if (process.env.NODE_ENV !== 'production') {
      console.error('Error revoking token:', error instanceof Error ? error.message : error);
    }
  }
}

/**
 * Ends the server-side session (cookie-based). Failures are logged
 * (non-production) and swallowed.
 */
export async function logout(): Promise<void> {
  try {
    await axios.post(
      OAUTH_ENDPOINTS.logout,
      {},
      {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded',
        },
        withCredentials: true,
      }
    );
  } catch (error) {
    if (process.env.NODE_ENV !== 'production') {
      console.error('Error during logout:', error instanceof Error ? error.message : error);
    }
  }
}
