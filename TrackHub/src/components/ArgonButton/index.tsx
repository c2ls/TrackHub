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
* Argon Dashboard 2 MUI - v1.0.0
=========================================================

* Product Page: https://www.creative-tim.com/product/argon-dashboard-material-ui
* Copyright 2021 Creative Tim (https://www.creative-tim.com)

Coded by www.creative-tim.com

=========================================================

* The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
*/

import { forwardRef } from "react";
import type { ReactNode } from "react";
import type { ButtonProps } from "@mui/material/Button";

type MuiButtonSize = ButtonProps["size"];

// Custom styles for ArgonButton
import ArgonButtonRoot from "components/ArgonButton/ArgonButtonRoot";

export interface ArgonButtonProps extends Omit<ButtonProps, "color" | "variant" | "size"> {
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
  variant?: "text" | "contained" | "outlined" | "gradient";
  size?: "xsmall" | "small" | "medium" | "large";
  /**
   * react-router target, valid when `component` is a router `Link`. Declared on
   * the control (rather than cast at each call site) per the portal's rule 13.
   */
  to?: string;
  circular?: boolean;
  iconOnly?: boolean;
  children: ReactNode;
}

const ArgonButton = forwardRef<HTMLButtonElement, ArgonButtonProps>(
  (
    {
      color = "white",
      variant = "contained",
      size = "medium",
      circular = false,
      iconOnly = false,
      children,
      ...rest
    },
    ref
  ) => (
    <ArgonButtonRoot
      {...rest}
      ref={ref}
      color="primary"
      variant={variant === "gradient" ? "contained" : variant}
      // "xsmall" is an Argon-only size passed through to the underlying MUI Button
      // verbatim so the generated size className is preserved byte-for-byte.
      size={size as unknown as MuiButtonSize}
      ownerState={{ color, variant, size, circular, iconOnly }}
    >
      {children}
    </ArgonButtonRoot>
  )
);

export default ArgonButton;
