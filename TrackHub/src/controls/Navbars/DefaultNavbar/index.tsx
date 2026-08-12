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

/* eslint-disable no-param-reassign */
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

import { useState, useEffect } from "react";
import type { MouseEvent } from "react";

// react-router components
import { Link } from "react-router-dom";

// @mui material components
import Icon from "@mui/material/Icon";
import Container from "@mui/material/Container";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import ArgonTypography from "components/ArgonTypography";
import ArgonButton from "components/ArgonButton";

// Argon Dashboard 2 MUI controls components
import DefaultNavbarLink from "controls/Navbars/DefaultNavbar/DefaultNavbarLink";
import DefaultNavbarMobile from "controls/Navbars/DefaultNavbar/DefaultNavbarMobile";

// Argon Dashboard 2 MUI Base Styles
import breakpoints from "assets/theme/base/breakpoints";

// Material Dashboard 2 PRO React context
import { useArgonController } from "context";

export interface DefaultNavbarAction {
  type: "external" | "internal";
  route: string;
  variant?: "contained" | "outlined" | "gradient";
  color?:
    | "white"
    | "primary"
    | "secondary"
    | "info"
    | "success"
    | "warning"
    | "error"
    | "light"
    | "dark";
  label: string;
}

export interface DefaultNavbarProps {
  brand?: string;
  transparent?: boolean;
  light?: boolean;
  action?: DefaultNavbarAction | false;
}

function DefaultNavbar({
  brand = "Argon Dashboard 2",
  transparent = false,
  light = false,
  action = false,
}: DefaultNavbarProps) {
  const [controller] = useArgonController();
  const { darkMode } = controller;
  const [mobileNavbar, setMobileNavbar] = useState<ParentNode | boolean | null>(false);
  const [mobileView, setMobileView] = useState(false);

  const openMobileNavbar = (event: MouseEvent<HTMLElement>) =>
    setMobileNavbar(event.currentTarget.parentNode);
  const closeMobileNavbar = () => setMobileNavbar(false);

  useEffect(() => {
    // A function that sets the display state for the DefaultNavbarMobile.
    function displayMobileNavbar() {
      if (window.innerWidth < breakpoints.values.lg) {
        setMobileView(true);
        setMobileNavbar(false);
      } else {
        setMobileView(false);
        setMobileNavbar(false);
      }
    }

    /**
     The event listener that's calling the displayMobileNavbar function when
     resizing the window.
    */
    window.addEventListener("resize", displayMobileNavbar);

    // Call the displayMobileNavbar function to set the state with the initial value.
    displayMobileNavbar();

    // Remove event listener on cleanup
    return () => window.removeEventListener("resize", displayMobileNavbar);
  }, []);

  const brandLinkProps = { component: Link, to: "/" };
  const actionInternalProps = action ? { component: Link, to: action.route } : {};
  const actionExternalProps = action
    ? { component: "a" as const, href: action.route, target: "_blank", rel: "noreferrer" }
    : {};

  return (
    <Container>
      <ArgonBox
        pt={0.75}
        pb={1}
        px={{ xs: 4, sm: transparent ? 2 : 3, lg: transparent ? 0 : 2 }}
        my={2}
        mx={3}
        width="calc(100% - 48px)"
        borderRadius="lg"
        shadow={transparent ? "none" : "md"}
        color={light ? "white" : "dark"}
        position="absolute"
        left={0}
        zIndex={99}
        sx={(theme) => {
          const { transparent: transparentColor, white } = theme.palette;
          const background = theme.palette.background as typeof theme.palette.background & {
            dark: string;
          };
          const { rgba } = theme.functions;
          return {
            backgroundColor: transparent
              ? transparentColor.main
              : rgba(darkMode ? background.dark : white.main, 0.8),
            backdropFilter: transparent ? "none" : `saturate(200%) blur(30px)`,
          };
        }}
      >
        <ArgonBox display="flex" justifyContent="space-between" alignItems="center" px={2}>
          <ArgonBox {...brandLinkProps} py={transparent ? 1.5 : 0.75} lineHeight={1}>
            <ArgonTypography variant="button" fontWeight="bold" color={light ? "white" : "dark"}>
              {brand}
            </ArgonTypography>
          </ArgonBox>
          <ArgonBox color="inherit" display={{ xs: "none", lg: "flex" }} m={0} p={0}>
            <DefaultNavbarLink
              icon="donut_large"
              name="dashboard"
              route="/dashboard"
              light={light}
            />
            <DefaultNavbarLink icon="person" name="profile" route="/profile" light={light} />
            <DefaultNavbarLink
              icon="account_circle"
              name="sign up"
              route="/authentication/sign-up"
              light={light}
            />
            <DefaultNavbarLink
              icon="key"
              name="sign in"
              route="/authentication/sign-in"
              light={light}
            />
          </ArgonBox>
          {action &&
            (action.type === "internal" ? (
              <ArgonBox display={{ xs: "none", lg: "inline-block" }}>
                <ArgonButton
                  {...actionInternalProps}
                  variant={action.variant ? action.variant : "contained"}
                  color={action.color ? action.color : "info"}
                  size="small"
                >
                  {action.label}
                </ArgonButton>
              </ArgonBox>
            ) : (
              <ArgonBox display={{ xs: "none", lg: "inline-block" }}>
                <ArgonButton
                  {...actionExternalProps}
                  variant={action.variant ? action.variant : "contained"}
                  color={action.color ? action.color : "info"}
                  size="small"
                  sx={{ mt: -0.3 }}
                >
                  {action.label}
                </ArgonButton>
              </ArgonBox>
            ))}
          <ArgonBox
            display={{ xs: "inline-block", lg: "none" }}
            lineHeight={0}
            py={1.5}
            pl={1.5}
            color="inherit"
            sx={{ cursor: "pointer" }}
            onClick={openMobileNavbar}
          >
            <Icon fontSize={"default" as never}>{mobileNavbar ? "close" : "menu"}</Icon>
          </ArgonBox>
        </ArgonBox>
      </ArgonBox>
      {mobileView && <DefaultNavbarMobile open={mobileNavbar} close={closeMobileNavbar} />}
    </Container>
  );
}

export default DefaultNavbar;
