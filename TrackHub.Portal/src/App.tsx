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

/**
=========================================================
* Argon Dashboard 2 MUI - v3.0.1
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-material-ui
* Copyright 2023 Creative Tim (https://www.creative-tim.com)

Coded by www.creative-tim.com

 =========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
*/
import { useState, useEffect } from 'react';
import type { ReactNode } from 'react';

// react-router components
import { Routes, Route, Navigate, useLocation } from "react-router-dom";

// @mui material components
import { ThemeProvider } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import GlobalStyles from "@mui/material/GlobalStyles";
import Icon from "@mui/material/Icon";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";

// Argon Dashboard 2 MUI example components
import Sidenav from "controls/Sidenav";
import Configurator from "controls/Configurator";

// Argon Dashboard 2 MUI themes
import theme from "assets/theme";
import themeDark from "assets/theme-dark";

// Argon Dashboard 2 MUI routes
import routes from "routes";
import type { RouteDefinition } from "routes";

// Argon Dashboard 2 MUI contexts
import {
  useArgonController,
  setOpenConfigurator,
  setDarkSidenav,
  setMiniSidenav,
  setDarkMode,
 } from "context";

// Images
import brand from "assets/images/logo-th.svg";
import brandDark from "assets/images/logo-th-dark.svg";

// Icon Fonts
import "assets/css/nucleo-icons.css";
import "assets/css/nucleo-svg.css";

import { useAuth } from "AuthContext";
import { LoadingContext } from 'LoadingContext';
import { ClipLoader } from 'react-spinners';
import { isAdmin, isManager } from "api/security/users";
import { getUserSettings, getAccountSettings, updateAccountSettings } from "api/manager/settings";
import type { AccountSettings, AccountSettingsDtoInput } from "api/manager/settings";
import { getAccountContext } from "api/manager/accounts";
import type { AccountContext, AccountStatus } from "api/manager/accounts";
import { getCurrentPrincipal } from "api/manager/principals";
import type { CurrentPrincipal } from "api/manager/principals";
import { notifyApiError } from "api/core/errors";
import { useTranslation } from 'react-i18next';
import ErrorBoundary from "components/ErrorBoundary";
import SuspensionScreen from "components/SuspensionScreen";
import PrincipalTypes from "constants/principalTypes";
import HelpProvider from "context/help";
import { FeaturesContext, isFeatureActive } from "context/features";
import AnnouncementBanner from "components/AnnouncementBanner";

/** Public, chrome-less route (see routes.tsx `platformStatus`). */
const STATUS_ROUTE = "/status";

export default function App() {
  const [controller, dispatch] = useArgonController();
  const { miniSidenav, layout, openConfigurator, darkMode } = controller;
  const [onMouseEnter, setOnMouseEnter] = useState(false);
  const { isAuthenticated, login, isLoggingIn, authError } = useAuth();
  const { pathname } = useLocation();
  const { i18n } = useTranslation();

  // Account settings save is fire-and-forget from the Configurator; keep the
  // legacy toast-on-failure semantics (the new api function throws).
  const saveAccountSettings = (
    accountId: string,
    settings: Omit<AccountSettingsDtoInput, 'accountId'>
  ) => updateAccountSettings(accountId, settings).catch(notifyApiError);

  const [loading, setLoading] = useState(false);
  const [userIsAdmin, setUserIsAdmin] = useState(true);
  const [userIsManager, setUserIsManager] = useState(true);
  const [accountSettings, setAccountSettings] = useState<Partial<AccountSettings>>({});
  const [accountFeatures, setAccountFeatures] = useState<AccountContext['features']>([]);
  const [accountStatus, setAccountStatus] = useState<AccountStatus | null>(null);
  const [branding, setBranding] = useState<AccountContext['branding'] | null>(null);
  const [currentPrincipal, setCurrentPrincipal] = useState<CurrentPrincipal | null>(null);

  useEffect(() => {
    // Redirect to login page if not authenticated
    // Skip redirect if: on callback page, already logging in, on error page, or auth error occurred
    const isAuthRoute = pathname.startsWith("/authentication/");
    const isErrorPage = pathname === "/error";
    // The platform status page is deliberately public: its whole purpose is to
    // answer "can nobody sign in?", so it must never bounce to the login flow.
    const isStatusPage = pathname === STATUS_ROUTE;

    if (!isAuthenticated && !isLoggingIn && !isAuthRoute && !isErrorPage && !isStatusPage && !authError) {
      login();
    }

  }, [isAuthenticated, isLoggingIn, login, pathname, authError]);

  useEffect(() => {
    const fetchPermissions = async () => {
      if (isAuthenticated) {
        // Graceful-null on failure (matches the old service's null fallback);
        // routes fall back to the default principal type.
        const principal = await getCurrentPrincipal().catch(() => null);
        setCurrentPrincipal(principal);
        // Silent ops: default to false on failure (matches the old service's
        // handleSilentError — no toast, routes stay locked down).
        const admin = await isAdmin().catch(() => false);
        const manager = await isManager().catch(() => false);
        const userSettings = await getUserSettings();
        setUserIsAdmin(admin);
        setUserIsManager(manager);
        /* Initialize settings */
        setDarkMode(dispatch, userSettings.style !== 'light');
        setDarkSidenav(dispatch, userSettings.style !== 'light');
        setMiniSidenav(dispatch, userSettings.navbar !== 'none');
        if (userSettings.language) {
          i18n.changeLanguage(userSettings.language);
        }
        const settings = await getAccountSettings();
        setAccountSettings(settings);
        // Single bootstrap read (status + branding + features); allowed on non-operational accounts
        // so the shell can render a suspension state instead of issuing operational queries.
        const context = await getAccountContext().catch(() => null);
        if (context) {
          setAccountStatus(context.status);
          setBranding(context.branding);
          setAccountFeatures(context.features || []);
        }
      }
    };
    fetchPermissions();
  }, [isAuthenticated]);

  // Open sidenav when mouse enter on mini sidenav
  const handleOnMouseEnter = () => {
    if (miniSidenav && !onMouseEnter) {
      setMiniSidenav(dispatch, false);
      setOnMouseEnter(true);
    }
  };

  // Close sidenav when mouse leave mini sidenav
  const handleOnMouseLeave = () => {
    if (onMouseEnter) {
      setMiniSidenav(dispatch, true);
      setOnMouseEnter(false);
    }
  };

  // Change the openConfigurator state
  const handleConfiguratorOpen = () => setOpenConfigurator(dispatch, !openConfigurator);

  // The Argon template's RTL support was dropped in this fork: the controller
  // has no `direction` state, so the app is LTR-only. The old effect wrote the
  // invalid dir="undefined" to <body>; it is intentionally gone.

  // Setting page scroll to 0 when changing the route
  useEffect(() => {
    document.documentElement.scrollTop = 0;
    if (document.scrollingElement) {
      document.scrollingElement.scrollTop = 0;
    }
  }, [pathname]);

  // Operational statuses (Trial/Active) permit normal access; anything else renders a suspension shell.
  const accountOperational = !accountStatus || accountStatus === 'TRIAL' || accountStatus === 'ACTIVE';

  // The status page renders chrome-less for everyone (signed in or not) and stays
  // reachable on a suspended account — it is the screen you check when things break.
  const onStatusPage = pathname === STATUS_ROUTE;

  // Shared with FeaturesContext consumers; matches the backend flag semantics
  // (missing row ⇒ disabled, effective window honoured).
  const featureEnabled = (featureKey?: string | null): boolean =>
    isFeatureActive(accountFeatures, featureKey);

  const filterRoutesByFeatures = (allRoutes: RouteDefinition[]): RouteDefinition[] =>
    allRoutes
      .filter(route => featureEnabled(route.featureKey))
      .map(route => route.collapse ? { ...route, collapse: filterRoutesByFeatures(route.collapse) } : route);

  const enabledRoutes = filterRoutesByFeatures(routes);

  const routeAllowed = (route: RouteDefinition): boolean => {
    // Public routes bypass every gate. Without this a non-User principal (Driver, public link)
    // would fail the default [User] check and be redirected to /dashboard — which is itself
    // User-only, so it would redirect again: an infinite navigation loop.
    if (route.public) return true;
    if (route.key === 'systemAdmin' && !userIsAdmin) return false;
    if (route.key === 'manageAdmin' && !userIsManager) return false;
    if (route.key === 'gpsIntegration' && !userIsManager) return false;
    if (currentPrincipal?.principalType) {
      const allowedPrincipalTypes = route.principalTypes || [PrincipalTypes.User];
      if (!allowedPrincipalTypes.includes(currentPrincipal.principalType)) return false;
    }
    return true;
  };

  // Screens the principal can actually reach (feature + role filtered) — the
  // help index hides topics for screens outside this set (UX filtering, CH-01).
  const allowedScreens = enabledRoutes
    .filter(route => route.type === 'route' && !!route.route && routeAllowed(route))
    .map(route => route.key);

  const getRoutes = (allRoutes: RouteDefinition[]): ReactNode =>
    allRoutes.map((route) => {
      if (route.collapse) {
        return getRoutes(route.collapse);
      }

      if (route.route) {
        return <Route path={route.route} element={routeAllowed(route) ? route.component : <Navigate to="/dashboard" replace />} key={route.key} />;
      }

      return null;
    });

  const configsButton = (
    <ArgonBox
      display="flex"
      justifyContent="center"
      alignItems="center"
      width="3.5rem"
      height="3.5rem"
      bgColor="white"
      shadow="sm"
      borderRadius="50%"
      position="fixed"
      right="2rem"
      bottom="2rem"
      zIndex={99}
      color="dark"
      sx={{ cursor: "pointer" }}
      onClick={handleConfiguratorOpen}
    >
      <Icon color="inherit">
        settings
      </Icon>
    </ArgonBox>
  );

  return (
    <LoadingContext.Provider value={{ loading, setLoading }}>
      <ThemeProvider theme={darkMode ? themeDark : theme}>
        <CssBaseline />
        {/* The page itself never scrolls laterally (full-bleed bands, sub-pixel grid rounding
            under browser zoom, etc. must clip); wide content scrolls inside its own container. */}
        <GlobalStyles styles={{ html: { overflowX: "clip" }, body: { overflowX: "clip" } }} />
        <ErrorBoundary>
          {isAuthenticated && !accountOperational && !onStatusPage ? (
            <SuspensionScreen status={accountStatus} branding={branding} />
          ) : (
          <FeaturesContext.Provider value={{ features: accountFeatures, isFeatureEnabled: featureEnabled }}>
          <HelpProvider allowedScreens={allowedScreens} isFeatureEnabled={featureEnabled}>
          {layout === "dashboard" && !onStatusPage && (
          <>
            <Sidenav
              brand={darkMode ? brand : brandDark}
              brandName="Track Hub"
              routes={enabledRoutes}
              onMouseEnter={handleOnMouseEnter}
              onMouseLeave={handleOnMouseLeave}
              isAdmin={userIsAdmin}
              isManager={userIsManager}
              currentPrincipal={currentPrincipal}
            />
            <Configurator
              settings={accountSettings}
              updateSettings={saveAccountSettings}
               />
            {userIsManager && configsButton}
          </>
        )}
        {/* Platform announcements reach every signed-in user on every screen; the
            status page renders its own copy, so it is skipped there. */}
        {isAuthenticated && !onStatusPage && <AnnouncementBanner />}
        <Routes>
          {getRoutes(enabledRoutes)}
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
          </HelpProvider>
          </FeaturesContext.Provider>
          )}
        </ErrorBoundary>
        {loading && (
          <div style={{
            position: 'fixed',
            top: 0,
            left: 0,
            width: '100%',
            height: '100%',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            backgroundColor: 'rgba(0, 0, 0, 0.5)',
            zIndex: 9999,
          }}>
            <ClipLoader color="#00BFFF" loading={loading} size={150} />
          </div>
        )}
      </ThemeProvider>
    </LoadingContext.Provider>
  );
}
