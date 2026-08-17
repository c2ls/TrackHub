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

// react-router-dom components
import { useLocation, NavLink } from "react-router-dom";

// @mui material components
import List from "@mui/material/List";
import Divider from "@mui/material/Divider";
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
import { useTranslation } from "react-i18next";
import PrincipalTypes from "constants/principalTypes";
import type { RouteDefinition } from "routes";
import type { CurrentPrincipal } from "api/manager/principals";

export interface SidenavProps {
  color?: "primary" | "secondary" | "info" | "success" | "warning" | "error" | "dark";
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
  const itemName = pathname.split("/").slice(1)[0];
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

  const keysToFilterOut = ["authorize-redirect", "callback"];
  // Render all the routes (except keysToFilterOut) from the routes.js
  const renderRoutes = routes
    .filter(({ key }) => !keysToFilterOut.includes(key))
    .filter((route) => {
      // Public routes are visible to every principal type (see RouteDefinition.public).
      if (route.public) return true;
      if (!currentPrincipal?.principalType) return true;
      const allowedPrincipalTypes = route.principalTypes || [PrincipalTypes.User];
      return allowedPrincipalTypes.includes(currentPrincipal.principalType);
    })
    .map(({ type, name, icon, title, key, href, route }) => {
      let returnValue: ReactNode;

      if (type === "route") {
        if (href) {
          returnValue = (
            <Link href={href} key={key} target="_blank" rel="noreferrer">
              <SidenavItem name={t(name as never)} icon={icon} active={key === itemName} />
            </Link>
          );
        } else {
          returnValue = (
            <NavLink to={route!} key={key}>
              <SidenavItem name={t(name as never)} icon={icon} active={key === itemName} />
            </NavLink>
          );
        }
      } else if (type === "title") {
        returnValue = (
          <ArgonTypography
            key={key}
            color={darkSidenav ? "white" : "dark"}
            display="block"
            variant="caption"
            fontWeight="bold"
            textTransform="uppercase"
            opacity={0.6}
            pl={3}
            mt={2}
            mb={1}
            ml={1}
          >
            {t(title as never)}
          </ArgonTypography>
        );
      } else if (type === "divider") {
        returnValue = <Divider key={key} light={darkSidenav} />;
      }

      if (key === "systemAdmin" && !isAdmin) {
        return null;
      } else if ((key === "manageAdmin" || key === "gpsIntegration") && !isManager) {
        return null;
      } else {
        return returnValue;
      }
    });

  const brandLinkProps = { component: NavLink, to: "/" };
  const brandImgProps = { component: "img" as const, src: brand, alt: "Argon Logo" };

  return (
    <SidenavRoot {...rest} variant="permanent" ownerState={{ darkSidenav, miniSidenav, layout }}>
      <ArgonBox pt={3} pb={1} px={4} textAlign="center">
        <ArgonBox
          display={{ xs: "block", xl: "none" }}
          position="absolute"
          top={0}
          right={0}
          p={1.625}
          onClick={closeSidenav}
          sx={{ cursor: "pointer" }}
        >
          <ArgonTypography variant="h6" color="secondary">
            <Icon sx={{ fontWeight: "bold" }}>close</Icon>
          </ArgonTypography>
        </ArgonBox>
        <ArgonBox {...brandLinkProps} display="flex" alignItems="center">
          {brand && <ArgonBox {...brandImgProps} width="3rem" mr={0.25} />}
          <ArgonBox
            width={!brandName ? "100%" : undefined}
            sx={(theme) => sidenavLogoLabel(theme, { miniSidenav })}
          >
            <ArgonTypography
              component="h6"
              variant="button"
              fontWeight="medium"
              color={darkSidenav ? "white" : "dark"}
            >
              {brandName}
            </ArgonTypography>
          </ArgonBox>
        </ArgonBox>
      </ArgonBox>
      <Divider light={darkSidenav} />
      <List>{renderRoutes}</List>
    </SidenavRoot>
  );
}

export default Sidenav;
