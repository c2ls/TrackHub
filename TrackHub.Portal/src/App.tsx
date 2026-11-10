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
import { useState, useEffect, useMemo, useCallback, Suspense } from 'react';
import type { ReactNode } from 'react';

// react-router components
import { Routes, Route, Navigate, useLocation } from "react-router";

// @mui material components
import { ThemeProvider } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import GlobalStyles from "@mui/material/GlobalStyles";

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
import { isDarkStyle, isMiniSidenav, writeUiPreferences } from "utils/uiPreferences";
import { useTranslation } from 'react-i18next';
import ErrorBoundary from "components/ErrorBoundary";
import SuspensionScreen from "components/SuspensionScreen";
import PrincipalTypes from "constants/principalTypes";
import HelpProvider from "context/help";
import { CurrentAccountContext } from "context/account";
import { FeaturesContext, isFeatureActive } from "context/features";
import { PermissionsContext, buildPermissionIndex } from "context/permissions";
import { getAuthorizedActions } from "api/security/permissions";
import type { AuthorizedAction } from "api/security/permissions";
import AnnouncementBanner from "components/AnnouncementBanner";

/**
 * Public, chrome-less routes (see routes.tsx `platformStatus` / `tripTracking`).
 * Both are reachable signed-out and render their own full-page shell, so the
 * Sidenav, Configurator and announcement banner are all suppressed on them.
 */
const PUBLIC_ROUTES = ["/status", "/track"] as const;

export default function App() {
  const [controller, dispatch] = useArgonController();
  const { miniSidenav, layout, openConfigurator, darkMode } = controller;
  const [onMouseEnter, setOnMouseEnter] = useState(false);
  const { isAuthenticated, login, isLoggingIn, authError } = useAuth();
  const { pathname } = useLocation();
  const { i18n } = useTranslation();

  const saveAccountSettings = async (
    accountId: string,
    settings: Omit<AccountSettingsDtoInput, 'accountId'>
  ) => {
    await updateAccountSettings(accountId, settings);
    setAccountSettings(await getAccountSettings());
  };

  const [loading, setLoading] = useState(false);
  // Closed until bootstrap answers. Open defaults briefly showed every signed-in user the admin
  // consoles, which then fired their privileged queries and filled the screen with rejections.
  const [userIsAdmin, setUserIsAdmin] = useState(false);
  const [userIsManager, setUserIsManager] = useState(false);
  const [bootstrapLoaded, setBootstrapLoaded] = useState(false);
  const [accountSettings, setAccountSettings] = useState<Partial<AccountSettings>>({});
  const [accountFeatures, setAccountFeatures] = useState<AccountContext['features']>([]);
  const [featuresLoaded, setFeaturesLoaded] = useState(false);
  const [accountTimeZoneId, setAccountTimeZoneId] = useState<string | null>(null);
  const [authorizedActions, setAuthorizedActions] = useState<AuthorizedAction[]>([]);
  const [permissionsLoaded, setPermissionsLoaded] = useState(false);
  const [accountStatus, setAccountStatus] = useState<AccountStatus | null>(null);
  const [branding, setBranding] = useState<AccountContext['branding'] | null>(null);
  const [currentPrincipal, setCurrentPrincipal] = useState<CurrentPrincipal | null>(null);

  useEffect(() => {
    // Redirect to login page if not authenticated
    // Skip redirect if: on callback page, already logging in, on error page, or auth error occurred
    const isAuthRoute = pathname.startsWith("/authentication/");
    const isErrorPage = pathname === "/error";
    // The public pages must never bounce to the login flow: the status page's
    // whole purpose is to answer "can nobody sign in?", and a customer following
    // a trip tracking link has no account to sign in with at all.
    const isPublicPage = (PUBLIC_ROUTES as readonly string[]).includes(pathname);

    if (!isAuthenticated && !isLoggingIn && !isAuthRoute && !isErrorPage && !isPublicPage && !authError) {
      login();
    }

  }, [isAuthenticated, isLoggingIn, login, pathname, authError]);

  useEffect(() => {
    const fetchPermissions = async () => {
      if (!isAuthenticated) return;

      // Appearance first, and on its own round trip: every other bootstrap read can
      // land late without the user noticing, but the style has to be applied before
      // the shell paints or a dark-profile user watches the screen flip from light.
      // The result is mirrored to localStorage so the *next* sign-in paints correctly
      // from the first frame (see utils/uiPreferences).
      const userSettings = await getUserSettings().catch(() => null);
      if (userSettings) {
        setDarkMode(dispatch, isDarkStyle(userSettings.style));
        setDarkSidenav(dispatch, isDarkStyle(userSettings.style));
        setMiniSidenav(dispatch, isMiniSidenav(userSettings.navbar));
        if (userSettings.language) {
          i18n.changeLanguage(userSettings.language);
        }
        writeUiPreferences(userSettings);
      }

      // The rest of the bootstrap is independent — fan out instead of chaining five
      // sequential round trips behind each other.
      // Graceful fallbacks on failure (matching the old services' silent handling):
      // null principal ⇒ routes fall back to the default principal type, false roles
      // ⇒ admin/manager routes stay locked down, no toast either way.
      const [principal, admin, manager, settings, context] = await Promise.all([
        getCurrentPrincipal().catch(() => null),
        isAdmin().catch(() => false),
        isManager().catch(() => false),
        getAccountSettings().catch(() => null),
        // Single bootstrap read (status + branding + features); allowed on non-operational accounts
        // so the shell can render a suspension state instead of issuing operational queries.
        getAccountContext().catch(() => null),
      ]);

      setCurrentPrincipal(principal);
      setUserIsAdmin(admin);
      setUserIsManager(manager);
      setBootstrapLoaded(true);

      // The caller's effective permission set (roles UNION policies), used to gate
      // actions the way `featureEnabled` gates screens. Needs the principal's user
      // id, so it follows the fan-out rather than joining it. A failure leaves the
      // set unloaded, and `can` then answers true — UI gating is UX only and the
      // backend re-checks every call, so degrading OPEN shows a toast on a denied
      // click instead of silently hiding controls the user actually has.
      if (principal?.userId) {
        const actions = await getAuthorizedActions(principal.userId).catch(() => null);
        if (actions) {
          setAuthorizedActions(actions);
          setPermissionsLoaded(true);
        }
      }
      if (settings) {
        setAccountSettings(settings);
      }
      if (context) {
        setAccountStatus(context.status);
        setBranding(context.branding);
        setAccountFeatures(context.features || []);
        setFeaturesLoaded(true);
        setAccountTimeZoneId(context.timeZoneId ?? null);
      }
    };

    // Only once the caller is authenticated: fetchPermissions returns immediately while signed out,
    // and releasing the gate on that pass would decide every gated route against role flags that have
    // not loaded — a deep link opened signed out was redirected away before sign-in even completed.
    if (!isAuthenticated) {
      return;
    }

    // Routing resumes whatever happens in there. Every read above already degrades to a null, but a
    // bootstrap that threw for some other reason must not leave gated routes rendering nothing at
    // all — with the role flags still closed, the worst case is a redirect to /dashboard.
    fetchPermissions().finally(() => setBootstrapLoaded(true));
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

  // Other screens write the same settings row; open the panel on its current version.
  useEffect(() => {
    if (!openConfigurator || !isAuthenticated) return;
    getAccountSettings().then(setAccountSettings).catch(() => undefined);
  }, [openConfigurator, isAuthenticated]);

  // The Argon template's RTL support was dropped in this fork: the controller
  // has no `direction` state, so the app is LTR-only. The old effect wrote the
  // invalid dir="undefined" to <body>; it is intentionally gone.

  // The snippet in index.html paints <html> from the mirrored preference before the
  // bundle loads; take it over here so a mid-session theme switch also moves the
  // backdrop and the native scrollbars/form controls with it.
  useEffect(() => {
    const activeTheme = darkMode ? themeDark : theme;
    document.documentElement.style.backgroundColor = activeTheme.palette.background.default;
    document.documentElement.style.colorScheme = darkMode ? "dark" : "light";
  }, [darkMode]);

  // Setting page scroll to 0 when changing the route
  useEffect(() => {
    document.documentElement.scrollTop = 0;
    if (document.scrollingElement) {
      document.scrollingElement.scrollTop = 0;
    }
  }, [pathname]);

  // Operational statuses (Trial/Active) permit normal access; anything else renders a suspension shell.
  const accountOperational = !accountStatus || accountStatus === 'TRIAL' || accountStatus === 'ACTIVE';

  // The public pages render chrome-less for everyone (signed in or not) and stay
  // reachable on a suspended account — the status page is the screen you check
  // when things break, and a customer's tracking link is not an account surface.
  const onPublicPage = (PUBLIC_ROUTES as readonly string[]).includes(pathname);

  // The auth error page and the authorize/callback interstitials render their own
  // full-page shell too — without this the Sidenav overlays the error card and
  // hides the message.
  const onChromelessPage = onPublicPage || pathname === "/error" || pathname.startsWith("/authentication/");

  // Shared with FeaturesContext consumers; matches the backend flag semantics
  // (missing row ⇒ disabled, effective window honoured).
  // Stable identities: these travel in context values, and a new function per render re-renders
  // every consumer on each loading toggle — of which the map refresh alone fires two.
  const featureEnabled = useCallback(
    (featureKey?: string | null): boolean => isFeatureActive(accountFeatures, featureKey),
    [accountFeatures]);

  // Permission gate for ACTIONS (the feature flags above gate SCREENS). Open until the
  // set has loaded — see the note on PermissionsContext.
  const permissionIndex = useMemo(() => buildPermissionIndex(authorizedActions), [authorizedActions]);
  const can = useCallback(
    (resource: string, action: string): boolean =>
      !permissionsLoaded || permissionIndex.has(`${resource}.${action}`),
    [permissionsLoaded, permissionIndex]);

  const filterRoutesByFeatures = (allRoutes: RouteDefinition[]): RouteDefinition[] =>
    allRoutes
      .filter(route => featureEnabled(route.featureKey))
      .map(route => route.collapse ? { ...route, collapse: filterRoutesByFeatures(route.collapse) } : route);

  const enabledRoutes = filterRoutesByFeatures(routes);

  // Reached BEFORE a session exists — the callback is what creates one — so these can never wait for
  // bootstrap: gate them and the code is never exchanged, isAuthenticated never flips, and the gate
  // never releases. Public routes are already exempt; they need no session at all.
  const PRE_SESSION_ROUTES = ['/authentication/callback', '/error'];
  const awaitsBootstrap = (route: RouteDefinition): boolean =>
    !route.public && !PRE_SESSION_ROUTES.includes(route.route ?? '');

  const routeAllowed = (route: RouteDefinition): boolean => {
    // Public routes bypass every gate. Without this a non-User principal (Staff, public link)
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
        // The ROLE-gated screens wait for bootstrap: their flags start closed, so deciding now would
        // bounce a deep link the user is entitled to. Nothing else may wait — every other route is
        // already allowed while the principal is null, and the sign-in callback is one of them: gate
        // it and it never mounts, never exchanges the code, and sign-in can never complete.
        const element = !awaitsBootstrap(route) || bootstrapLoaded
          ? (routeAllowed(route) ? route.component : <Navigate to="/dashboard" replace />)
          : null;

        return <Route path={route.route} element={element} key={route.key} />;
      }

      return null;
    });

  const loadingValue = useMemo(() => ({ loading, setLoading }), [loading]);
  const reloadAccountContext = useCallback(async () => {
    const context = await getAccountContext().catch(() => null);
    if (context) {
      setAccountStatus(context.status);
      setBranding(context.branding);
      setAccountFeatures(context.features || []);
      setAccountTimeZoneId(context.timeZoneId ?? null);
    }
  }, []);
  const featuresValue = useMemo(
    () => ({ features: accountFeatures, isFeatureEnabled: featureEnabled, reload: reloadAccountContext }),
    [accountFeatures, featureEnabled, reloadAccountContext]);
  const accountValue = useMemo(
    () => ({ timeZoneId: accountTimeZoneId, isManager: userIsManager }),
    [accountTimeZoneId, userIsManager]);
  const permissionsValue = useMemo(
    () => ({ actions: authorizedActions, can, loaded: permissionsLoaded }),
    [authorizedActions, can, permissionsLoaded]);

  return (
    <LoadingContext.Provider value={loadingValue}>
      <ThemeProvider theme={darkMode ? themeDark : theme}>
        <CssBaseline />
        {/* The page itself never scrolls laterally (full-bleed bands, sub-pixel grid rounding
            under browser zoom, etc. must clip); wide content scrolls inside its own container. */}
        <GlobalStyles styles={{ html: { overflowX: "clip" }, body: { overflowX: "clip" } }} />
        <ErrorBoundary>
          {isAuthenticated && !accountOperational && !onPublicPage ? (
            <SuspensionScreen status={accountStatus} branding={branding} />
          ) : (
          <CurrentAccountContext.Provider value={accountValue}>
          <FeaturesContext.Provider value={featuresValue}>
          <PermissionsContext.Provider value={permissionsValue}>
          <HelpProvider allowedScreens={allowedScreens} isFeatureEnabled={featureEnabled}>
          {layout === "dashboard" && !onChromelessPage && (
          <>
            <Sidenav
              brand={darkMode ? brand : brandDark}
              brandName="TrackHub"
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
          </>
        )}
        {/* Platform announcements reach every signed-in user on every screen; the
            status page renders its own copy, so it is skipped there. */}
        {isAuthenticated && !onChromelessPage && <AnnouncementBanner />}
        {/* Screens load on demand, so the router needs a boundary to render while one arrives. */}
        <Suspense fallback={null}>
          <Routes>
            {getRoutes(enabledRoutes)}
            {/* Feature-gated routes are absent until the account context loads; deciding the
                catch-all before that bounced every reload or deep link of such a screen to the
                dashboard. Signed-out visitors have no context to wait for. */}
            <Route
              path="*"
              element={isAuthenticated && !featuresLoaded ? null : <Navigate to="/dashboard" replace />}
            />
          </Routes>
        </Suspense>
          </HelpProvider>
          </PermissionsContext.Provider>
          </FeaturesContext.Provider>
          </CurrentAccountContext.Provider>
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
            <ClipLoader color={(darkMode ? themeDark : theme).palette.info.main} loading={loading} size={96} />
          </div>
        )}
      </ThemeProvider>
    </LoadingContext.Provider>
  );
}
