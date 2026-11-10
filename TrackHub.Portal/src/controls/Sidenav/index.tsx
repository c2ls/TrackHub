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

import { useEffect } from "react";
import type { MouseEventHandler, ReactNode } from "react";

// react-router components
import { useLocation, NavLink } from "react-router";

// @mui material components
import List from "@mui/material/List";
import Link from "@mui/material/Link";
import Icon from "@mui/material/Icon";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";

// Argon Dashboard 2 MUI example components
import SidenavItem from "controls/Sidenav/SidenavItem";

// Custom styles for the Sidenav
import SidenavRoot from "controls/Sidenav/SidenavRoot";
import sidenavLogoLabel from "controls/Sidenav/styles/sidenav";

// Argon Dashboard 2 MUI context
import { useArgonController, setMiniSidenav } from "context";
import { activateOnKeyboard } from 'utils/keyboard';
import { useTranslation } from "react-i18next";
import PrincipalTypes from "constants/principalTypes";
import type { RouteDefinition } from "routes";
import type { CurrentPrincipal } from "api/manager/principals";

export interface SidenavProps {
  brand?: string;
  isAdmin?: boolean;
  isManager?: boolean;
  currentPrincipal?: CurrentPrincipal | null;
  brandName: string;
  routes: RouteDefinition[];
  onMouseEnter?: MouseEventHandler;
  onMouseLeave?: MouseEventHandler;
}

function Sidenav({
  brand = "",
  isAdmin = false,
  isManager = false,
  currentPrincipal = null,
  brandName,
  routes,
  ...rest
}: SidenavProps) {
  const [controller, dispatch] = useArgonController();
  const { miniSidenav, darkSidenav, layout } = controller;
  const location = useLocation();
  const { pathname } = location;
  const { t } = useTranslation();

  const closeSidenav = () => setMiniSidenav(dispatch, true);

  useEffect(() => {
    // A function that sets the mini state of the sidenav.
    function handleMiniSidenav() {
      setMiniSidenav(dispatch, window.innerWidth < 1200);
    }
    /**
     The event listener that's calling the handleMiniSidenav function when resizing the window.
    */
    window.addEventListener("resize", handleMiniSidenav);

    // Call the handleMiniSidenav function to set the state with the initial value.
    handleMiniSidenav();

    // Remove event listener on cleanup
    return () => window.removeEventListener("resize", handleMiniSidenav);
  }, [dispatch, location]);

  // `tripTracking` is a real route (it needs a help topic and a <Route>), but it
  // is an anonymous customer page reached only through a shared link token — a
  // Sidenav entry would lead a portal user to an "invalid link" screen.
  const keysToFilterOut = ["authorize-redirect", "callback", "tripTracking"];

  const roleAllowed = (key: string): boolean => {
    if (key === "systemAdmin") return isAdmin;
    if (key === "manageAdmin" || key === "gpsIntegration") return isManager;
    return true;
  };

  const visibleRoutes = routes
    .filter(({ key, type }) => !keysToFilterOut.includes(key) && (type === "route" || type === "title"))
    .filter((route) => {
      if (route.type === "title") return true;
      if (!roleAllowed(route.key)) return false;
      // Public routes are visible to every principal type (see RouteDefinition.public).
      if (route.public) return true;
      if (!currentPrincipal?.principalType) return true;
      const allowedPrincipalTypes = route.principalTypes || [PrincipalTypes.User];
      return allowedPrincipalTypes.includes(currentPrincipal.principalType);
    })
    // A group heading with nothing under it for this user is dropped.
    .filter((route, index, all) => route.type !== "title" || all[index + 1]?.type === "route");

  const renderRoutes = visibleRoutes.map(({ type, name, icon, title, key, href, route }): ReactNode => {
    if (type === "title") {
      return (
        <ArgonTypography
          key={key}
          component="li"
          display={miniSidenav ? { xs: "block", xl: "none" } : "block"}
          variant="caption"
          fontWeight="bold"
          textTransform="uppercase"
          color="secondary"
          sx={{ letterSpacing: "0.06em", px: 2.75, pt: 2.5, pb: 0.75 }}
        >
          {t(title as never)}
        </ArgonTypography>
      );
    }

    const active = route === pathname;
    return href ? (
      <Link href={href} key={key} target="_blank" rel="noreferrer">
        <SidenavItem name={t(name as never)} icon={icon} active={active} />
      </Link>
    ) : (
      <NavLink to={route!} key={key} data-testid={`nav-${key}`}>
        <SidenavItem name={t(name as never)} icon={icon} active={active} />
      </NavLink>
    );
  });

  const brandLinkProps = { component: NavLink, to: "/" };
  const brandImgProps = { component: "img" as const, src: brand, alt: t("sidenav.brandAlt") };

  return (
    <SidenavRoot {...rest} variant="permanent" ownerState={{ darkSidenav, miniSidenav, layout }}>
      <ArgonBox
        display="flex"
        alignItems="center"
        justifyContent={miniSidenav ? { xs: "flex-start", xl: "center" } : "flex-start"}
        minHeight="3.5rem"
        px={2.25}
        sx={({ borders }) => ({
          borderBottom: `${borders.borderWidth[1]} solid ${borders.borderColor}`,
        })}
      >
        <ArgonBox {...brandLinkProps} display="flex" alignItems="center">
          {brand && <ArgonBox {...brandImgProps} width="2rem" />}
          <ArgonBox sx={(theme) => sidenavLogoLabel(theme, { miniSidenav })}>
            <ArgonTypography component="span" variant="h6" fontWeight="bold" color="dark">
              {brandName}
            </ArgonTypography>
          </ArgonBox>
        </ArgonBox>
        <ArgonBox
          display={{ xs: "block", xl: "none" }}
          ml="auto"
          lineHeight={0}
          role="button"
          tabIndex={0}
          aria-label={t("generic.close")}
          onClick={closeSidenav}
          onKeyDown={activateOnKeyboard(closeSidenav)}
          sx={{ cursor: "pointer" }}
        >
          <ArgonTypography variant="h6" color="secondary" lineHeight={0}>
            <Icon>close</Icon>
          </ArgonTypography>
        </ArgonBox>
      </ArgonBox>
      <List sx={{ pb: 2 }}>{renderRoutes}</List>
    </SidenavRoot>
  );
}

export default Sidenav;
