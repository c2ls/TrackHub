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

import { useState } from "react";
import type { ReactNode } from "react";

// @mui material components
import Fade from "@mui/material/Fade";
import type { BoxProps } from "@mui/material/Box";

// Argon Dashboard 2 MUI components
import ArgonBox from "components/ArgonBox";

// Custom styles for the ArgonAlert
import ArgonAlertRoot from "components/ArgonAlert/ArgonAlertRoot";
import ArgonAlertCloseIcon from "components/ArgonAlert/ArgonAlertCloseIcon";

export interface ArgonAlertProps extends Omit<BoxProps, "color"> {
  color?:
    | "primary"
    | "secondary"
    | "info"
    | "success"
    | "warning"
    | "error"
    | "light"
    | "dark";
  dismissible?: boolean;
  children: ReactNode;
}

function ArgonAlert({ color = "info", dismissible = false, children, ...rest }: ArgonAlertProps) {
  const [alertStatus, setAlertStatus] = useState("mount");

  const handleAlertStatus = () => setAlertStatus("fadeOut");

  // The base template for the alert
  const alertTemplate = (mount = true) => (
    <Fade in={mount} timeout={300}>
      <ArgonAlertRoot ownerState={{ color }} {...rest}>
        <ArgonBox display="flex" alignItems="center" color="white">
          {children}
        </ArgonBox>
        {dismissible ? (
          <ArgonAlertCloseIcon onClick={mount ? handleAlertStatus : undefined}>
            &times;
          </ArgonAlertCloseIcon>
        ) : null}
      </ArgonAlertRoot>
    </Fade>
  );

  switch (true) {
    case alertStatus === "mount":
      return alertTemplate();
    case alertStatus === "fadeOut":
      setTimeout(() => setAlertStatus("unmount"), 400);
      return alertTemplate(false);
    default:
      alertTemplate();
      break;
  }

  return null;
}

export default ArgonAlert;
