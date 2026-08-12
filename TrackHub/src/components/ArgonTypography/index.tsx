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
import { forwardRef } from "react";
import type { ReactNode } from "react";
import type { TypographyProps } from "@mui/material/Typography";

// Custom styles for ArgonTypography
import ArgonTypographyRoot from "components/ArgonTypography/ArgonTypographyRoot";

// Argon Dashboard 2 MUI context
import { useArgonController } from "context";

export interface ArgonTypographyProps
  extends Omit<TypographyProps, "color" | "fontWeight" | "textTransform"> {
  color?:
    | "inherit"
    | "primary"
    | "secondary"
    | "info"
    | "success"
    | "warning"
    | "error"
    | "light"
    | "dark"
    | "text"
    | "white";
  fontWeight?: false | "light" | "regular" | "medium" | "bold";
  textTransform?: "none" | "capitalize" | "uppercase" | "lowercase";
  verticalAlign?:
    | "unset"
    | "baseline"
    | "sub"
    | "super"
    | "text-top"
    | "text-bottom"
    | "middle"
    | "top"
    | "bottom";
  textGradient?: boolean;
  opacity?: number;
  /**
   * Anchor target when rendered polymorphically as a link (`component="a"`).
   * The interface extends TypographyProps non-polymorphically, so anchor
   * attributes must be declared explicitly.
   */
  href?: string;
  children: ReactNode;
}

const ArgonTypography = forwardRef<HTMLSpanElement, ArgonTypographyProps>(
  (
    {
      color = "dark",
      fontWeight = false,
      textTransform = "none",
      verticalAlign = "unset",
      textGradient = false,
      opacity = 1,
      children,
      ...rest
    },
    ref
  ) => {
    const [controller] = useArgonController();
    const { darkMode } = controller;

    return (
      <ArgonTypographyRoot
        {...rest}
        ref={ref}
        ownerState={{
          color,
          textTransform,
          verticalAlign,
          fontWeight,
          opacity,
          textGradient,
          darkMode,
        }}
      >
        {children}
      </ArgonTypographyRoot>
    );
  }
);

export default ArgonTypography;
