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
import type { InputBaseProps } from "@mui/material/InputBase";

// Custom styles for ArgonInput
import ArgonInputRoot from "components/ArgonInput/ArgonInputRoot";

// Argon Dashboard 2 MUI context
import { useArgonController } from "context";

export interface ArgonInputProps extends Omit<InputBaseProps, "size"> {
  size?: "small" | "medium" | "large";
  error?: boolean;
  success?: boolean;
  disabled?: boolean;
}

const ArgonInput = forwardRef<HTMLDivElement, ArgonInputProps>(
  ({ size = "medium", error = false, success = false, disabled = false, ...rest }, ref) => {
    const [controller] = useArgonController();
    const { darkMode } = controller;

    return (
      <ArgonInputRoot
        {...rest}
        ref={ref}
        ownerState={{ size, error, success, disabled, darkMode }}
      />
    );
  }
);

export default ArgonInput;
