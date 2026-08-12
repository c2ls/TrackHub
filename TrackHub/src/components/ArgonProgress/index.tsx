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
import type { LinearProgressProps } from "@mui/material/LinearProgress";

// Argon Dashboard 2 MUI components
import ArgonTypography from "components/ArgonTypography";

// Custom styles for ArgonProgress
import ArgonProgressRoot from "components/ArgonProgress/ArgonProgressRoot";

export interface ArgonProgressProps extends Omit<LinearProgressProps, "variant" | "color"> {
  variant?: "contained" | "gradient";
  color?:
    | "primary"
    | "secondary"
    | "info"
    | "success"
    | "warning"
    | "error"
    | "light"
    | "dark";
  value?: number;
  label?: boolean;
}

const ArgonProgress = forwardRef<HTMLSpanElement, ArgonProgressProps>(
  ({ variant = "contained", color = "info", value = 0, label = false, ...rest }, ref) => (
    <>
      {label && (
        <ArgonTypography variant="button" fontWeight="medium" color="text">
          {value}%
        </ArgonTypography>
      )}
      <ArgonProgressRoot
        {...rest}
        ref={ref}
        variant="determinate"
        value={value}
        ownerState={{ color, value, variant }}
      />
    </>
  )
);

export default ArgonProgress;
