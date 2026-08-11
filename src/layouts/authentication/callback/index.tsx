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

import { useEffect } from "react";
import { useLocation } from "react-router";
import { useNavigate } from 'react-router';
import { exchangeAuthorizationCode } from "services/auth";
import type { TokenResponse } from "services/auth";
import { useAuth } from "AuthContext";

// Helpers for the stale-callback recovery below. A browser that blocks
// sessionStorage (strict privacy mode) can never pass the state check;
// detect it so we fail with a clear message instead of restarting.
const sessionStorageWorks = (): boolean => {
  try {
    sessionStorage.setItem('__storage_probe', '1');
    const ok = sessionStorage.getItem('__storage_probe') === '1';
    sessionStorage.removeItem('__storage_probe');
    return ok;
  } catch {
    return false;
  }
};

const RESTART_COUNT_KEY = 'auth_restart_count';
const MAX_AUTH_RESTARTS = 2;

const CallbackPage = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { setIsAuthenticated, setAccessToken, setRefreshToken, resetAuthError, setIsLoggingIn } = useAuth();

  useEffect(() => {
    // Clear any previous auth errors at the start of callback processing
    sessionStorage.removeItem('auth_error');
    
    const searchParams = new URLSearchParams(location.search);
    const authorizationCode = searchParams.get("code");
    const error = searchParams.get("error");

    if (error) {
      if (process.env.NODE_ENV !== 'production') {
        console.error("Authentication error:", error, searchParams.get("error_description"));
      }
      setIsLoggingIn(false); // Reset logging in state
      sessionStorage.setItem('auth_error', error);
      navigate("/error", { replace: true });
      return;
    }

    // CSRF check: the state round-tripped by the identity provider must match
    // the value login() generated for this attempt (single-use).
    const returnedState = searchParams.get('state');
    const expectedState = sessionStorage.getItem('oauth_state');
    sessionStorage.removeItem('oauth_state');
    // Stale-callback recovery: no stored state means no attempt is in flight in
    // this tab — a bookmarked sign-in/callback URL, a restored tab, or
    // back-navigation after the state was consumed. That is not an attack:
    // restart the flow so a fresh state is minted and verified, bounded so a
    // browser that silently drops storage cannot redirect forever.
    if (!expectedState && sessionStorageWorks()) {
      const restarts = Number(sessionStorage.getItem(RESTART_COUNT_KEY) ?? '0');
      if (restarts < MAX_AUTH_RESTARTS) {
        sessionStorage.setItem(RESTART_COUNT_KEY, String(restarts + 1));
        navigate("/", { replace: true });
        return;
      }
    }
    if (!expectedState || returnedState !== expectedState) {
      if (process.env.NODE_ENV !== 'production') {
        console.error('OAuth state mismatch — possible CSRF or stale callback.');
      }
      setIsLoggingIn(false);
      sessionStorage.setItem('auth_error', 'state_mismatch');
      navigate("/error", { replace: true });
      return;
    }

    if (authorizationCode) {
      // Exchange authorization code for access token
      exchangeAuthorizationCode(authorizationCode).then((data: TokenResponse) => {
        setAccessToken(data.access_token);
        setRefreshToken(data.refresh_token);
        setIsAuthenticated(true);
        setIsLoggingIn(false); // Reset logging in state
        resetAuthError(); // Reset error state on successful auth
        sessionStorage.removeItem('auth_error');
        sessionStorage.removeItem(RESTART_COUNT_KEY); // successful sign-in resets the stale-callback restart budget
        // Redirect to dashboard
        navigate("/dashboard", { replace: true });
      })
      .catch((error: unknown) => {
        if (process.env.NODE_ENV !== 'production') {
          console.error("Error exchanging authorization code:", error);
        }
        setIsLoggingIn(false); // Reset logging in state on error
        sessionStorage.setItem('auth_error', 'token_exchange_failed');
        navigate("/error", { replace: true });
      });
    } else {
      if (process.env.NODE_ENV !== 'production') {
        console.error("No authorization code received in callback");
      }
      setIsLoggingIn(false); // Reset logging in state
      sessionStorage.setItem('auth_error', 'no_code');
      navigate("/error", { replace: true });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div>
    </div>
  );
};

export default CallbackPage;