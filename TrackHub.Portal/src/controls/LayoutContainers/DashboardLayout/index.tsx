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
import type { ReactNode } from "react";

// react-router-dom components
import { useLocation } from "react-router-dom";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";
import type { ArgonBoxProps } from "components/ArgonBox";

// Argon Dashboard 2 MUI context
import { useArgonController, setLayout } from "context";

export interface DashboardLayoutProps {
  bgColor?: string;
  /**
   * Styles for the page-background box (the 300px header band). Spread onto the
   * inner background ArgonBox via `...rest`, so callers can set gradient/image
   * backgrounds that `bgColor` cannot express (e.g. profile's cover photo).
   */
  sx?: ArgonBoxProps["sx"];
  children: ReactNode;
}

function DashboardLayout({ bgColor, children, ...rest }: DashboardLayoutProps) {
  const [controller, dispatch] = useArgonController();
  const { miniSidenav, darkMode } = controller;
  const { pathname } = useLocation();

  useEffect(() => {
    setLayout(dispatch, "dashboard");
  }, [pathname]);

  const background = darkMode && !bgColor ? "transparent" : bgColor;

  return (
    <ArgonBox
      sx={({ breakpoints, transitions, functions: { pxToRem } }) => ({
        p: 3,

        [breakpoints.up("xl")]: {
          marginLeft: miniSidenav ? pxToRem(120) : pxToRem(274),
          transition: transitions.create(["margin-left", "margin-right"], {
            easing: transitions.easing.easeInOut,
            duration: transitions.duration.standard,
          }),
        },
      })}
    >
      {/* Full-bleed header band. Anchored left/right instead of width:100vw — 100vw includes the
          vertical scrollbar width and forced a browser-level horizontal scrollbar on every
          vertically-scrolling page. */}
      <ArgonBox
        bgColor={background || "info"}
        height="300px"
        position="absolute"
        top={0}
        left={0}
        right={0}
        sx={darkMode ? { bgColor: (theme) => theme.palette.background.default } : undefined}
        {...rest}
      />
      {children}
    </ArgonBox>
  );
}

export default DashboardLayout;
