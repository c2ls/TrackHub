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

import React, { createContext, useContext, useState, useRef, useEffect } from 'react';
import { generateCodeVerifier, generateCodeChallenge } from 'utils/authutils';
import { refreshAccessToken, revokeAccessToken, logout } from 'services/auth';
import { tokenStore } from 'api/core/tokenStore';
import { OAUTH_ENDPOINTS } from 'api/core/endpoints';

export interface AuthContextValue {
  isAuthenticated: boolean;
  isLoggingIn: boolean;
  authError: boolean;
  setIsAuthenticated: (value: boolean) => void;
  setIsLoggingIn: (value: boolean) => void;
  login: () => void;
  logoff: () => Promise<void>;
  accessToken: string | null;
  setAccessToken: (token: string | null) => void;
  setRefreshToken: (token: string | null) => void;
  handleRefreshToken: () => Promise<string | undefined>;
  resetAuthError: () => void;
}

/** Public, no-auth route that must never be interrupted by the login flow (see routes.tsx). */
const PUBLIC_STATUS_ROUTE = '/status';

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

// Custom hook to consume the authentication context
export const useAuth = (): AuthContextValue => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: React.ReactNode;
  navigate: (to: string) => void;
}

interface LoginAttempts {
  count: number;
  lastAttempt: number | null;
}

// Authentication provider component
export const AuthProvider = ({ children, navigate }: AuthProviderProps) => {
  const [accessToken, setAccessTokenState] = useState<string | null>(null);
  const [refreshToken, setRefreshToken] = useState<string | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoggingIn, setIsLoggingIn] = useState(false);
  const [authError, setAuthError] = useState(false);
  const codeVerifierRef = useRef<string | undefined>(undefined);
  const loginAttemptsRef = useRef<LoginAttempts>({ count: 0, lastAttempt: null });
  const MAX_LOGIN_ATTEMPTS = 3;
  const COOLDOWN_PERIOD = 30000; // 30 seconds

  // Mirror the access token into the framework-agnostic token store so the
  // api layer (plain functions, not hooks) can authenticate requests.
  const setAccessToken = (token: string | null): void => {
    tokenStore.setAccessToken(token);
    setAccessTokenState(token);
  };

  const login = (): void => {
    // Check if we're in cooldown period
    const now = Date.now();
    if (loginAttemptsRef.current.lastAttempt) {
      const timeSinceLastAttempt = now - loginAttemptsRef.current.lastAttempt;
      if (
        loginAttemptsRef.current.count >= MAX_LOGIN_ATTEMPTS &&
        timeSinceLastAttempt < COOLDOWN_PERIOD
      ) {
        if (process.env.NODE_ENV !== 'production') {
          console.warn('Too many login attempts. Please wait before trying again.');
        }
        setAuthError(true);
        return;
      }
      // Reset counter if cooldown period has passed
      if (timeSinceLastAttempt >= COOLDOWN_PERIOD) {
        loginAttemptsRef.current.count = 0;
      }
    }

    if (!isLoggingIn && !authError) {
      setIsLoggingIn(true);
      loginAttemptsRef.current.count += 1;
      loginAttemptsRef.current.lastAttempt = now;

      if (!codeVerifierRef.current) {
        codeVerifierRef.current = generateCodeVerifier();
        sessionStorage.setItem('code_verifier', codeVerifierRef.current);
      }
      const codeChallenge = generateCodeChallenge(codeVerifierRef.current);
      const responseType = 'code';
      const scope = 'web_scope offline_access';
      // CSRF protection: random state, verified by the callback page. (Was a
      // constant '123' that the callback never checked.)
      const state = crypto.randomUUID();
      sessionStorage.setItem('oauth_state', state);

      const queryParams = new URLSearchParams({
        client_id: OAUTH_ENDPOINTS.clientId,
        redirect_uri: OAUTH_ENDPOINTS.callback,
        response_type: responseType,
        scope: scope,
        state: state,
        code_challenge: codeChallenge,
        code_challenge_method: 'S256',
      });
      const authorizationUrl = `${OAUTH_ENDPOINTS.authorization}?${queryParams.toString()}`;
      navigate(`/authentication/authorize?authorizationUrl=${encodeURIComponent(authorizationUrl)}`);
    }
  };

  const logoff = async (): Promise<void> => {
    await revokeAccessToken(accessToken);
    await logout();

    // Clear any stored tokens or user information
    setAccessToken('');
    setRefreshToken('');

    // Redirect to the login page
    login();
  };

  const handleRefreshToken = async (): Promise<string | undefined> => {
    try {
      const data = await refreshAccessToken(refreshToken);
      setAccessToken(data.access_token);
      setRefreshToken(data.refresh_token);
      return data.access_token;
    } catch (refreshError) {
      // Refresh token is also expired or invalid, redirect to login page
      if (process.env.NODE_ENV !== 'production') {
        console.error('Token refresh failed:', refreshError);
      }
      setAuthError(true);
      // ...except on the public status page. A failing refresh is often caused by the very outage
      // the visitor came to check (AuthorityServer down), and bouncing them into the login flow
      // would defeat the one screen that is supposed to survive it. The page simply degrades to its
      // anonymous tier.
      if (window.location.pathname !== PUBLIC_STATUS_ROUTE) {
        login();
      }
      return undefined;
    }
  };

  // The refresh routine closes over the current refresh token; re-register on change.
  const handleRefreshTokenRef = useRef(handleRefreshToken);
  handleRefreshTokenRef.current = handleRefreshToken;
  useEffect(() => {
    tokenStore.registerRefreshHandler(() => handleRefreshTokenRef.current());
    return () => tokenStore.registerRefreshHandler(null);
  }, []);

  const resetAuthError = (): void => {
    setAuthError(false);
    loginAttemptsRef.current = { count: 0, lastAttempt: null };
  };

  return (
    <AuthContext.Provider
      value={{
        isAuthenticated,
        isLoggingIn,
        authError,
        setIsAuthenticated,
        setIsLoggingIn,
        login,
        logoff,
        accessToken,
        setAccessToken,
        setRefreshToken,
        handleRefreshToken,
        resetAuthError,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};
