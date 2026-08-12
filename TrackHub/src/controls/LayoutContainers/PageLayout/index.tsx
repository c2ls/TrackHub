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

// Argon Dashboard 2 MUI context
import { useArgonController, setLayout } from "context";

export interface PageLayoutProps {
  background?: "white" | "light" | "default";
  children: ReactNode;
}

function PageLayout({ background = "default", children }: PageLayoutProps) {
  const [controller, dispatch] = useArgonController();
  const { darkMode } = controller;

  const { pathname } = useLocation();

  useEffect(() => {
    setLayout(dispatch, "page");
  }, [pathname]);

  return (
    <ArgonBox
      width="100vw"
      height="100%"
      minHeight="100vh"
      bgColor={darkMode ? "transparent" : background}
      sx={{ overflowX: "hidden" }}
    >
      {children}
    </ArgonBox>
  );
}

export default PageLayout;
